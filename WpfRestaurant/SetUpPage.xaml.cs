using System;
using System.Collections.Generic;
using System.Data.Entity;
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

namespace WpfRestaurant
{
    /// <summary>
    /// SetUpPage.xaml 的交互逻辑
    /// </summary>
    public partial class SetUpPage : Page
    {
        private LoginWindow _parentWindow;
        private Config _config; 
        public SetUpPage()
        {
            InitializeComponent();
            using (var db=new restaurantEntities())
            {
                _config = db.Config.FirstOrDefault();
                ConfigStackPanel.DataContext = _config;
            }
        }

        public LoginWindow ParentWindow
        {
            get
            {
                return _parentWindow;
            }

            set
            {
                _parentWindow = value;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using(var db=new restaurantEntities())
            {
                db.Entry(_config).State = EntityState.Modified;
                db.SaveChanges();
            }
            LoginPage lp = new LoginPage();
            lp.ParentWindow = _parentWindow;
            _parentWindow.PageFrame.Content = lp;
        }
    }
}
