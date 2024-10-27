import sqlite3
import json

class DBManager():
    def __init__(self, db_name = 'chat.db'):
        self.db_name = db_name

    def create_db(self):
        conn = sqlite3.connect(self.db_name)
        cursor = conn.cursor()
        cursor.execute('''
        CREATE TABLE IF NOT EXISTS chats (
            source TEXT NOT NULL,
            chat_id TEXT NOT NULL,
            status TEXT NOT NULL,
            type TEXT,
            priority TEXT,
            PRIMARY KEY (source, chat_id)
        )
        ''')
        cursor.execute('''
        CREATE TABLE IF NOT EXISTS messages (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            source TEXT NOT NULL,
            chat_source TEXT NOT NULL,
            chat_id TEXT NOT NULL,
            text TEXT NOT NULL,
            timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
            FOREIGN KEY (chat_source, chat_id) REFERENCES chats(source, chat_id)
        )
        ''')
        conn.commit()
        conn.close()

    def is_chat_exists(self, chat_source, chat_id):
        conn = sqlite3.connect(self.db_name)
        cursor = conn.cursor()
        query = "SELECT * FROM chats WHERE source = ? AND chat_id = ?"
        cursor.execute(query, (chat_source, chat_id))
        row = cursor.fetchone()
        conn.close()
        return row is not None

    def add_message(self, source, chat_source, chat_id, text):
        conn = sqlite3.connect(self.db_name)
        cursor = conn.cursor()
        cursor.execute('INSERT OR IGNORE INTO chats (source, chat_id, status) VALUES (?, ?, ?)', (chat_source, chat_id, 'awaiting'))
        cursor.execute('INSERT INTO messages (source, chat_source, chat_id, text) VALUES (?, ?, ?, ?)', (source, chat_source, chat_id, text))
        conn.commit()
        conn.close()

    def add_chat_type(self, source, chat_id, chat_type):
        conn = sqlite3.connect(self.db_name)
        cursor = conn.cursor()
        cursor.execute('UPDATE chats SET type = ? WHERE source = ? AND chat_id = ?', (chat_type, source, chat_id))
        conn.commit()
        conn.close()

    def change_status(self, source, chat_id, status):
        conn = sqlite3.connect(self.db_name)
        cursor = conn.cursor()
        cursor.execute('UPDATE chats SET status = ? WHERE source = ? AND chat_id = ?', (status, source, chat_id))
        conn.commit()
        conn.close()


    def get_chats(self):
        conn = sqlite3.connect(self.db_name)
        cursor = conn.cursor()
        cursor.execute("SELECT * FROM chats")
        rows = cursor.fetchall()
        column_names = [description[0] for description in cursor.description]
        data = []
        for row in rows:
            data.append(dict(zip(column_names, row)))

        json_data = json.dumps(data, indent=4)
        conn.close()
        return json_data

    def get_chat_messages(self, chat_source, chat_id):
        conn = sqlite3.connect(self.db_name)
        cursor = conn.cursor()
        query = "SELECT * FROM messages WHERE chat_source = ? AND chat_id = ? ORDER BY timestamp"
        cursor.execute(query, (chat_source, chat_id))
        rows = cursor.fetchall()
        column_names = [description[0] for description in cursor.description]
        data = []
        for row in rows:
            data.append(dict(zip(column_names, row)))
        json_data = json.dumps(data, indent=4)
        conn.close()
        return json_data

        