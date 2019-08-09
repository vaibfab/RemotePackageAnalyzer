using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace GUI
{
    [ValueConversion(typeof(string),typeof(BitmapImage))]
    public class ImgConverter : IValueConverter
    {
        public static ImgConverter oConverter = new ImgConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = (string)value;
            if (path == null)
                return null;
            var imgPath = "Images/folder.png";
            var name = MainWindow.GetFolderName(path);
            if(String.IsNullOrEmpty(path))
                imgPath = "Images/folder.png";
            
            else if(new FileInfo(path).Extension.Equals(".cs"))
                imgPath = "Images/file.jpg";
            return new BitmapImage(new Uri($"pack://application:,,,/{imgPath}"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
