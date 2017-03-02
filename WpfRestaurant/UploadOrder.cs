using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfRestaurant
{
    public class Menu
    {
        public long menuId;
        public int counter;
    }
    class UploadOrder
    {
        public int restaurantId;
        public long repastDeskId;
        public decimal price;
        public List<Menu> subOrderList;
    }
}
