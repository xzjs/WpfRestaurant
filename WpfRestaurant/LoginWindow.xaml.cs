using System.Linq;
using System.Windows;

namespace WpfRestaurant
{
    /// <summary>
    ///     LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public Infomation Infomation;

        public LoginWindow()
        {
            InitializeComponent();

            using (var db = new restaurantEntities())
            {
                Infomation = db.Infomation.FirstOrDefault();
                if (Infomation != null)
                    PageFrame.Content = new LogoutPage(this);
                else
                    PageFrame.Content = new LoginPage(this);
            }
        }
    }
}