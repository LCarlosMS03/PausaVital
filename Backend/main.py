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

# Standard model to receive data from C#
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
    
    # Simple counting logic for MVP: Total completed breaks
    cursor.execute("SELECT COUNT(*) FROM event_logs WHERE status = 'completed'")
    count = cursor.fetchone()[0]
    conn.close()
    
    return {"current_streak": count}