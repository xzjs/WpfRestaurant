using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfRestaurant
{
    /// <summary>
    ///     BookPage.xaml 的交互逻辑
    /// </summary>
    public partial class BookPage : Page
    {
        private readonly MainWindow _mainWindow;
        private readonly Order _order;

        public BookPage(MainWindow mainWindow, Order order)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _order = order;
            BookStackPanel.DataContext = _order;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var op = new OrderPage(_mainWindow);
            _mainWindow.SidebarFrame.Content = op;
            TableItem.SetTableStatus(2, MyApp.TableId);
            _mainWindow.Lop.GetList();
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Order(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new restaurantEntities())
                {
                    long tableId = _order.Table_id;
                    TableItem.SetTableStatus(0, tableId);
                    List<Bill> bills = db.Bill.Where(b => b.Order_id == _order.Id).ToList();
                    db.Bill.RemoveRange(bills);
                    Order order = db.Order.Find(_order.Id);
                    db.Order.Remove(order);
                    db.SaveChanges();
                    _mainWindow.SidebarFrame.Content = null;
                    _mainWindow.Lop.GetList();

                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void Close_Page(object sender, RoutedEventArgs e)
        {
            _mainWindow.SidebarFrame.Content = null;
        }
    }
}