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

        public SetUpPage()
        {
            InitializeComponent();
            using (var db = new restaurantEntities())
            {
                _config = db.Config.FirstOrDefault();
                ConfigStackPanel.DataContext = _config;
            }
        }

        public LoginWindow ParentWindow { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new restaurantEntities())
            {
                db.Entry(_config).State = EntityState.Modified;
                db.SaveChanges();
            }
            MyApp.Http = _config.Http;
            var lp = new LoginPage {ParentWindow = ParentWindow};
            ParentWindow.PageFrame.Content = lp;
        }
    }
}