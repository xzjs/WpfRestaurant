using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace WpfRestaurant
{
    class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapImage bitmap;
            string img = value.ToString();
            if (img == "menu.png")
            {
                string src = "pack://application:,,,/pic/" + img;
                bitmap = new BitmapImage(new Uri(src));
            }
            else
            {
                string src = Path.Combine(Directory.GetCurrentDirectory(), img);
                bitmap = new BitmapImage(new Uri(src));
            }
            
            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
