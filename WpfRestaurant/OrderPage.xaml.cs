using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WpfRestaurant
{
    /// <summary>
    ///     OrderPage.xaml 的交互逻辑
    /// </summary>
    public partial class OrderPage : Page
    {
        private readonly MainWindow _mainWindow;
        private readonly Table _table;
        private Order _order;

        public OrderPage(MainWindow pw)
        {
            InitializeComponent();

            _mainWindow = pw;
            using (var db = new restaurantEntities())
            {
                _table = db.Table.Find(MyApp.TableId);
            }
            LoadData();
            SetTableNo();
        }

        public void SetTableNo()
        {
            using (var db = new restaurantEntities())
            {
                var t = db.Table.Find(MyApp.TableId);
                TableNoTextblock.Text = t.No;
                string[] typeStrings = new string[] { "大厅", "小包间", "大包间" };
                TypeTextBlock.Text = typeStrings[t.Type - 1];
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var mw = new MenuWindow(_mainWindow, _order);
            mw.ShowDialog();
        }

        public void LoadData()
        {
            using (var db = new restaurantEntities())
            {
                _order =
                    db.Order.Include("Bill.Food")
                        .Where(x => x.Table_id == MyApp.TableId).Where(o=>o.Finish==0)
                        .OrderByDescending(x => x.Id)
                        .FirstOrDefault();
            }
            BillDataGrid.ItemsSource = _order.Bill.ToList();
            CostTextBlock.Text = "结算：￥" + _order.Cost.Value.ToString(CultureInfo.InvariantCulture);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using (var db = new restaurantEntities())
            {
                var o = db.Order.Find(_order.Id);
                if (o != null)
                {
                    o.Cost = _order.Cost;
                    o.Finish = 1;
                    db.SaveChanges();

                    if (_mainWindow.Infomation.RestaurantID != null)
                    {
                        var uo = new UploadOrder
                        {
                            restaurantId = (int)_mainWindow.Infomation.RestaurantID,
                            repastDeskId = _table.DeskID,
                            repastTimeStr = o.Time.Value.ToString("yyyy-M-d H:m:s"),
                            price = o.Cost.Value,
                            subOrderList = new List<Menu>()
                        };
                        foreach (var item in o.Bill)
                            if (item.Num != null)
                            {
                                var m = new Menu
                                {
                                    menuId = item.Food.No,
                                    counter = (int)item.Num
                                };
                                uo.subOrderList.Add(m);
                            }
                        var json = JsonConvert.SerializeObject(uo, Formatting.Indented);
                        // 上传订单
                        try
                        {
                            using (var client = new WebClient())
                            {
                                var values = new NameValueCollection {["details"] = json};

                                var response =
                                    client.UploadValues(
                                        "http://" + _mainWindow.Config.Http + "/restClient/uploadMenuOrder.nd", values);

                                var responseString = Encoding.Default.GetString(response);
                                var jo = JObject.Parse(responseString);
                                if ((string) jo["errorFlag"] != "false")
                                    throw new Exception("上传订单失败");
                            }
                        }
                        catch (WebException webException)
                        {
                            string parameter = JsonConvert.SerializeObject(new Dictionary<string, string>
                            {
                                ["details"] = json
                            }, Formatting.Indented);
                            Queue queue = new Queue
                            {
                                Url = "http://" + MyApp.Http + "/restClient/uploadMenuOrder.nd",
                                Type = "POST",
                                Time = DateTime.Now,
                                Parameter = parameter
                            };
                            db.Queue.Add(queue);
                            db.SaveChanges();
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(exception.Message);
                        }
                    }
                }
                // 设置桌子状态
                TableItem.SetTableStatus(0, MyApp.TableId);
                //刷新桌子
                _mainWindow.Lop.GetList();
                //设置右边栏
                _mainWindow.SidebarFrame.Content = new FreeTablePage(_mainWindow);
            }
        }

        private void ClosePage(object sender, RoutedEventArgs e)
        {
            _mainWindow.SidebarFrame.Content = null;
        }
    }
}