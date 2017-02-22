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
    /// FreeTablePage.xaml 的交互逻辑
    /// </summary>
    public partial class FreeTablePage : Page
    {
        private MainWindow mainWindow;
        public FreeTablePage(MainWindow mw)
        {
            InitializeComponent();

            mainWindow = mw;
            SetTableNo();
        }

        public void SetTableNo()
        {
            using(var db=new restaurantEntities())
            {
                Table t = db.Table.Find(MyApp.tableId);
                tableNoTextblock.Text = t.No;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using(var db=new restaurantEntities())
            {
                MenuWindow mw = new MenuWindow(mainWindow);
                mw.ShowDialog();
            }
        }
    }
}
