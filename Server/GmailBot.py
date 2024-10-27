import smtplib
import imaplib
import email
from email.mime.text import MIMEText
from email.header import decode_header
from email.utils import parseaddr
import requests
import threading
import time
import concurrent.futures
import sys

# Конфигурация почты
SMTP_SERVER = 'smtp.gmail.com'
SMTP_PORT = 587
IMAP_SERVER = 'imap.gmail.com'
EMAIL_ADDRESS = sys.argv[1]
APP_PASSWORD = sys.argv[2]

processed_emails = set()

def send_data(user_data):
    try:
        response = requests.post(sys.argv[3] + 'gmail', json=user_data)
        if response.status_code == 200:
            print("Данные успешно отправлены на сервер.")
        else:
            print("Ошибка при отправке данных на сервер:", response.status_code)
    except Exception as e:
        print("Не удалось отправить данные на сервер:", e)

def read_emails():
    with imaplib.IMAP4_SSL(IMAP_SERVER) as mail:
        mail.login(EMAIL_ADDRESS, APP_PASSWORD)
        mail.select('inbox')
        
        status, messages = mail.search(None, 'UNSEEN')
        email_ids = messages[0].split()

        with concurrent.futures.ThreadPoolExecutor(max_workers=5) as executor:
            for email_id in email_ids:
                if email_id in processed_emails:
                    continue
                status, msg_data = mail.fetch(email_id, '(RFC822)')
                msg = email.message_from_bytes(msg_data[0][1])
                from_ = msg['From']
                name, email_address = parseaddr(from_)
                subject, encoding = decode_header(msg['Subject'])[0]
                if isinstance(subject, bytes):
                    subject = subject.decode(encoding if isinstance(encoding, str) else 'utf-8')

                body = ""
                if msg.is_multipart():
                    for part in msg.walk():
                        content_type = part.get_content_type()
                        content_disposition = str(part.get("Content-Disposition"))

                        if content_type == "text/plain" and "attachment" not in content_disposition:
                            try:
                                body = part.get_payload(decode=True).decode()
                            except UnicodeDecodeError:
                                try:
                                    body = part.get_payload(decode=True).decode('latin1') 
                                except UnicodeDecodeError:
                                    body = part.get_payload(decode=True).decode('utf-8', errors='replace')
                            break  
                else:
                    body = msg.get_payload(decode=True).decode()  

                user_data = {
                    'name' : name,
                    'email_address': email_address,
                    'subject': subject,
                    'text': body
                }
                
                executor.submit(send_data, user_data)
                processed_emails.add(email_id)


def main():
    while True:
        read_emails()
        time.sleep(2)  

if __name__ == "__main__":
    main()
        
