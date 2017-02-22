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
    /// OrderPage.xaml 的交互逻辑
    /// </summary>
    public partial class OrderPage : Page
    {
        private MainWindow mainWindow;
        private Table table;
        private List<Bill> listBill;
        public OrderPage(MainWindow _pw)
        {
            InitializeComponent();
            mainWindow = _pw;
            using(var db=new restaurantEntities())
            {
                table = db.Table.Find(MyApp.tableId);
                Order o = db.Order.Where(x => x.Table_id == MyApp.tableId).OrderByDescending(x => x.Id).First();
                listBill = o.Bill.ToList();
            }
            BillDataGrid.ItemsSource = listBill;
            tableNoTextblock.Text = table.No;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MenuWindow mw = new MenuWindow(mainWindow);
            mw.ShowDialog();
        }
    }
}
