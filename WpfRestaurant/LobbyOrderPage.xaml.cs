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
        private List<Table> _flt, _blt;
        public LobbyOrderPage()
        {
            InitializeComponent();

            GetList();
        }

        public MainWindow ParentWin
        {
            get
            {
                return _parentWin;
            }

            set
            {
                _parentWin = value;
            }
        }

        /// <summary>
        /// 忙碌餐桌点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StackPanel sp = sender as StackPanel;
            MyApp.TableId = Convert.ToInt64(sp.Tag);
            OrderPage op = new OrderPage(_parentWin);
            _parentWin.Op = op;
            ParentWin.SidebarFrame.Content = op;
        }

        /// <summary>
        /// 空闲餐桌点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            StackPanel sp = sender as StackPanel;
            MyApp.TableId = Convert.ToInt64(sp.Tag);
            FreeTablePage ftb = new FreeTablePage(_parentWin);
            ParentWin.SidebarFrame.Content = ftb;
        }

        public void GetList()
        {
            BusyTableList.ItemsSource = GetTableList(2);
            FreeTableList.ItemsSource = GetTableList(0);
            OrderTableList.ItemsSource = GetTableList(1);
        }

        /// <summary>
        /// 预定桌位点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_MouseLeftButtonUp_2(object sender, MouseButtonEventArgs e)
        {
            StackPanel sp = sender as StackPanel;
            Order o=sp.Tag as Order;
            BookPage bp = new BookPage(o);
            ParentWin.SidebarFrame.Content = bp;
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
    }
}
