using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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
    /// LobbyOrderPage.xaml 的交互逻辑
    /// </summary>
    public partial class LobbyOrderPage : Page
    {
        private MainWindow _parentWin;
        public LobbyOrderPage(MainWindow parentWin)
        {
            _parentWin = parentWin;
            InitializeComponent();

            GetList();
        }

        public void GetList()
        {
            BusyTableList.ItemsSource = GetTableList(2);
            FreeTableList.ItemsSource = GetTableList(0);
            OrderTableList.ItemsSource = GetTableList(1);
            _parentWin.SetNum();
        }

        /// <summary>
        /// 获取餐桌列表
        /// </summary>
        /// <param name="status">0：空闲；1：预约；2：繁忙</param>
        /// <returns></returns>
        private List<TableItem> GetTableList(int status)
        {
            using (var db = new restaurantEntities())
            {
                var tables = db.Table.Where(m => m.Status == status);
                int type = MyApp.TableType;
                if (type > 0)
                {
                    tables = tables.Where(m => m.Type == type);
                }
                List<Table> lt = tables.ToList();
                List<TableItem> lti=new List<TableItem>();
                foreach (var t in lt)
                {
                    TableItem tableItem = new TableItem
                    {
                        Id = t.Id,
                        Table = t,
                        No = t.No,
                    };
                    if (status > 0)
                    {
                        Order order = t.Order.First(o => o.Finish == 0);
                        if (status == 1)
                        {
                            tableItem.Time = order.Time.ToShortTimeString();
                        }
                        if (status == 2)
                        {
                            tableItem.Cost = order.Cost;
                        }
                        tableItem.Order = order;
                    }
                    lti.Add(tableItem);
                }
                return lti;
            }
        }

        /// <summary>
        /// 繁忙桌子点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BusyTableClick(object sender, RoutedEventArgs e)
        {
            Button sp = sender as Button;
            MyApp.TableId = Convert.ToInt64(sp.Tag);
            OrderPage op = new OrderPage(_parentWin);
            _parentWin.Op = op;
            _parentWin.SidebarFrame.Content = op;
        }

        /// <summary>
        /// 预订桌位点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OrderTableClick(object sender, RoutedEventArgs e)
        {
            Button sp = sender as Button;
            Order o = sp.Tag as Order;
            MyApp.TableId = o.Table_id;
            BookPage bp = new BookPage(_parentWin,o);
            _parentWin.SidebarFrame.Content = bp;
        }

        /// <summary>
        /// 空闲桌子点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FreeTableClick(object sender, RoutedEventArgs e)
        {
            Button b=sender as Button;
            MyApp.TableId = Convert.ToInt64(b.Tag);
            FreeTablePage ftb=new FreeTablePage(_parentWin);
            _parentWin.SidebarFrame.Content = ftb;
        }
    }
}
