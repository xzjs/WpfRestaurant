using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WpfRestaurant
{
    class MyConverter
    {
    }

    /// <summary>
    /// 状态转换器
    /// </summary>
    public class StatusConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int status = (int)value;
            string s = "空闲";
            switch (status)
            {
                case 1:
                    s = "预约";
                    break;
                case 2:
                    s = "已下单";
                    break;
                default:
                    break;
            }
            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = (string)value;
            int i = 0;
            switch (status)
            {
                case "预约":
                    i = 1;
                    break;
                case "已下单":
                    i = 2;
                    break;
                default:
                    break;
            }
            return i;
        }
    }
}
