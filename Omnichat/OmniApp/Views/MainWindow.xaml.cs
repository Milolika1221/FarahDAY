using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;

namespace OmniApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private void Window_MouseLeftButton(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private bool flag = false;
        private double offset = 50;
        public ObservableCollection<Chat> Chats { get; set; }
        public MainWindow(string userName)
        {
            InitializeComponent();
            CreateMenuElements();
            UserAccount user = new UserAccount(userName);
            user.UserName = userName;
            ProfilPlace.Children.Add(user);
            HideElements();
            ServerConnection.RunBots();
        }


        private Timer _updateTimer;

        private async void CreateMenuElements()
        {
            _updateTimer = new Timer(10000);
            _updateTimer.Elapsed += async (sender, e) => await UpdateChats();
            _updateTimer.AutoReset = true;
            _updateTimer.Enabled = true;

            await UpdateChats();
        }

        private async Task UpdateChats()
        {
            var chats = await ServerConnection.GetChats();
            Chats = new ObservableCollection<Chat>(chats ?? new List<Chat>());

            var AwaitingChats = Chats.Where(chat => chat.Status == "awaiting");
            var OpenChats = Chats.Where(chat => chat.Status == "open");
            var ClosedChats = Chats.Where(chat => chat.Status == "closed");
            var OfflineChats = Chats.Where(chat => chat.Status == "offline");

            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateMenuElements(AwaitingChats, OpenChats, ClosedChats, OfflineChats);
            });
        }

        private void UpdateMenuElements(IEnumerable<Chat> awaitingChats, IEnumerable<Chat> openChats, IEnumerable<Chat> closedChats, IEnumerable<Chat> offlineChats)
        {
            MenuPlace.Children.Clear();
            AddMenuElement("Ожидают ответа", awaitingChats);
            AddMenuElement("В диалоге", openChats);
            AddMenuElement("Закрытые диалоги", closedChats);
            AddMenuElement("Офлайн-обращения", offlineChats);
        }

        private void AddMenuElement(string header, IEnumerable<Chat> chats)
        {
            ClientMenuElem menuElem = new ClientMenuElem
            {
                Text = header,
                listBox = { ItemsSource = chats }
            };
            menuElem.SelectionChangedEvent += OnChatSelected;
            Style listBoxStyle = new Style(typeof(ListBoxItem));
            listBoxStyle.Setters.Add(new Setter(Control.FontFamilyProperty, new FontFamily("Georgia")));
            //menuElem.listBox.Height = 30 * menuElem.listBox.Items.Count; ;
            MenuPlace.Children.Add(menuElem);
        }


        private void OnChatSelected(object sender, Chat selectedChat)
        {
            Console.WriteLine($"Выбран чат с ID: {selectedChat.Chat_id} в MainWindow");
            WithoutChat.Visibility = Visibility.Collapsed;
            SettingWindow.Visibility = Visibility.Collapsed;
            /*if (ChatWindow.Parent is Panel parent)
            {
                parent.Children.Remove(ChatWindow);
            }
            ChatWindow newChatWindow = new ChatWindow(selectedChat)
            {
                Visibility = Visibility.Visible
            };
            Grid.SetRow(newChatWindow, 1);
            Grid.SetColumn(newChatWindow, 2);
            MainGrid.Children.Add(newChatWindow);*/
            MainGrid.Children.Remove(ChatWindow);
            ChatWindow = new ChatWindow(selectedChat);
            ChatWindow.Visibility = Visibility.Visible;
            Grid.SetRow(ChatWindow, 1);
            Grid.SetColumn(ChatWindow, 2);
            MainGrid.Children.Add(ChatWindow);
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (!flag)
            {
                ShowElements();
                ShiftElements(offset);
            }
            else
            {
                HideElements();
                ShiftElements(-offset);
            }
            flag = !flag;
        }

        private void ShowElements()
        {
            img.Visibility = Visibility.Visible;
            label1.Visibility = Visibility.Visible;
            label2.Visibility = Visibility.Visible;
            label3.Visibility = Visibility.Visible;
            label4.Visibility = Visibility.Visible;
        }

        private void HideElements()
        {
            img.Visibility = Visibility.Hidden;
            label1.Visibility = Visibility.Hidden;
            label2.Visibility = Visibility.Hidden;
            label3.Visibility = Visibility.Hidden;
            label4.Visibility = Visibility.Hidden;
        }

        private void ShiftElements(double offset)
        {
            foreach (var child in MenuPlace.Children)
            {
                if (child is ClientMenuElem elem)
                {
                    var transform = elem.RenderTransform as TranslateTransform;

                    if (transform == null)
                    {
                        transform = new TranslateTransform();
                        elem.RenderTransform = transform;
                    }

                    // Сдвигаем элемент вправо
                    transform.X += offset;
                }
            }
        }
        private void Setting_Button_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow.Visibility = Visibility.Visible;
            ChatWindow.Visibility = Visibility.Collapsed;
        }
        private void ChatWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void ChatWindow_Loaded_1(object sender, RoutedEventArgs e)
        {

        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void SetWindowSize()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            bor.Width = screenWidth;
            bor.Height = screenHeight;
        }


        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Height = 650;
            Width = 1200;
            bor.Width = Width;
            bor.Height = Height;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                SetWindowSize();
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application app = Application.Current;
            app.Shutdown();
        }


    }
}
