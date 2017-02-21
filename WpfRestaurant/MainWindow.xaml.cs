using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Config config;
        private Infomation infomation;
        public MainWindow()
        {
            InitializeComponent();
            LobbyOrderPage lop = new LobbyOrderPage();
            lop.ParentWin = this;
            PageFrame.Content = lop;
            using (var db = new restaurantEntities())
            {
                config = db.Config.First();
                infomation = db.Infomation.First();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LobbyOrderPage lop = new LobbyOrderPage();
            lop.ParentWin = this;
            PageFrame.Content = lop;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            AppointPage ap = new AppointPage();
            PageFrame.Content = ap;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new restaurantEntities())
                {
                    using (var client = new WebClient())
                    {
                        client.Encoding = Encoding.UTF8;
                        var responseString = client.DownloadString("http://" + config.Http + "/restClient/menuInfoById.nd?id=" + infomation.RestaurantID);
                        JObject jo = JObject.Parse(responseString);
                        if (jo["menuList"] != null)
                        {
                            db.Database.ExecuteSqlCommand("DELETE FROM Food");
                            foreach (var item in jo["menuList"])
                            {
                                Food f = new Food();
                                f.No = (long)item["id"];
                                f.Name = (string)item["menuName"];
                                f.Detail = (string)item["details"];
                                f.Type = (int)item["type"];
                                f.Img = (string)item["picUrl"];
                                if (f.Img == null)
                                {
                                    f.Img = "default.jpg";
                                }
                                f.Price = (decimal)item["price"];
                                f.OnsalePrice = (decimal)item["onsalePrice"];
                                f.SaleType = (int)item["saleType"];
                                db.Food.Add(f);
                            }
                        }

                        responseString = client.DownloadString("http://" + config.Http + "/restClient/deskInfoById.nd?id=" + infomation.RestaurantID);
                        jo = JObject.Parse(responseString);
                        if (jo["deskList"] != null)
                        {
                            db.Database.ExecuteSqlCommand("DELETE FROM [Table]");
                            foreach (var item in jo["deskList"])
                            {
                                Table t = new Table();
                                t.DeskID = (long)item["id"];
                                t.No = (string)item["deskNumber"];
                                t.Type = (int)item["type"];
                                t.Counts = (int)item["counts"];
                                if ((string)item["status"] == null)
                                {
                                    t.Status = 0;
                                }else
                                {
                                    t.Status = (int)item["status"];
                                }
                                db.Table.Add(t);
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
