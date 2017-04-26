using System.Windows;
using System.Windows.Controls;

namespace WpfRestaurant
{
    /// <summary>
    ///     FreeTablePage.xaml 的交互逻辑
    /// </summary>
    public partial class FreeTablePage : Page
    {
        private readonly MainWindow _mainWindow;

        public FreeTablePage(MainWindow mw)
        {
            InitializeComponent();

            _mainWindow = mw;
            SetTableNo();
        }

        /// <summary>
        ///     设置桌号
        /// </summary>
        public void SetTableNo()
        {
            using (var db = new restaurantEntities())
            {
                var t = db.Table.Find(MyApp.TableId);
                TableNoTextblock.Text = t.No;
                string[] typeStrings = {"大厅", "小包间", "大包间"};
                TypeTextBlock.Text = typeStrings[t.Type - 1];
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var o = new Order();
            var mw = new MenuWindow(_mainWindow, o);
            mw.ShowDialog();
        }

        private void Close_Page(object sender, RoutedEventArgs e)
        {
            _mainWindow.SidebarFrame.Content = null;
        }
    }
}