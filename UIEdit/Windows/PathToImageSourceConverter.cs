using System;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UIEdit.Windows {
    public class PathToImageSourceConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            value = value ?? "";
            if (targetType != typeof(ImageSource))
                throw new InvalidOperationException("Target type must be System.Windows.Media.ImageSource.");

            var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + ((string) value).Replace(".dds", ".png").Replace(".DDS", ".png");
            if (!File.Exists(filePath)) {
                using (var memory = new MemoryStream()) {
                    Properties.Resources.errorpic.Save(memory, ImageFormat.Png);
                    memory.Position = 0;
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    return bitmapImage;
                }
            }

            try {
                var img = new BitmapImage();
                img.BeginInit();
                img.UriSource = new Uri(filePath);
                img.EndInit();
                return img;
            } catch (Exception) {
                using (var memory = new MemoryStream()) {
                    Properties.Resources.errorpic.Save(memory, ImageFormat.Png);
                    memory.Position = 0;
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    return bitmapImage;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return new BitmapImage();
        }
    }
}
