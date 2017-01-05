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
        public FreeTablePage()
        {
            InitializeComponent();
        }

        public void SetTableNo(string table_no)
        {
            tableNoTextblock.Text = table_no;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MenuWindow mw = new MenuWindow();
            mw.ShowDialog();
        }
    }
}
