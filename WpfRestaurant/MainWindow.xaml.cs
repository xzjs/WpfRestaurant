using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.ActiveMQ.Commands;
using Newtonsoft.Json.Linq;

namespace WpfRestaurant
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
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
            Lop = new LobbyOrderPage(this);
            PageFrame.Content = Lop;
            
            ListenOrderTcp();

            var showTimer = new DispatcherTimer();
            showTimer.Tick += ShowCurTimer;
            showTimer.Interval = new TimeSpan(0, 0, 0, 1);
            showTimer.Start();
        }

        private void ShowCurTimer(object sender, EventArgs e)
        {
            DateTextBlock.Text = DateTime.Now.ToShortDateString();
            TimeTextblock.Text = DateTime.Now.ToShortTimeString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var lop = new LobbyOrderPage(this);
            PageFrame.Content = lop;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Lop.GetList(1);
        }

        /// <summary>
        ///     更新桌位和菜单
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
                        var responseString =
                            client.DownloadString("http://" + Config.Http + "/restClient/menuInfoById.nd?id=" +
                                                  Infomation.RestaurantID);
                        var jo = JObject.Parse(responseString);
                        if (jo["menuList"] != null)
                        {
                            db.Database.ExecuteSqlCommand("DELETE FROM Food");
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
                                if (f.Img == null)
                                    f.Img = "menu.png";
                                f.Price = (decimal) item["price"];
                                f.OnsalePrice = (decimal) item["onsalePrice"];
                                f.SaleType = (int) item["saleType"];
                                db.Food.Add(f);
                            }
                        }

                        responseString =
                            client.DownloadString("http://" + Config.Http + "/restClient/deskInfoById.nd?id=" +
                                                  Infomation.RestaurantID);
                        jo = JObject.Parse(responseString);
                        if (jo["deskList"] != null)
                        {
                            db.Database.ExecuteSqlCommand("DELETE FROM [Table]");
                            foreach (var item in jo["deskList"])
                            {
                                var t = new Table
                                {
                                    DeskID = (long) item["id"],
                                    No = (string) item["deskNumber"],
                                    Type = (int) item["type"],
                                    Counts = (int) item["counts"]
                                };
                                if ((string) item["status"] == null)
                                    t.Status = 0;
                                else
                                    t.Status = (int) item["status"];
                                db.Table.Add(t);
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        ///     设置对应的餐桌数目
        /// </summary>
        public void SetNum()
        {
            using (var db = new restaurantEntities())
            {
                var count = db.Table.Count();
                var free = db.Table.Count(x => x.Status == 0);
                FreeTextBlock.Text = free.ToString();
                BusyTextBlock.Text = (count - free).ToString();
            }
        }

        /// <summary>
        ///     餐桌类型切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var b = sender as Button;
            var type = 0;
            switch (b.Tag.ToString())
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

        /// <summary>
        ///     通过tcp获取数据
        /// </summary>
        private void ListenOrderTcp()
        {
            try
            {
                IConnectionFactory factory = new ConnectionFactory("tcp://" + Config.Tcp + ":" + Config.Port);
                var connection = factory.CreateConnection();
                connection.ClientId = Infomation.RestaurantID.ToString();
                connection.Start();
                var session1 = connection.CreateSession();
                var consumer1 = session1.CreateConsumer(new ActiveMQQueue("menuAppoint" + Infomation.RestaurantID));
                consumer1.Listener += consumer_Listener;
                var session = connection.CreateSession();
                var consumer = session.CreateConsumer(new ActiveMQQueue("menuOrder" + Infomation.RestaurantID));
                consumer.Listener += Foodconsumer_Listener;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void consumer_Listener(IMessage message)
        {
            var msg = (ITextMessage) message;
            //异步调用下，否则无法回归主线程
            Console.WriteLine(msg.Text);
            Book(msg.Text);
        }

        private void Foodconsumer_Listener(IMessage message)
        {
            var msg = (ITextMessage) message;
            //异步调用下，否则无法回归主线程
            Console.WriteLine(msg.Text);
            OrderFood(msg.Text);
        }

        /// <summary>
        ///     预订桌位
        /// </summary>
        /// <param name="json"></param>
        private void Book(string json)
        {
            try
            {
                using (var db = new restaurantEntities())
                {
                    var jo = JArray.Parse(json);
                    foreach (var item in jo)
                    {
                        var phone = (long) item["contactTel"];
                        var name = (string) item["name"];
                        var counts = (int) item["counts"];
                        var no = (string) item["orderNumber"];
                        var remark = (string) item["remark"];
                        var deskid = (long) item["repastDeskId"];
                        var time = Convert.ToDateTime(item["repastTimeStr"]);
                        var type = (int) item["type"];
                        //先查找有没有已经创建订单
                        var order = db.Order.FirstOrDefault(x => x.No == no);
                        var table = db.Table.First(x => x.DeskID == deskid);
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
                                Type = type,
                                Cost = 0
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
                Lop.GetList();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void OrderFood(string json)
        {
            try
            {
                Thread.Sleep(1000);
                using (var db = new restaurantEntities())
                {
                    var jo = JArray.Parse(json);
                    foreach (var item in jo)
                    {
                        var no = (string) item["orderNumber"];
                        var price = (double) item["price"];
                        //先查找有没有已经创建订单
                        var order = db.Order.FirstOrDefault(x => x.No == no);
                        if (order == null)
                        {
                            order = new Order
                            {
                                No = no,
                                Table_id = 0,
                                Cost = 0
                            };
                            db.Order.Add(order);
                            db.SaveChanges();
                        }
                        foreach (var j in item["subOrderList"])
                        {
                            var foodNo = (long) j["menuId"];
                            var f = db.Food.First(m => m.No == foodNo);
                            var b = new Bill
                            {
                                Food_id = f.Id,
                                Num = (int) j["counter"],
                                Order_id = order.Id
                            };
                            if (b.Num != null)
                            {
                                b.Price = f.Price * b.Num.Value;
                                order.Cost += b.Price.Value;
                            }
                            db.Bill.Add(b);
                        }
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        /// <summary>
        ///     关闭按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void Login(object sender, RoutedEventArgs e)
        {
            var lw = new LoginWindow();
            lw.ShowDialog();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            using (var db = new restaurantEntities())
            {
                Config = db.Config.First();
                if (Config != null)
                {
                    MyApp.Http = Config.Http;
                }
                else
                {
                    LoginWindow lw = new LoginWindow();
                    lw.ShowDialog();
                }
                Infomation = db.Infomation.First();
            }
        }

        private void OutFoodClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("外卖功能尚未开放");
        }
    }
}