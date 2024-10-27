import telebot
import requests
import threading
import sys

bot = telebot.TeleBot(sys.argv[1])

#Стартовая команла
@bot.message_handler(commands=['start', 'help'])
def main(message):
    bot.send_message(message.chat.id, text='Здравствуйте, чем могу помочь?')

#Реакция бота на обычные сообщения
@bot.message_handler(func=lambda message: True)
def answer(message):
    user_data = {
        'Имя': message.from_user.first_name,
        'Фамилия': message.from_user.last_name,
        'Сообщение': message.text,
        'ID чата': message.chat.id
    } #Получаем данные пользователя
    # Создаем отдельный поток для отправки данных на сервер
    t = threading.Thread(target=send_data, args=(user_data,))
    t.start()

def send_data(user_data):
    # Отправляем данные на сервер
    try:
        response = requests.post(sys.argv[2] + '/telegram', json=user_data)
        if response.status_code == 200:
            print("Данные успешно отправлены на сервер.")
        else:
            print("Ошибка при отправке данных на сервер:", response.status_code)
    except Exception as e:
        print("Не удалось отправить данные на сервер:", e)

bot.polling(none_stop=True)
