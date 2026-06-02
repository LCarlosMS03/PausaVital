import sqlite3

def get_db_connection():
    connection = sqlite3.connect("pausavital_db.sqlite", check_same_thread=False)
    return connection

def initialize_database():
    connection = get_db_connection()
    cursor = connection.cursor()

    cursor.execute("""
        CREATE TABLE IF NOT EXISTS habits (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            title TEXT NOT NULL,
            category TEXT NOT NULL,
            periodicity TEXT NOT NULL
        )
    """)

    cursor.execute("""
        CREATE TABLE IF NOT EXISTS event_logs (
            log_id INTEGER PRIMARY KEY AUTOINCREMENT,
            habit_id INTEGER,
            completed_at TIMESTAMP NOT NULL,
            status TEXT NOT NULL,
            FOREIGN KEY (habit_id) REFERENCES habits(id)
        )
    """)

    cursor.execute("""
        CREATE TABLE IF NOT EXISTS user_shields (
            user_id INTEGER PRIMARY KEY,
            available_shields INTEGER NOT NULL
        )
    """)

    connection.commit()
    connection.close()