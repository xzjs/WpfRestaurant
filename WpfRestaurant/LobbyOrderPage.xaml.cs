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
        public LobbyOrderPage()
        {
            InitializeComponent();

            List<MyTable> lmt = new List<MyTable>()
            {
                new MyTable() {No="A001",Status="已下单" ,Paid=1},
                new MyTable() {No="A002",Status="已下单" ,Paid=0},
                new MyTable() {No="A003",Status="已下单" ,Paid=0},
                new MyTable() {No="A004",Status="已下单" ,Paid=0}
            };
            BusyTableList.ItemsSource = lmt;
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

        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StackPanel sp = sender as StackPanel;
            OrderPage op = new OrderPage();
            op.SetTableNo(sp.Tag.ToString());
            ParentWin.SidebarFrame.Content = op;
        }
    }
}
