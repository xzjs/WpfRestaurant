using System.Windows;

namespace WpfRestaurant
{
    /// <summary>
    ///     LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            var lg = new LoginPage();
            lg.ParentWindow = this;
            PageFrame.Content = lg;
        }
    }
}