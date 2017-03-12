using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfRestaurant
{
    internal class MyConverter
    {
    }

    /// <summary>
    ///     状态转换器
    /// </summary>
    public class StatusConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (int) value;
            var s = "空闲";
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
            var status = (string) value;
            var i = 0;
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