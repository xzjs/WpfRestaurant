using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfRestaurant
{
    public class Menu
    {
        public long MenuId;
        public int Counter;
    }
    class UploadOrder
    {
        public int RestaurantId;
        public long RepastDeskId;
        public decimal Price;
        public List<Menu> SubOrderList;
    }
}
