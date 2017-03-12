using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json.Linq;

namespace WpfRestaurant
{
    /// <summary>
    ///     LoginPage.xaml 的交互逻辑
    /// </summary>
    public partial class LoginPage : Page
    {
        private readonly Config _config;

        public LoginPage()
        {
            InitializeComponent();
            using (var db = new restaurantEntities())
            {
                _config = db.Config.First();
            }
        }

        public LoginWindow ParentWindow { get; set; }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var sup = new SetUpPage();
            sup.ParentWindow = ParentWindow;
            ParentWindow.PageFrame.Content = sup;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var name = NameTextbox.Text.Trim();
            var p = Marshal.SecureStringToBSTR(PwdBox.SecurePassword);
            var password = Marshal.PtrToStringBSTR(p);

            using (var client = new WebClient())
            {
                var values = new NameValueCollection();
                values["account"] = name;
                values["password"] = password;

                var response = client.UploadValues("http://" + _config.Http + "/restLogin/login.nd", values);

                var responseString = Encoding.Default.GetString(response);
                var jo = JObject.Parse(responseString);
                if (jo["id"] != null)
                    using (var db = new restaurantEntities())
                    {
                        var i = db.Infomation.First();
                        i.RestaurantID = (int) jo["id"];
                        db.SaveChanges();
                        ParentWindow.Close();
                    }
                else
                    MessageBox.Show("登录失败");
            }
        }
    }
}