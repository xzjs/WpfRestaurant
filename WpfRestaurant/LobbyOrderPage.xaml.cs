using System;
using System.Collections.Generic;
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
        }

        private List<Table> GetTableList(int status)
        {
            using (var db = new restaurantEntities())
            {
                var tables = db.Table.Where(m => m.Status == status);
                int type = MyApp.TableType;
                if (type > 0)
                {
                    tables = tables.Where(m => m.Type == type);
                }
                return tables.ToList();
            }
        }
    }
}
