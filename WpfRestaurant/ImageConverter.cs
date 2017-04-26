using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace WpfRestaurant
{
    internal class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapImage bitmap;
            var img = value.ToString();
            if (img == "menu.png")
            {
                var src = "pack://application:,,,/pic/" + img;
                bitmap = new BitmapImage(new Uri(src));
            }
            else
            {
                var src = Path.Combine(Directory.GetCurrentDirectory(), img);
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