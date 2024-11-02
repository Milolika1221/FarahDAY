using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Automation.Provider;
using System.Timers;
using SocketIOClient;
using System.Net.Sockets;
using System.Text.Json;
using Newtonsoft.Json;

namespace OmniApp
{
    /// <summary>
    /// Логика взаимодействия для ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : UserControl
    {
        private bool isOpen = false;
        public ObservableCollection<Message> Messages;
        private Chat chat;
        public ChatWindow(Chat chat)
        {
            this.chat = chat;
            InitializeComponent();
            IsHidden();
            LoadMessagesAsync();
            ScrollToBottom();
            this.Loaded += OnWindowLoaded;
        }
        public ChatWindow() 
        {
            InitializeComponent();
            IsHidden();
        }
        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Окно загружено");
            await InitializeSocketAsync();
        }

        private SocketIOClient.SocketIO client;
        private async Task InitializeSocketAsync()
        {
            client = new SocketIOClient.SocketIO(Properties.Settings.Default.ServerUrl);
            client.Options.AutoUpgrade = false;
            Console.WriteLine("Клиент создан");

            client.On("connection_ack", response =>
            {
                var jsonData = response.GetValue<JsonElement>();
                string message = jsonData.GetProperty("message").GetString();
                Console.WriteLine("Соединение подтверждено сервером: " + message);
            });
            client.OnConnected += (s, e) =>
            {
                Console.WriteLine(client.Connected);
            };
            client.On("new_message", response =>
            {
                Console.WriteLine(response.ToString());
                var newMessage = JsonConvert.DeserializeObject<List<Message>>(response.ToString())[0];
                Console.WriteLine($"newMessage: {newMessage.Chat_id}, {newMessage.Chat_sourсe}, {newMessage.Text}");
                if (newMessage.Chat_id == chat.Chat_id && newMessage.Chat_sourсe == chat.Source)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Messages.Add(newMessage);
                        Console.WriteLine("Добавлено новое сообщение");
                        if (Messages.Count > 0)
                        {
                            ChatName.Text = Messages[0].Source;
                        }
                    });
                }
            });
            await client.ConnectAsync();
        }
        private async Task LoadMessagesAsync()
        {
            var messages = await ServerConnection.GetChatMessages(chat.Source, chat.Chat_id);
            Messages = new ObservableCollection<Message>(messages ?? new List<Message>());
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessagesList.ItemsSource = Messages;
                if (Messages.Count > 0)
                {
                    ChatName.Text = Messages[0].Source;
                }
            });
        }

        private async void On_Closing(object sender, RoutedEventArgs e)
        {
            if (client != null)
            {
                await client.DisconnectAsync();
            }
        }

        public void IsHidden()
        {
            btn1.Visibility = Visibility.Hidden;
            btn2.Visibility = Visibility.Hidden;
            img1.Visibility = Visibility.Hidden;
            img2.Visibility = Visibility.Hidden;
        }

        public void IsVisible()
        {
            btn1.Visibility=Visibility.Visible;
            btn2.Visibility=Visibility.Visible;
            img1.Visibility=Visibility.Visible;
            img2.Visibility=Visibility.Visible;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textbox_GotFocus(object sender, RoutedEventArgs e)
        {
            textbox.Text = "";
        }
        private void ScrollToBottom()
        {
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToEnd();
            }
        }

        private void UploadButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (!isOpen) 
            {
                IsVisible();
            }
            else
            {
                IsHidden();
            }
            isOpen = !isOpen;
        }
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textbox.Text))
            {
                Message message = new Message("user", chat.Source, chat.Chat_id, textbox.Text);
                Messages.Add(message);
                textbox.Clear();
                ScrollToBottom();
                await ServerConnection.SendMessage(chat.Source, chat.Chat_id, message.Text);
            }
        }
        private async void CloseStatusButton_Click(object sender, RoutedEventArgs e)
        {
            await ServerConnection.CloseChat(chat.Source, chat.Chat_id);
        }
    }
}
