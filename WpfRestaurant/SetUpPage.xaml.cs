using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfRestaurant
{
    /// <summary>
    ///     SetUpPage.xaml 的交互逻辑
    /// </summary>
    public partial class SetUpPage : Page
    {
        private readonly Config _config;
        private readonly LoginWindow _loginWindow;
        public SetUpPage(LoginWindow loginWindow)
        {
            _loginWindow = loginWindow;
            InitializeComponent();
            using (var db = new restaurantEntities())
            {
                _config = db.Config.FirstOrDefault();
                ConfigStackPanel.DataContext = _config;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new restaurantEntities())
            {
            
                db.Entry(_config).State = EntityState.Modified;
                db.SaveChanges();
                MyApp.Http = _config.Http;
                Infomation infomation = db.Infomation.FirstOrDefault();
                if (infomation == null)
                {
                    _loginWindow.PageFrame.Content = new LoginPage(_loginWindow);
                }
                else
                {
                    _loginWindow.PageFrame.Content=new LogoutPage(_loginWindow);
                }
            }
        }
    }
}