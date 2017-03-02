using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace WpfRestaurant
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public Config Config;
        public Infomation Infomation;
        public LobbyOrderPage Lop;
        public OrderPage Op;

        public MainWindow()
        {
            InitializeComponent();
            Lop = new LobbyOrderPage {ParentWin = this};
            PageFrame.Content = Lop;
            using (var db = new restaurantEntities())
            {
                Config = db.Config.First();
                Infomation = db.Infomation.First();
            }
            SetNum();
            ListenOrderTcp();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LobbyOrderPage lop = new LobbyOrderPage {ParentWin = this};
            PageFrame.Content = lop;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            AppointPage ap = new AppointPage();
            PageFrame.Content = ap;
        }

        /// <summary>
        /// 更新桌位和菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new restaurantEntities())
                {
                    using (var client = new WebClient())
                    {
                        client.Encoding = Encoding.UTF8;
                        var responseString = client.DownloadString("http://" + Config.Http + "/restClient/menuInfoById.nd?id=" + Infomation.RestaurantID);
                        JObject jo = JObject.Parse(responseString);
                        if (jo["menuList"] != null)
                        {
                            db.Database.ExecuteSqlCommand("DELETE FROM Food");
                            foreach (var item in jo["menuList"])
                            {
                                Food f = new Food
                                {
                                    No = (long) item["id"],
                                    Name = (string) item["menuName"],
                                    Detail = (string) item["details"],
                                    Type = (int) item["type"],
                                    Img = (string) item["picUrl"]
                                };
                                if (f.Img == null)
                                {
                                    f.Img = "menu.png";
                                }
                                f.Price = (decimal)item["price"];
                                f.OnsalePrice = (decimal)item["onsalePrice"];
                                f.SaleType = (int)item["saleType"];
                                db.Food.Add(f);
                            }
                        }

                        responseString = client.DownloadString("http://" + Config.Http + "/restClient/deskInfoById.nd?id=" + Infomation.RestaurantID);
                        jo = JObject.Parse(responseString);
                        if (jo["deskList"] != null)
                        {
                            db.Database.ExecuteSqlCommand("DELETE FROM [Table]");
                            foreach (var item in jo["deskList"])
                            {
                                Table t = new Table
                                {
                                    DeskID = (long) item["id"],
                                    No = (string) item["deskNumber"],
                                    Type = (int) item["type"],
                                    Counts = (int) item["counts"]
                                };
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

        public void SetNum()
        {
            using (var db=new restaurantEntities())
            {
                int count = db.Table.Count();
                int free = db.Table.Count(x => x.Status == 0);
                FreeTextBlock.Text = free.ToString();
                BusyTextBlock.Text = (count - free).ToString();
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            int type = 0;
            switch (b?.Content.ToString())
            {
                case "大厅":
                    type = 1;
                    break;
                case "小包间":
                    type = 2;
                    break;
                case "大包间":
                    type = 3;
                    break;
            }
            MyApp.TableType = type;
            Lop.GetList();
        }

        private void ListenOrderTcp()
        {
            IConnectionFactory factory = new ConnectionFactory("tcp://"+Config.Tcp+":"+Config.Port);
            IConnection connection = factory.CreateConnection();
            connection.ClientId = Infomation.RestaurantID.ToString();
            connection.Start();
            ISession session = connection.CreateSession();
            IMessageConsumer consumer = session.CreateConsumer(new Apache.NMS.ActiveMQ.Commands.ActiveMQQueue("menuAppoint"+Infomation.RestaurantID));
            consumer.Listener += consumer_Listener;
        }
        void consumer_Listener(IMessage message)
        {
            ITextMessage msg = (ITextMessage)message;
            //异步调用下，否则无法回归主线程
            MessageBox.Show(msg.Text);
            Book(msg.Text);
        }

        /// <summary>
        /// 预订桌位
        /// </summary>
        /// <param name="json"></param>
        void Book(string json)
        {
            try
            {
                using (var db = new restaurantEntities())
                {
                    JArray jo = JArray.Parse(json);
                    foreach (var item in jo)
                    {
                        long phone = (long) item["contactTel"];
                        string name = (string) item["name"];
                        int counts = (int) item["counts"];
                        string no = (string) item["orderNumber"];
                        string remark = (string) item["remark"];
                        long deskid = (long) item["repastDeskId"];
                        DateTime time = Convert.ToDateTime(item["repastTimeStr"]);
                        int type = (int) item["type"];
                        //先查找有没有已经创建订单
                        Order order = db.Order.FirstOrDefault(x => x.No == no);
                        Table table = db.Table.First(x => x.DeskID == deskid);
                        table.Status = 1;
                        if (order == null)
                        {
                            
                            order = new Order
                            {
                                Phone = phone,
                                Name = name,
                                Counts = counts,
                                No = no,
                                Remark = remark,
                                Table_id = table.Id,
                                Time = time,
                                Type = type
                            };
                            db.Order.Add(order);
                        }
                        else
                        {
                            order.Phone = phone;
                            order.Name = name;
                            order.Counts = counts;
                            order.Remark = remark;
                            order.Table_id = table.Id;
                            order.Time = time;
                            order.Type = type;
                        }
                        db.SaveChanges();
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
