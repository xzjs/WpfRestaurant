using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace WpfRestaurant
{
    /// <summary>
    /// OrderPage.xaml 的交互逻辑
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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MenuWindow mw = new MenuWindow(_mainWindow, _order);
            mw.ShowDialog();
        }

        public void LoadData()
        {
            using (var db = new restaurantEntities())
            {
                _order = db.Order.Include("Bill.Food").Where(x => x.Table_id == MyApp.TableId).OrderByDescending(x => x.Id).First();
                foreach(var item in _order.Bill)
                {
                    if (item.Price != null) _order.Cost += (decimal)item.Price;
                }
            }
            BillDataGrid.ItemsSource = _order.Bill.ToList();
            tableNoTextblock.Text = _table.No;
            CostTextBlock.Text = _order.Cost.ToString(CultureInfo.InvariantCulture);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using (var db=new restaurantEntities())
            {
                Order o = db.Order.Find(_order.Id);
                if (o != null)
                {
                    o.Cost = _order.Cost;
                    //db.SaveChanges();

                    if (_mainWindow.Infomation.RestaurantID != null)
                    {
                        UploadOrder uo = new UploadOrder
                        {
                            restaurantId = (int)_mainWindow.Infomation.RestaurantID,
                            repastDeskId = _table.DeskID,
                            price = o.Cost,
                            subOrderList = new List<Menu>()
                        };
                        foreach(var item in o.Bill)
                        {
                            if (item.Num != null)
                            {
                                Menu m = new Menu
                                {
                                    menuId = item.Food.No,
                                    counter = (int)item.Num
                                };
                                uo.subOrderList.Add(m);
                            }
                        }
                        string json= JsonConvert.SerializeObject(uo, Formatting.Indented);
                        // 上传订单
                        //TODO 上传订单出错
                        try
                        {
                            using (var client = new WebClient())
                            {
                                var values = new NameValueCollection { ["detail"] = json };

                                var response = client.UploadValues("http://" + _mainWindow.Config.Http + "/restClient/uploadMenuOrder.nd", values);

                                var responseString = Encoding.Default.GetString(response);
                                JObject jo = JObject.Parse(responseString);
                                MessageBox.Show((string)jo["errorFlag"] == "false" ? "上传成功" : "上传失败");
                            }
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(exception.Message);
                        }            
                    }
                }
                // 设置桌子状态
                Table table = db.Table.Find(_table.Id);
                table.Status = 0;
                //保存数据库更改
                db.SaveChanges();
                //上传桌子状态
                try
                {
                    using (var client = new WebClient())
                    {
                        var values = new NameValueCollection
                        {
                            ["deskId"] = _table.DeskID.ToString(),
                            ["status"] = "0"
                        };

                        var response = client.UploadValues("http://" + _mainWindow.Config.Http + "/restClient/setDeskStatus.nd", values);

                        var responseString = Encoding.Default.GetString(response);
                        JObject jo = JObject.Parse(responseString);
                        MessageBox.Show((string)jo["errorFlag"] == "false" ? "设置成功" : "设置失败");
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
                
                //刷新桌子
                _mainWindow.Lop.GetList();
                //设置右边栏
                _mainWindow.SidebarFrame.Content=new FreeTablePage(_mainWindow);
            }
        }
    }
}
