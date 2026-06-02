from contextlib import asynccontextmanager
from fastapi import FastAPI
from pydantic import BaseModel
from datetime import datetime
from database import initialize_database, get_db_connection

@asynccontextmanager
async def lifespan(app: FastAPI):
    initialize_database()
    yield

app = FastAPI(lifespan=lifespan)

class BreakLog(BaseModel):
    habit_id: int
    status: str

@app.get("/health")
def check_health():
    return {"status": "ok", "service": "PausaVital Backend"}

@app.post("/logs/")
def record_break(log: BreakLog):
    conn = get_db_connection()
    cursor = conn.cursor()
    completed_at = datetime.utcnow().isoformat()
    
    cursor.execute(
        "INSERT INTO event_logs (habit_id, completed_at, status) VALUES (?, ?, ?)",
        (log.habit_id, completed_at, log.status)
    )
    conn.commit()
    conn.close()
    return {"message": "Break recorded successfully"}

@app.get("/streaks/")
def get_current_streak():
    conn = get_db_connection()
    cursor = conn.cursor()
    
    # 1. Buscar cuándo fue la última vez que el usuario falló
    cursor.execute("SELECT completed_at FROM event_logs WHERE status = 'failed' ORDER BY completed_at DESC LIMIT 1")
    last_fail = cursor.fetchone()
    
    if last_fail:
        # 2. Si falló alguna vez, contar solo los éxitos DESPUÉS de ese fallo
        cursor.execute("SELECT COUNT(*) FROM event_logs WHERE status = 'completed' AND completed_at > ?", (last_fail[0],))
    else:
        # 3. Si nunca ha fallado, contar todos
        cursor.execute("SELECT COUNT(*) FROM event_logs WHERE status = 'completed'")
        
    count = cursor.fetchone()[0]
    conn.close()
    
    return {"current_streak": count}

@app.get("/shields/{user_id}")
def get_shields(user_id: int):
    conn = get_db_connection()
    cursor = conn.cursor()
    cursor.execute("SELECT available_shields FROM user_shields WHERE user_id = ?", (user_id,))
    row = cursor.fetchone()
    conn.close()
    
    if row is None:
        return {"user_id": user_id, "available_shields": 0}
    return {"user_id": user_id, "available_shields": row[0]}

@app.post("/shields/{user_id}/add")
def add_shield(user_id: int):
    conn = get_db_connection()
    cursor = conn.cursor()
    cursor.execute("SELECT available_shields FROM user_shields WHERE user_id = ?", (user_id,))
    row = cursor.fetchone()
    
    if row is None:
        new_amount = 1
        cursor.execute("INSERT INTO user_shields (user_id, available_shields) VALUES (?, ?)", (user_id, new_amount))
    else:
        new_amount = row[0] + 1
        cursor.execute("UPDATE user_shields SET available_shields = ? WHERE user_id = ?", (new_amount, user_id))
        
    conn.commit()
    conn.close()
    return {"message": "Shield added successfully", "available_shields": new_amount}

@app.post("/shields/{user_id}/consume")
def consume_shield(user_id: int):
    conn = get_db_connection()
    cursor = conn.cursor()
    cursor.execute("SELECT available_shields FROM user_shields WHERE user_id = ?", (user_id,))
    row = cursor.fetchone()
    
    if row is None or row[0] <= 0:
        conn.close()
        return {"success": False, "message": "No shields available to consume"}
        
    new_amount = row[0] - 1
    cursor.execute("UPDATE user_shields SET available_shields = ? WHERE user_id = ?", (new_amount, user_id))
    conn.commit()
    conn.close()
    
    return {"success": True, "message": "Shield consumed successfully", "available_shields": new_amount}