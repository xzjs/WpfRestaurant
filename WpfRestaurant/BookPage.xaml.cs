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
    /// BookPage.xaml 的交互逻辑
    /// </summary>
    public partial class BookPage : Page
    {
        private Order _order;
        private MainWindow _mainWindow;
        public BookPage(MainWindow mainWindow, Order order)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _order = order;
            BookStackPanel.DataContext = _order;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OrderPage op=new OrderPage(_mainWindow);
            _mainWindow.SidebarFrame.Content = op;
            TableItem.SetTableStatus(2,MyApp.TableId);
            _mainWindow.Lop.GetList();
        }
    }
}
