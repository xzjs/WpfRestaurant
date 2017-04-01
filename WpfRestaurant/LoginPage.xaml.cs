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

                        var responseString = Encoding.UTF8.GetString(response);
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
                            MessageBox.Show("登录成功");
                            MainWindow mainWindow=Application.Current.MainWindow as MainWindow;
                            mainWindow.NameTextBlock.Text = i.Name;
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

        private void Init(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                using (var db = new restaurantEntities())
                {
                    int count = db.Order.Count(o => o.Finish == 1);
                    if (count > 0)
                    {
                        throw new Exception("有未完成的订单，无法更新本地数据");
                    }
                    var messageBoxResult = MessageBox.Show("更新数据将会清空订单和历史数据，是否更新", "是否更新数据",
                        MessageBoxButton.OKCancel);
                    if (messageBoxResult != MessageBoxResult.OK)
                        return;
                    using (var client = new WebClient())
                    {
                        client.Encoding = Encoding.UTF8;
                        
                        var responseString =
                            client.DownloadString("http://" + mainWindow.Config.Http + "/restClient/menuInfoById.nd?id=" +
                                                  mainWindow.Infomation.RestaurantID);
                        var jo = JObject.Parse(responseString);
                        Infomation infomation = db.Infomation.First();
                        mainWindow.Infomation.path = (string)jo["picUrl"];
                        infomation.path= (string)jo["picUrl"];
                        db.SaveChanges();
                        if (jo["menuList"] != null)
                        {
                            db.Database.ExecuteSqlCommand("DELETE FROM Food");
                            foreach (var item in jo["menuList"])
                            {
                                var f = new Food
                                {
                                    No = (long)item["id"],
                                    Name = (string)item["menuName"],
                                    Detail = (string)item["details"],
                                    Type = (int)item["type"],
                                    Img = (string)item["picUrl"]
                                };
                                
                                f.Img = mainWindow.Download_Img(f.Img);
                                f.Price = (decimal)item["price"];
                                f.OnsalePrice = (decimal)item["onsalePrice"];
                                f.SaleType = (int)item["saleType"];
                                db.Food.Add(f);
                            }
                        }

                        responseString =
                            client.DownloadString("http://" + mainWindow.Config.Http + "/restClient/deskInfoById.nd?id=" +
                                                  mainWindow.Infomation.RestaurantID);
                        jo = JObject.Parse(responseString);
                        if (jo["deskList"] != null)
                        {
                            db.Database.ExecuteSqlCommand("DELETE FROM [Table]");
                            foreach (var item in jo["deskList"])
                            {
                                var t = new Table
                                {
                                    DeskID = (long)item["id"],
                                    No = (string)item["deskNumber"],
                                    Type = (int)item["type"],
                                    Counts = (int)item["counts"]
                                };
                                if ((string)item["status"] == null)
                                    t.Status = 0;
                                else
                                    t.Status = (int)item["status"];
                                db.Table.Add(t);
                            }
                        }
                        db.SaveChanges();
                    }
                    mainWindow.Lop.GetList();
                    MessageBox.Show("数据初始化成功");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}