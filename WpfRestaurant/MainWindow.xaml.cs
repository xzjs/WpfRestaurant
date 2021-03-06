﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using Newtonsoft.Json;
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
        public ObservableCollection<MessageWindow> MessageWindows;
        public OrderPage Op;

        private string str = "";

        public MainWindow()
        {
            InitializeComponent();
            Lop = new LobbyOrderPage(this);
            PageFrame.Content = Lop;
            var showTimer = new DispatcherTimer();
            showTimer.Tick += ShowCurTimer;
            showTimer.Interval = new TimeSpan(0, 0, 0, 1);
            showTimer.Start();

            //开启队列线程
            var uploadQueue = new DispatcherTimer();
            uploadQueue.Tick += UploadQueue;
            uploadQueue.Interval = new TimeSpan(1, 0, 0);
            uploadQueue.Start();

            MessageWindows = new ObservableCollection<MessageWindow>();
            MessageWindows.CollectionChanged += ArrageWindow;

            using (var db = new restaurantEntities())
            {
                Config = db.Config.FirstOrDefault();
                Infomation = db.Infomation.FirstOrDefault();
                if (Infomation != null)
                    NameTextBlock.Text = Infomation.Name;
            }

            ListenOrderTcp();
        }

        /// <summary>
        ///     排列显示消息窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ArrageWindow(object sender, NotifyCollectionChangedEventArgs e)
        {
            var height = SystemParameters.WorkArea.Height;
            var width = SystemParameters.WorkArea.Width;
            var max = Convert.ToInt32(Math.Floor(height / 200));

            for (var i = 0; i < MessageWindows.Count; i++)
            {
                var m = i % max;
                MessageWindows[i].Top = height - 200 * (m + 1);
                var n = i / max;
                MessageWindows[i].Left = width - 300 * (n + 1);
            }
        }

        private void ShowMessageWindow()
        {
            var messageWindow = new MessageWindow(str, this);
            messageWindow.Show();
            MessageWindows.Add(messageWindow);
        }

        private void UploadQueue(object sender, EventArgs e)
        {
            using (var db = new restaurantEntities())
            {
                var queues = db.Queue.OrderBy(q => q.Id).ToList();
                if (queues.Count > 0)
                    using (var client = new WebClient())
                    {
                        foreach (var queue in queues)
                            try
                            {
                                var dictionary =
                                    JsonConvert.DeserializeObject<Dictionary<string, string>>(queue.Parameter);
                                var values = new NameValueCollection();
                                foreach (var item in dictionary)
                                    values[item.Key] = item.Value;
                                var response = client.UploadValues(queue.Url, values);

                                var responseString = Encoding.Default.GetString(response);
                                var jo = JObject.Parse(responseString);
                                if ((string) jo["errorFlag"] != "false") continue;
                                db.Queue.Remove(queue);
                                db.SaveChanges();
                            }
                            catch (WebException webException)
                            {
                                break;
                            }
                            catch (Exception exception)
                            {
                                MessageBox.Show(exception.Message);
                            }
                    }
            }
        }

        private void ShowCurTimer(object sender, EventArgs e)
        {
            DateTextBlock.Text = DateTime.Now.ToShortDateString();
            TimeTextblock.Text = DateTime.Now.ToShortTimeString();
        }

        /// <summary>
        ///     更新桌位和菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
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
                case "包间":
                    type = 2;
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

                var session2 = connection.CreateSession();
                var modify = session2.CreateConsumer(new ActiveMQQueue("restaurantModify" + Infomation.RestaurantID));
                modify.Listener += modify_Listener;
            }
            catch (Exception exception)
            {
                //MessageBox.Show(exception.Message);
            }
        }

        private void modify_Listener(IMessage message)
        {
            try
            {
                var msg = (ITextMessage) message;
                Console.WriteLine(msg.Text);
                using (var db = new restaurantEntities())
                {
                    var jo = JArray.Parse(msg.Text);
                    foreach (var item in jo)
                    {
                        var type = (int) item["modifyType"];
                        if ((int) item["modifyItem"] == 0) //修改餐桌
                        {
                            if (type == 0) //添加
                            {
                                var t = new Table
                                {
                                    DeskID = (long) item["deskInfo"]["id"],
                                    No = (string) item["deskInfo"]["deskNumber"],
                                    Type = (int) item["deskInfo"]["type"],
                                    Counts = (int) item["deskInfo"]["counts"]
                                };
                                if ((string) item["deskInfo"]["status"] == null)
                                    t.Status = 0;
                                else
                                    t.Status = (int) item["deskInfo"]["status"];
                                var table = db.Table.FirstOrDefault(o => o.DeskID == t.DeskID);
                                if (table != null)
                                    continue;
                                db.Table.Add(t);
                            }
                            else if (type == 1) //删除
                            {
                                var deskID = (long) item["deskInfo"]["id"];
                                var table = db.Table.FirstOrDefault(t => t.DeskID == deskID);
                                if (table != null)
                                    db.Table.Remove(table);
                            }
                            else //修改
                            {
                                var deskID = (long) item["deskInfo"]["id"];
                                var table = db.Table.FirstOrDefault(t => t.DeskID == deskID);
                                if (table != null)
                                {
                                    table.No = (string) item["deskInfo"]["deskNumber"];
                                    table.Type = (int) item["deskInfo"]["type"];
                                    table.Counts = (int) item["deskInfo"]["counts"];
                                }
                            }
                        }
                        else
                        {
                            if (type == 0) //添加
                            {
                                var f = new Food
                                {
                                    No = (long) item["menuInfo"]["id"],
                                    Name = (string) item["menuInfo"]["menuName"],
                                    Detail = (string) item["menuInfo"]["details"],
                                    Type = (int) item["menuInfo"]["type"],
                                    Img = (string) item["menuInfo"]["picUrl"]
                                };
                                f.Img = MyApp.Download_Img(Infomation.path, f.Img);
                                var food = db.Food.FirstOrDefault(o => o.No == f.No);
                                if (food != null)
                                    continue;
                                db.Food.Add(f);
                            }
                            else if (type == 1) //删除
                            {
                                var foodID = (long) item["menuInfo"]["id"];
                                var food = db.Food.FirstOrDefault(t => t.No == foodID);
                                if (food != null)
                                    db.Food.Remove(food);
                            }
                            else //修改
                            {
                                var foodID = (long) item["menuInfo"]["id"];
                                var food = db.Food.FirstOrDefault(t => t.No == foodID);
                                if (food != null)
                                {
                                    food.Name = (string) item["menuInfo"]["menuName"];
                                    food.Detail = (string) item["menuInfo"]["details"];
                                    food.Type = (int) item["menuInfo"]["type"];
                                    food.Img = MyApp.Download_Img(Infomation.path, (string) item["menuInfo"]["picUrl"]);
                                }
                            }
                        }
                        db.SaveChanges();
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { Lop.GetList(); }));
                        str = "修改数据成功\n";
                        switch (type)
                        {
                            case 0:
                                str += "添加";
                                break;
                            case 1:
                                str += "删除";
                                break;
                            default:
                                str += "修改";
                                break;
                        }
                        if ((int) item["modifyItem"] == 0)
                            str += "餐桌" + (string) item["deskInfo"]["deskNumber"];
                        else
                            str += "菜单" + (string) item["menuInfo"]["menuName"];
                    }
                }
            }
            catch (Exception exception)
            {
                //MessageBox.Show(exception.Message);
                str = "修改失败" + exception.Message;
            }
            finally
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(ShowMessageWindow));
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
                        long phone = 0;
                        var counts = 0;
                        long deskid = 0;
                        var time = DateTime.Now;
                        var type = 0;
                        long server_id = 0;
                        if (!string.IsNullOrEmpty((string) item["contactTel"]))
                            phone = (long) item["contactTel"];
                        var name = (string) item["name"];
                        if (!string.IsNullOrEmpty((string) item["counts"]))
                            counts = (int) item["counts"];
                        var no = (string) item["orderNumber"];
                        var remark = (string) item["remark"];
                        if (!string.IsNullOrEmpty((string) item["repastDeskId"]))
                            deskid = (long) item["repastDeskId"];
                        if (!string.IsNullOrEmpty((string) item["repastTimeStr"]))
                            time = Convert.ToDateTime(item["repastTimeStr"]);
                        if (!string.IsNullOrEmpty((string) item["type"]))
                            type = (int) item["type"];
                        if (!string.IsNullOrEmpty((string)item["id"]))
                            server_id = (long)item["id"];

                        //先查找有没有已经创建订单
                        var order = db.Order.FirstOrDefault(x => x.No == no);
                        var table = db.Table.First(x => x.DeskID == deskid);
                        table.Status = type == 0 ? 2 : 1;

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
                                Cost = 0,
                                Finish = 0,
                                Server_id = server_id
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
                            order.Finish = 0;
                            order.Server_id = server_id;
                        }
                        db.SaveChanges();
                        if (type == 0)
                            str = table.No + "桌点餐";
                        else
                            str = table.No + "桌预定\n预定时间" + time + "\n联系方式" + order.Phone;
                    }
                }
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { Lop.GetList(); }));
            }
            catch (Exception exception)
            {
                //MessageBox.Show(exception.Message);
                str = "订单错误" + exception.Message;
            }
            finally
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(ShowMessageWindow));
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
            lw.Show();
            Close();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            using (var db = new restaurantEntities())
            {
                Config = db.Config.FirstOrDefault();
                Infomation = db.Infomation.FirstOrDefault();
                if (Config != null && Infomation != null)
                {
                    MyApp.Http = Config.Http;
                }
                else
                {
                    var lw = new LoginWindow();
                    lw.ShowDialog();
                }
            }
        }

        private void OutFoodClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("外卖功能尚未开放");
        }
    }
}