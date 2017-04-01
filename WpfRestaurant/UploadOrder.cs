using System.Collections.Generic;

namespace WpfRestaurant
{
    public class Menu
    {
        public int counter;
        public long menuId;
    }

    internal class UploadOrder
    {
        public decimal price;
        public long repastDeskId;
        public int restaurantId;
        public List<Menu> subOrderList;
        public string repastTimeStr;
    }
}