import sqlite3
import sys
from pathlib import Path
import uvicorn
from contextlib import asynccontextmanager
from fastapi import FastAPI
from pydantic import BaseModel
from datetime import datetime

# DATA BASE
if getattr(sys, 'frozen', False):
    application_path = Path(sys.executable).parent
else:
    application_path = Path(__file__).resolve().parent

DATABASE_PATH = application_path / "pausavital_db.sqlite"

def get_db_connection():
    return sqlite3.connect(DATABASE_PATH, check_same_thread=False)

def initialize_database():
    conn = get_db_connection()
    cursor = conn.cursor()

    cursor.execute("""
        CREATE TABLE IF NOT EXISTS users (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            username TEXT UNIQUE NOT NULL,
            shields INTEGER NOT NULL DEFAULT 0
        )
    """)
    cursor.execute("""
        CREATE TABLE IF NOT EXISTS habits (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            title TEXT NOT NULL,
            category TEXT NOT NULL,
            periodicity TEXT NOT NULL
        )
    """)
    cursor.execute("SELECT COUNT(*) FROM habits")
    if cursor.fetchone()[0] == 0:
        cursor.execute("""
            INSERT INTO habits (title, category, periodicity) 
            VALUES ('Regla 20-20-20', 'Salud Visual', '20m')
        """)
    cursor.execute("""
        CREATE TABLE IF NOT EXISTS event_logs (
            log_id INTEGER PRIMARY KEY AUTOINCREMENT,
            user_id INTEGER NOT NULL,
            habit_id INTEGER NOT NULL,
            completed_at TIMESTAMP NOT NULL,
            status TEXT NOT NULL,
            FOREIGN KEY (user_id) REFERENCES users(id),
            FOREIGN KEY (habit_id) REFERENCES habits(id)
        )
    """)
    conn.commit()
    conn.close()

# LOCAL API
@asynccontextmanager
async def lifespan(app: FastAPI):
    initialize_database()
    yield

app = FastAPI(lifespan=lifespan)

class LoginRequest(BaseModel):
    username: str

class BreakLog(BaseModel):
    user_id: int
    habit_id: int
    status: str

@app.get("/health")
def check_health():
    return {"status": "ok", "service": "PausaVital Backend"}

@app.post("/auth/login")
def login(req: LoginRequest):
    conn = get_db_connection()
    cursor = conn.cursor()
    cursor.execute("SELECT id, username, shields FROM users WHERE username = ?", (req.username,))
    row = cursor.fetchone()
    if row is None:
        cursor.execute("INSERT INTO users (username, shields) VALUES (?, 0)", (req.username,))
        conn.commit()
        user_id = cursor.lastrowid
        shields = 0
    else:
        user_id = row[0]
        shields = row[2]
    conn.close()
    return {"user_id": user_id, "username": req.username, "shields": shields}

@app.get("/habits/default")
def get_default_habit():
    conn = get_db_connection()
    cursor = conn.cursor()
    cursor.execute("SELECT id FROM habits WHERE title = 'Regla 20-20-20' LIMIT 1")
    row = cursor.fetchone()
    conn.close()
    return {"habit_id": row[0] if row else 1}

@app.post("/logs/")
def record_break(log: BreakLog):
    conn = get_db_connection()
    cursor = conn.cursor()
    completed_at = datetime.utcnow().isoformat()
    cursor.execute(
        "INSERT INTO event_logs (user_id, habit_id, completed_at, status) VALUES (?, ?, ?, ?)",
        (log.user_id, log.habit_id, completed_at, log.status)
    )
    conn.commit()
    conn.close()
    return {"message": "Break recorded"}

@app.get("/streaks/{user_id}")
def get_current_streak(user_id: int):
    conn = get_db_connection()
    cursor = conn.cursor()
    cursor.execute("SELECT completed_at FROM event_logs WHERE user_id = ? AND status = 'failed' ORDER BY completed_at DESC LIMIT 1", (user_id,))
    last_fail = cursor.fetchone()
    if last_fail:
        cursor.execute("SELECT COUNT(*) FROM event_logs WHERE user_id = ? AND status = 'completed' AND completed_at > ?", (user_id, last_fail[0]))
    else:
        cursor.execute("SELECT COUNT(*) FROM event_logs WHERE user_id = ? AND status = 'completed'", (user_id,))
    count = cursor.fetchone()[0]
    conn.close()
    return {"current_streak": count}

@app.get("/shields/{user_id}")
def get_shields(user_id: int):
    conn = get_db_connection()
    cursor = conn.cursor()
    cursor.execute("SELECT shields FROM users WHERE id = ?", (user_id,))
    row = cursor.fetchone()
    conn.close()
    return {"user_id": user_id, "available_shields": row[0] if row else 0}

@app.post("/shields/{user_id}/consume")
def consume_shield(user_id: int):
    conn = get_db_connection()
    cursor = conn.cursor()
    cursor.execute("SELECT shields FROM users WHERE id = ?", (user_id,))
    row = cursor.fetchone()
    if row is None or row[0] <= 0:
        conn.close()
        return {"success": False}
    new_amount = row[0] - 1
    cursor.execute("UPDATE users SET shields = ? WHERE id = ?", (new_amount, user_id))
    conn.commit()
    conn.close()
    return {"success": True, "available_shields": new_amount}

@app.get("/stats/{user_id}")
def get_user_stats(user_id: int):
    conn = get_db_connection()
    cursor = conn.cursor()
    
    cursor.execute("SELECT COUNT(*) FROM event_logs WHERE user_id = ? AND status = 'completed'", (user_id,))
    completed = cursor.fetchone()[0]
    
    cursor.execute("SELECT COUNT(*) FROM event_logs WHERE user_id = ? AND status = 'failed'", (user_id,))
    failed = cursor.fetchone()[0]
    
    conn.close()
    
    total = completed + failed
    success_rate = (completed / total * 100) if total > 0 else 0
    
    return {
        "total_completed": completed,
        "total_failed": failed,
        "success_rate": round(success_rate, 1)
    }

# Run the app
if __name__ == "__main__":
    uvicorn.run(app, host="127.0.0.1", port=8000)