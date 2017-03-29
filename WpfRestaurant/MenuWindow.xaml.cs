using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfRestaurant
{
    /// <summary>
    ///     MenuWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MenuWindow : Window
    {
        private readonly Config _config;
        private readonly List<Bill> _listBill;
        private readonly MainWindow _mainWindow;
        private readonly Order _order;

        public MenuWindow(MainWindow mw, Order o)
        {
            InitializeComponent();

            try
            {
                _mainWindow = mw;
                _order = o;
                _listBill = new List<Bill>();

                using (var db = new restaurantEntities())
                {
                    _config = db.Config.First();

                    var lf = db.Food.ToList();
                    foreach (var item in lf)
                    {
                        var b = new Bill
                        {
                            Food = item,
                            Order_id = 0,
                            Num = 0
                        };
                        foreach (var bill in _order.Bill)
                            if (bill.Food.Id == item.Id)
                                b.Num = bill.Num;
                        _listBill.Add(b);
                    }
                }

                FoodListBind();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        public void FoodListBind(int type = 1)
        {
            var lb = _listBill.Where(x => x.Food.Type == type).ToList();
            FoodListBox.ItemsSource = lb;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var b = sender as Button;
            if (b != null)
            {
                var type = Convert.ToInt32(b.Tag);
                var title = "";
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
                Title = title;
                FoodListBind(type);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var id = Convert.ToInt64((sender as Button)?.Tag);
            var b = _listBill.First(x => x.Food.Id == id);
            b.Num++;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var id = Convert.ToInt64(button.Tag);
                var b = _listBill.First(x => x.Food.Id == id);
                if (b.Num > 0)
                    b.Num--;
            }
        }

        /// <summary>
        ///     点菜
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
                            if (item.Num > 0)
                            {
                                var b = new Bill
                                {
                                    Food_id = item.Food.Id,
                                    Order_id = _order.Id,
                                    Num = item.Num,
                                    Price = item.Food.Price * item.Num
                                };
                                db.Bill.Add(b);
                                if (b.Price != null) _order.Cost += b.Price.Value;
                            }
                        db.SaveChanges();

                        var op = new OrderPage(_mainWindow);
                        _mainWindow.Op = op;
                        _mainWindow.SidebarFrame.Content = op;
                        var t = db.Table.Find(MyApp.TableId);
                        t.Status = 2;
                        db.SaveChanges();
                        _mainWindow.Lop.GetList();

                        //设置服务器上的桌子状态
                        TableItem.SetTableStatus(2, MyApp.TableId);
                    }
                    else
                    {
                        //删除原来点的菜
                        db.Bill.RemoveRange(db.Bill.Where(m => m.Order_id == _order.Id));
                        foreach (var item in _listBill)
                        {
                            if (!(item.Num > 0)) continue;
                            var b = new Bill
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

        /// <summary>
        ///     关闭菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Closer_Menu(object sender, RoutedEventArgs e)
        {
            Close();
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
    }
}