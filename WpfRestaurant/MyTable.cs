using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfRestaurant
{
   
    class MyTable
    {
        private string _no;
        private string _status;
        private int _paid;

        public string No
        {
            get
            {
                return _no;
            }

            set
            {
                _no = value;
            }
        }

        public string Status
        {
            get
            {
                return _status;
            }

            set
            {
                _status = value;
            }
        }

        public int Paid
        {
            get
            {
                return _paid;
            }

            set
            {
                _paid = value;
            }
        }
    }
}
