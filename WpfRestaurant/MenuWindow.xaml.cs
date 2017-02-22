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
using System.Windows.Shapes;

namespace WpfRestaurant
{
    /// <summary>
    /// MenuWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MenuWindow : Window
    {
        private List<Bill> listBill;
        private MainWindow mainWindow;

        public MenuWindow(MainWindow mw)
        {
            InitializeComponent();

            mainWindow = mw;
            listBill = new List<Bill>();

            using(var db=new restaurantEntities())
            {
                List<Food> lf = db.Food.ToList();
                foreach (var item in lf)
                {
                    Bill b = new Bill
                    {
                        Food = item,
                        Order_id = 0,
                        Num = 0
                    };
                    listBill.Add(b);
                }
            }
            FoodListBind();
        }

        public void FoodListBind(int type = 1)
        {
            List<Bill> lb = listBill.Where(x => x.Food.Type == type).ToList();
            FoodListBox.ItemsSource = lb;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            int type = Convert.ToInt32(b.Tag);
            string title = "";
            switch (type)
            {
                case 1:
                    title = "热菜";
                    break;
                case 2:
                    title = "凉菜";
                    break;
                case 3:
                    title = "主食";
                    break;
                case 4:
                    title = "饮品";
                    break;
                case 5:
                    title = "香烟";
                    break;
                default:
                    break;
            }
            this.Title = title;
            FoodListBind(type);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            long id = Convert.ToInt64((sender as Button).Tag);
            Bill b = listBill.Where(x => x.Food.Id == id).First();
            b.Num++;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            long id = Convert.ToInt64((sender as Button).Tag);
            Bill b = listBill.Where(x => x.Food.Id == id).First();
            if (b.Num > 0)
            {
                b.Num--;
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new restaurantEntities())
                {
                    Order o = new Order
                    {
                        Table_id = MyApp.tableId,
                        Time = DateTime.Now,
                        Type = 0,
                        Cost = 0
                    };
                    db.Order.Add(o);
                    db.SaveChanges();
                    foreach (var item in listBill)
                    {
                        if (item.Num > 0)
                        {
                            Bill b = new Bill
                            {
                                Food_id = item.Food.Id,
                                Order_id = o.Id,
                                Num = item.Num,
                                Price = item.Food.Price * item.Num
                            };
                            db.Bill.Add(b);
                        }
                    }
                    db.SaveChanges();
                    OrderPage op = new OrderPage(mainWindow);
                    mainWindow.SidebarFrame.Content = op;
                    Table t = db.Table.Find(MyApp.tableId);
                    t.Status = 2;
                    db.SaveChanges();
                    this.Close();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
    }
}
