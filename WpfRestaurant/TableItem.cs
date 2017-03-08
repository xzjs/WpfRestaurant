using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfRestaurant
{
    public class TableItem
    {
        public Table Table { get; set; }

        public string Time { get; set; }

        public string No { get; set; }

        public decimal Cost { get; set; }

        public Order Order { get; set; }
    }
}
