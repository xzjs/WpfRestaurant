using System;
using System.Collections.Specialized;
using System.ComponentModel;
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
        private readonly BackgroundWorker _backgroundWorker;
        private readonly LoginWindow _loginWindow;
        private Config _config;

        public LoginPage(LoginWindow loginWindow)
        {
            InitializeComponent();
            _loginWindow = loginWindow;
            using (var db = new restaurantEntities())
            {
                _config = db.Config.First();
            }
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += Init;
            _backgroundWorker.RunWorkerCompleted += Completed;
        }

        private void Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            _loginWindow.Close();
        }

        private void Init(object sender, DoWorkEventArgs e)
        {
            try
            {
                using (var db = new restaurantEntities())
                {
                    using (var client = new WebClient())
                    {
                        client.Encoding = Encoding.UTF8;
                        var infomation = db.Infomation.First();
                        var responseString =
                            client.DownloadString("http://" + _config.Http + "/restClient/menuInfoById.nd?id=" +
                                                  infomation.RestaurantID);
                        var jo = JObject.Parse(responseString);
                        infomation.path = (string) jo["picUrl"];
                        db.SaveChanges();

                        if (jo["menuList"] != null)
                            foreach (var item in jo["menuList"])
                            {
                                var f = new Food
                                {
                                    No = (long) item["id"],
                                    Name = (string) item["menuName"],
                                    Detail = (string) item["details"],
                                    Type = (int) item["type"],
                                    Img = (string) item["picUrl"]
                                };
                                f.Img = MyApp.Download_Img(infomation.path, f.Img);
                                f.Price = (decimal) item["price"];
                                f.OnsalePrice = (decimal) item["onsalePrice"];
                                f.SaleType = (int) item["saleType"];
                                db.Food.Add(f);
                            }
                        responseString =
                            client.DownloadString("http://" + _config.Http + "/restClient/deskInfoById.nd?id=" +
                                                  infomation.RestaurantID);
                        jo = JObject.Parse(responseString);
                        if (jo["deskList"] != null)
                            foreach (var item in jo["deskList"])
                            {
                                var t = new Table
                                {
                                    DeskID = (long) item["id"],
                                    No = (string) item["deskNumber"],
                                    Type = (int) item["type"],
                                    Counts = (int) item["counts"]
                                };

                                t.Status = 0;
                                db.Table.Add(t);
                            }
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //Console.WriteLine(ex.Message);
            }
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var sup = new SetUpPage(_loginWindow);
            _loginWindow.PageFrame.Content = sup;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new restaurantEntities())
                {
                    _config = db.Config.First();
                    MyApp.Config = _config;
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

                        var response = client.UploadValues("http://" + _config.Http + "/restLogin/login.nd", values);
                        var responseString = Encoding.UTF8.GetString(response);
                        var jo = JObject.Parse(responseString);
                        if (jo["id"] != null)
                        {
                            var i = new Infomation
                            {
                                RestaurantID = (int) jo["id"],
                                Name = (string) jo["name"]
                            };

                            db.Infomation.Add(i);
                            MyApp.Infomation = i;
                            db.SaveChanges();
                            Button.Content = "登录成功，正在初始化数据";
                            ProgressRing.IsActive = true;
                            _backgroundWorker.RunWorkerAsync();
                        }
                        else
                        {
                            MessageBox.Show("登录失败");
                        }
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