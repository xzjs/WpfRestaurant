using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace WpfRestaurant
{
    /// <summary>
    /// MenuWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MenuWindow : Window
    {
        private readonly List<Bill> _listBill;
        private readonly MainWindow _mainWindow;
        private readonly Config _config;
        private readonly Order _order;

        public MenuWindow(MainWindow mw, Order o)
        {
            InitializeComponent();

            _mainWindow = mw;
            _order = o;
            _listBill = new List<Bill>();

            using (var db = new restaurantEntities())
            {
                _config = db.Config.First();

                List<Food> lf = db.Food.ToList();
                foreach (var item in lf)
                {
                    Bill b = new Bill
                    {
                        Food = item,
                        Order_id = 0,
                        Num = 0
                    };
                    foreach (var bill in _order.Bill)
                    {
                        if (bill.Food.Id == item.Id)
                        {
                            b.Num = bill.Num;
                        }
                    }
                    _listBill.Add(b);
                }
            }

            FoodListBind();
        }

        public void FoodListBind(int type = 1)
        {
            List<Bill> lb = _listBill.Where(x => x.Food.Type == type).ToList();
            FoodListBox.ItemsSource = lb;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b != null)
            {
                int type = Convert.ToInt32(b.Tag);
                string title = "";
                switch (type)
                {
                    case 1:
                        title = "热菜";
                        break;
                    case 2:
                        title = "凉菜";
                        break;
                    case 3:
                        title = "主食";
                        break;
                    case 4:
                        title = "饮品";
                        break;
                    case 5:
                        title = "香烟";
                        break;
                    default:
                        break;
                }
                this.Title = title;
                FoodListBind(type);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            long id = Convert.ToInt64((sender as Button)?.Tag);
            Bill b = _listBill.First(x => x.Food.Id == id);
            b.Num++;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                long id = Convert.ToInt64(button.Tag);
                Bill b = _listBill.First(x => x.Food.Id == id);
                if (b.Num > 0)
                {
                    b.Num--;
                }
            }
        }

        /// <summary>
        /// 点菜
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new restaurantEntities())
                {
                    if (_order.Id == 0)
                    {
                        _order.Table_id = MyApp.TableId;
                        _order.Time = DateTime.Now;
                        _order.Type = 0;
                        _order.Cost = 0;
                        db.Order.Add(_order);
                        db.SaveChanges();
                        foreach (var item in _listBill)
                        {
                            if (item.Num > 0)
                            {
                                Bill b = new Bill
                                {
                                    Food_id = item.Food.Id,
                                    Order_id = _order.Id,
                                    Num = item.Num,
                                    Price = item.Food.Price * item.Num
                                };
                                db.Bill.Add(b);
                            }
                        }
                        db.SaveChanges();

                        OrderPage op = new OrderPage(_mainWindow);
                        _mainWindow.Op = op;
                        _mainWindow.SidebarFrame.Content = op;
                        Table t = db.Table.Find(MyApp.TableId);
                        t.Status = 2;
                        db.SaveChanges();
                        _mainWindow.Lop.GetList();

                        //设置服务器上的桌子状态
                        using (var client = new WebClient())
                        {
                            var values = new NameValueCollection
                            {
                                ["deskId"] = t.DeskID.ToString(),
                                ["status"] = "2"
                            };

                            var response = client.UploadValues("http://" + _config.Http + "/restClient/setDeskStatus.nd", values);

                            var responseString = Encoding.Default.GetString(response);
                            JObject jo = JObject.Parse(responseString);
                            MessageBox.Show((string) jo["errorFlag"] == "false" ? "设置成功" : "设置失败");
                        }
                    }
                    else
                    {
                        //删除原来点的菜
                        db.Bill.RemoveRange(db.Bill.Where(m => m.Order_id == _order.Id));
                        foreach (var item in _listBill)
                        {
                            if (!(item.Num > 0)) continue;
                            Bill b = new Bill
                            {
                                Food_id = item.Food.Id,
                                Order_id = _order.Id,
                                Num = item.Num,
                                Price = item.Food.Price * item.Num
                            };
                            db.Bill.Add(b);
                        }
                        db.SaveChanges();

                    }
                    _mainWindow.Op.LoadData();
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }
}
