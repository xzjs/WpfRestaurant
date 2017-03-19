using System;
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
        public LoginPage()
        {
            InitializeComponent();
        }

        public LoginWindow ParentWindow { get; set; }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var sup = new SetUpPage { ParentWindow = ParentWindow };
            ParentWindow.PageFrame.Content = sup;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new restaurantEntities())
                {
                    var config = db.Config.FirstOrDefault();
                    if (config == null)
                    {
                        throw new Exception("请先配置相关参数");
                    }
                    var name = NameTextbox.Text.Trim();
                    var p = Marshal.SecureStringToBSTR(PwdBox.SecurePassword);
                    var password = Marshal.PtrToStringBSTR(p);

                    using (var client = new WebClient())
                    {
                        var values = new NameValueCollection
                        {
                            ["account"] = name,
                            ["password"] = password
                        };

                        var response = client.UploadValues("http://" + config.Http + "/restLogin/login.nd", values);

                        var responseString = Encoding.Default.GetString(response);
                        var jo = JObject.Parse(responseString);
                        if (jo["id"] != null)
                        {
                            var i = db.Infomation.FirstOrDefault() ?? new Infomation();
                            i.RestaurantID = (int)jo["id"];
                            i.Name = (string) jo["name"];
                            if (i.Id == 0)
                            {
                                db.Infomation.Add(i);
                            }
                            db.SaveChanges();
                            ParentWindow.Close();
                        }
                        else
                            MessageBox.Show("登录失败");
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

        }
    }
}