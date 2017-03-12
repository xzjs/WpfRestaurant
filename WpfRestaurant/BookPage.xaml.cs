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
    }
}