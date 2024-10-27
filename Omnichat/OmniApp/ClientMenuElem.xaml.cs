using System;
using System.Collections.Generic;
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

namespace OmniApp
{
    /// <summary>
    /// Логика взаимодействия для ClientMenuElem.xaml
    /// </summary>
    public partial class ClientMenuElem : UserControl
    {
        public event EventHandler<Chat> SelectionChangedEvent;

        private bool is_opened = false;
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ClientMenuElem), new PropertyMetadata("label"));

        private void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            label.Content = e.NewValue.ToString();
        }

        private void UpdateLabel()
        {
            label.Content = Text;
        }

        public ClientMenuElem()
        {
            InitializeComponent();
            listBox.Visibility = Visibility.Visible;
            //double itemHeight = 30;
            //listBox.Height = itemHeight * listBox.Items.Count;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button SND = sender as Button;
            listBox.SelectedItem = null;
            if (!is_opened)
            {
                listBox.Visibility = Visibility.Visible;
                //double itemHeight = 30; 
                //listBox.Height = itemHeight * listBox.Items.Count;
                SND.Content = "^";
            }
            else
            {
                listBox.Visibility = Visibility.Collapsed;
                SND.Content = "v";
            }
            is_opened = !is_opened;
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox.SelectedItem is Chat selectedChat)
            {
                Console.WriteLine($"Вы выбрали чат с ID: {selectedChat.Chat_id}");
                SelectionChangedEvent?.Invoke(this, selectedChat);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
