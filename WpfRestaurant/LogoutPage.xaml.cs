using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfRestaurant
{
    /// <summary>
    ///     LogoutPage.xaml 的交互逻辑
    /// </summary>
    public partial class LogoutPage : Page
    {
        private readonly LoginWindow _loginWindow;

        public LogoutPage(LoginWindow loginWindow)
        {
            InitializeComponent();
            _loginWindow = loginWindow;
            TextBlock.Text = loginWindow.Infomation.Name;
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _loginWindow.PageFrame.Content = new SetUpPage(_loginWindow);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("注销会清空本地所有数据，是否注销", "注销", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;
                using (var db = new restaurantEntities())
                {
                    db.Database.ExecuteSqlCommand("DELETE FROM [Queue]");
                    db.Database.ExecuteSqlCommand("DELETE FROM [Bill]");
                    db.Database.ExecuteSqlCommand("DELETE FROM [Order]");
                    db.Database.ExecuteSqlCommand("DELETE FROM [Food]");
                    db.Database.ExecuteSqlCommand("DELETE FROM [Table]");
                    db.Database.ExecuteSqlCommand("DELETE FROM [Infomation]");
                    _loginWindow.PageFrame.Content = new LoginPage(_loginWindow);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void Enter(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            _loginWindow.Close();
        }
    }
}