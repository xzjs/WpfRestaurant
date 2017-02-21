using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Net;

namespace WpfRestaurant
{
    /// <summary>
    /// LoginPage.xaml 的交互逻辑
    /// </summary>
    public partial class LoginPage : Page
    {
        private LoginWindow parentWindow;
        private Config config;
        public LoginPage()
        {
            InitializeComponent();
            using (var db = new restaurantEntities())
            {
                config = db.Config.First();
            }
        }

        public LoginWindow ParentWindow
        {
            get
            {
                return parentWindow;
            }

            set
            {
                parentWindow = value;
            }
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetUpPage sup = new SetUpPage();
            sup.ParentWindow = ParentWindow;
            parentWindow.PageFrame.Content = sup;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            string name = NameTextbox.Text.Trim();
            IntPtr p = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(PwdBox.SecurePassword);
            string password = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(p);

            using (var client = new WebClient())
            {
                var values = new NameValueCollection();
                values["account"] = name;
                values["password"] = password;

                var response = client.UploadValues("http://" + config.Http + "/restLogin/login.nd", values);

                var responseString = Encoding.Default.GetString(response);
                JObject jo = JObject.Parse(responseString);
                if (jo["id"] != null)
                {
                    using (var db = new restaurantEntities())
                    {
                        Infomation i = db.Infomation.First();
                        i.RestaurantID = (int)jo["id"];
                        MainWindow mw = new MainWindow();
                        mw.ShowDialog();
                    }
                }
                else
                {
                    MessageBox.Show("登录失败");
                }
            }
        }
    }
}
