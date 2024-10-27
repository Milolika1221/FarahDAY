from flask import Flask, request, jsonify
import threading
import requests
from dbmanager import DBManager
import sys
sys.path.append('/task_classifier')
sys.path.append('/text_generation')
from task_classifier.task_classifier import TaskClassifier
from text_generation.text_generator import TextGenerator
import vk_api
from flask_socketio import SocketIO, emit
import smtplib
import email
from email.mime.text import MIMEText
from email.header import decode_header
import subprocess
import os

script_dir = os.path.dirname(os.path.abspath(__file__))
os.chdir(script_dir)

HOST = '127.0.0.1'
PORT = '5000'
if (len(sys.argv) > 1):
    HOST, PORT = sys.argv[1].split(":")
app = Flask(__name__)
socketio = SocketIO(app, cors_allowed_origins="*")
users_data = []
dbmanager = DBManager()
taskclassifier = TaskClassifier()
textgenerator = TextGenerator()
tg_bot_token = ''
tg_url = ''
vk_bot_token = ''
SMTP_SERVER = 'smtp.gmail.com'
SMTP_PORT = 587
EMAIL_ADDRESS = ''
APP_PASSWORD = ''

def save_user(chat_source, chat_id, name, message):
    bot_name = 'system'
    if (dbmanager.is_chat_exists(chat_source, chat_id)):
        dbmanager.add_message(name, chat_source, chat_id, message)
        socketio.emit(
            'new_message', 
            {'source': name, 'chat_source': chat_source, 'chat_id': chat_id, 'text': message}
        )
        dbmanager.change_status(chat_source, chat_id, 'awaiting')
    else:
        message_type = taskclassifier.get_predict(message)
        bot_answer = textgenerator.get_response(message)
        if chat_source == 'tg':
            send_user_telegram(chat_id, bot_answer)
        elif chat_source == 'vk':
            send_user_vk(chat_id, bot_answer)
        elif chat_source == 'mail':
            send_user_gmail(chat_id, bot_answer)
        dbmanager.add_message(name, chat_source, chat_id, message)
        dbmanager.add_chat_type(chat_source, chat_id, message_type)
        dbmanager.add_message(bot_name, chat_source, chat_id, bot_answer)
        dbmanager.change_status(chat_source, chat_id, 'open')
        socketio.emit(
            'new_message', 
            {'source': bot_name, 'chat_source': chat_source, 'chat_id': chat_id, 'text': bot_answer}
        )

@socketio.on('connect')
def handle_connect():
    print(f"Новое соединение установлено: {request.sid}")  
    emit('connection_ack', {'message': 'Соединение установлено'}, room=request.sid)

@app.route('/telegram', methods=['POST'])
def save_user_telegram():
    data = request.json
    chat_source = 'tg'
    chat_id = str(data['ID чата'])
    name = f'{str(data['Имя'])} {str(data['Фамилия'])}'
    message = str(data['Сообщение'])
    save_user(chat_source, chat_id, name, message)
    return jsonify({'message': 'Данные успешно сохранены'})

def send_user_telegram(chat_id, message_bot):
    payload = {
        'chat_id': chat_id,
        'text': message_bot
    }
    print(payload)
    response = requests.post(f'https://api.telegram.org/bot{tg_bot_token}/sendMessage', json=payload)
    if response.status_code == 200:
        print("Сообщение успешно отправлено пользователю.")
    else:
        print("Ошибка при отправке сообщения:", response.status_code)

@app.route('/vk', methods=['POST'])
def save_user_vk():
    data = request.json
    chat_source = 'vk'
    chat_id = str(data['ID чата'])
    name = f'{str(data['Имя'])} {str(data['Фамилия'])}'
    message = str(data['Сообщение'])
    save_user(chat_source, chat_id, name, message)
    return jsonify({'message': 'Данные успешно сохранены'})

def send_user_vk(id, text):
    vk = vk_api.VkApi(token = vk_bot_token)
    vk.method('messages.send', {'user_id' : id, 'message' : text, 'random_id': 0})

@app.route('/gmail', methods=['POST'])
def save_user_gmail():
    data = request.json
    print(data)
    chat_source = 'mail'
    chat_id = str(data['email_address'])
    name = str(data['name'])
    message = f'{str(data['subject'])}\n{str(data['text'])}'
    save_user(chat_source, chat_id, name, message)
    return jsonify({'message': 'Данные успешно сохранены'})

def send_user_gmail(chat_id, text):
    msg = MIMEText(text)
    msg['Subject'] = ''
    msg['From'] = EMAIL_ADDRESS
    msg['To'] = chat_id
    with smtplib.SMTP(SMTP_SERVER, SMTP_PORT) as server:
        server.starttls()
        server.login(EMAIL_ADDRESS, APP_PASSWORD)
        server.sendmail(EMAIL_ADDRESS, chat_id, msg.as_string())

@app.route('/get_chats', methods=['GET'])
def get_chats():
    res_data = dbmanager.get_chats()
    return res_data

@app.route('/get_chat_messages', methods=['POST'])
def get_chat_messages():
    data = request.json
    chat_source = data['chat_source']
    chat_id = data['chat_id']
    res_data = dbmanager.get_chat_messages(chat_source, chat_id)
    return res_data

@app.route('/send_message', methods=['POST'])
def send_message():
    data = request.json
    source = 'user'
    chat_source = data['chat_source']
    chat_id = data['chat_id']
    message = data['message']
    if (len(message) > 0):
        dbmanager.add_message(source, chat_source, chat_id, message)
        dbmanager.change_status(chat_source, chat_id, 'awaiting')
        if chat_source  == 'tg':
            send_user_telegram(chat_id, message)
        elif chat_source  == 'vk':
            send_user_vk(chat_id, message)
        elif chat_source  == 'mail':
            send_user_gmail(chat_id, message)
    res_data = jsonify({'message': 'Данные успешно сохранены'})
    return res_data

@app.route('/close_chat', methods=['POST'])
def close_chat():
    data = request.json
    chat_source = data['chat_source']
    chat_id = data['chat_id']
    dbmanager.change_status(chat_source, chat_id, 'closed')
    res_data = jsonify({'message': 'Данные успешно сохранены'})
    return res_data

processes = []

@app.route('/run_bots', methods=['POST'])
def run_bots():
    global processes, tg_bot_token, vk_bot_token, EMAIL_ADDRESS, APP_PASSWORD
    data = request.json
    tg_bot_token = data['tg_token']
    vk_bot_token = data['vk_token']
    EMAIL_ADDRESS = data['email_address']
    APP_PASSWORD = data['email_password']
    for process in processes:
        process.terminate()  
        process.wait()      
    if len(tg_bot_token) > 0:
        processes.append(subprocess.Popen(['python', 'BotTelegram.py', tg_bot_token]))
    if len(vk_bot_token) > 0:
        processes.append(subprocess.Popen(['python', 'VkBot.py', vk_bot_token]))
    if len(EMAIL_ADDRESS) > 0 and len(APP_PASSWORD) > 0:
        processes.append(subprocess.Popen(['python', 'GmailBot.py', EMAIL_ADDRESS, APP_PASSWORD]))
    res_data = jsonify({'message': 'Боты запущены'})
    return res_data


if __name__ == '__main__':
    dbmanager.create_db()
    socketio.run(app, host=HOST, port=PORT)
