from contextlib import asynccontextmanager
from fastapi import FastAPI
from database import initialize_database


@asynccontextmanager
async def lifespan(app: FastAPI):
    initialize_database()
    yield


app = FastAPI(lifespan=lifespan)


@app.get("/health")
def check_health():
    return {"status": "ok", "service": "PausaVital Backend"}
