import vk_api
from vk_api.longpoll import VkLongPoll, VkEventType
import requests
import threading
import sys
token = sys.argv[1]

# Авторизуемся как сообщество
vk = vk_api.VkApi(token=token)
give = vk.get_api()
longpoll = VkLongPoll(vk)

def send_data(user_data):
    # Отправляем данные на сервер
    try:
        response = requests.post('http://127.0.0.1:5000/vk', json=user_data)
        if response.status_code == 200:
            print("Данные успешно отправлены на сервер.")
        else:
            print("Ошибка при отправке данных на сервер:", response.status_code)
    except Exception as e:
        print("Не удалось отправить данные на сервер:", e)

for event in longpoll.listen():

    # Если пришло новое сообщение
    if event.type == VkEventType.MESSAGE_NEW:
        
        if event.to_me:
            message = event.text
            id = event.user_id
            response = give.users.get(user_ids = id, verify=True)
            user = vk.method("users.get", {"user_ids": id})#Все данные
            user_data = {
                'Фамилия': user[0]['first_name'],
                'Имя': user[0]['last_name'],
                'Сообщение': message,
                'ID чата': id
            }
            #print(user_data)
            t = threading.Thread(target=send_data, args=(user_data,))
            t.start()
