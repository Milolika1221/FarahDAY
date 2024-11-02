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
    /// Логика взаимодействия для UserAccount.xaml
    /// </summary>
    public partial class UserAccount : UserControl
    {
        private bool is_opened = false;
        public UserAccount()
        {
            InitializeComponent();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ExitAccount exitAccount = new ExitAccount();
            Button SND = sender as Button;
            if (!is_opened)
            {
                bord.Height = 80;
                SND.Content = "Имя пользователя   ^";
                ExitPlace.Children.Add(exitAccount);
            }
            else
            {
                bord.Height = 40;
                SND.Content = "Имя пользователя   V";
                ExitPlace.Children.Remove(exitAccount);
            }
            is_opened = !is_opened;
        }

    }
}
