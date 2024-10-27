using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OmniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace OmniApp
{
    public static class ServerConnection
    {
        public static async Task<List<Chat>> GetChats()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(Properties.Settings.Default.ServerUrl + "/get_chats");
                    response.EnsureSuccessStatusCode();
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine("Полученный JSON:\n" + jsonResponse);
                    return JsonConvert.DeserializeObject<List<Chat>>(jsonResponse);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка при запросе: " + e.Message);
                    return null;
                }
            }
        }

        public static async Task<List<Message>> GetChatMessages(string chatSource, string chatId)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var data = new
                    {
                        chat_source = chatSource,
                        chat_id = chatId
                    };

                    string jsonData = JsonConvert.SerializeObject(data);
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(Properties.Settings.Default.ServerUrl + "/get_chat_messages", content);
                    response.EnsureSuccessStatusCode();

                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine("Полученный JSON:\n" + jsonResponse);

                    return JsonConvert.DeserializeObject<List<Message>>(jsonResponse);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка при запросе: " + e.Message);
                    return null;
                }
            }
        }

        public static async Task<string> SendMessage(string chatSource, string chatId, string Message)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var data = new
                    {
                        chat_source = chatSource,
                        chat_id = chatId,
                        message = Message
                    };

                    string jsonData = JsonConvert.SerializeObject(data);
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(Properties.Settings.Default.ServerUrl + "/send_message", content);
                    response.EnsureSuccessStatusCode();

                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine("Ответ сервера:\n" + jsonResponse);

                    return jsonResponse;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка при отправке сообщения: " + e.Message);
                    return null;
                }
            }
        }

        public static async Task<string> CloseChat (string chatSource, string chatId)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var data = new 
                    {
                        chat_source = chatSource,
                        chat_id = chatId,
                    };
                    string jsonData = JsonConvert.SerializeObject(data);
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(Properties.Settings.Default.ServerUrl + "/close_chat", content);
                    response.EnsureSuccessStatusCode();

                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine("Ответ сервера:\n" + jsonResponse);

                    return jsonResponse;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка при попытке закрыть чат: " + e.Message);
                    return null;
                }
            }
        }

        public static async Task<string> RunBots()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var data = new
                    {
                        tg_token = Properties.Settings.Default.TgToken,
                        vk_token = Properties.Settings.Default.VkToken,
                        email_address = Properties.Settings.Default.MailAddress,
                        email_password = Properties.Settings.Default.MailPassword
                    };
                    string jsonData = JsonConvert.SerializeObject(data);
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(Properties.Settings.Default.ServerUrl + "/run_bots", content);
                    response.EnsureSuccessStatusCode();

                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine("Ответ сервера:\n" + jsonResponse);

                    return jsonResponse;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка при попытке запустить ботов: " + e.Message);
                    return null;
                }
            }
        }


    }
}
