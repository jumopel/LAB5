using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HorseRacingSimulation.Services
{
    public class RenderService
    {
        public List<ImageSource> GetHorseAnimation(Color color)
        {
            const int count = 12; 
            var bitmap_image_list = ReadImageList("Images", "WithOutBorder_", ".png", count);
            var mask_image_list = ReadImageList("Images", "mask_", ".png", count);

            return bitmap_image_list.Select((item, index) =>
                GetImageWithColor(item, mask_image_list[index], color)).ToList();
        }

        private List<BitmapImage> ReadImageList(string path, string name, string format, int count)
        {
            string basePath = $"pack://application:,,,/{path}/{name}";
            List<BitmapImage> list = new List<BitmapImage>();

            for (int i = 0; i < count; i++)
            {
                var uri = basePath + string.Format("{0:0000}", i) + format;
                var img = new BitmapImage(new Uri(uri));
                list.Add(img);
            }
            return list;
        }

        private ImageSource GetImageWithColor(BitmapImage image, BitmapImage mask, Color color)
        {
            WriteableBitmap image_bmp = new WriteableBitmap(image);
            WriteableBitmap mask_bmp = new WriteableBitmap(mask);
            WriteableBitmap output_bmp = BitmapFactory.New(image.PixelWidth, image.PixelHeight);

            output_bmp.ForEach((x, y, c) =>
            {
                return MultiplyColors(image_bmp.GetPixel(x, y), color, mask_bmp.GetPixel(x, y).A);
            });

            return output_bmp;
        }

        private Color MultiplyColors(Color color_1, Color color_2, byte alpha)
        {
            var amount = alpha / 255.0;
            byte r = (byte)(color_2.R * amount + color_1.R * (1 - amount));
            byte g = (byte)(color_2.G * amount + color_1.G * (1 - amount));
            byte b = (byte)(color_2.B * amount + color_1.B * (1 - amount));

            return Color.FromArgb(color_1.A, r, g, b);
        }

        public BitmapSource GetRender(double width, double height)
        {
            var bitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
            var drawingVisual = new DrawingVisual();

            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
            }

            bitmap.Render(drawingVisual);
            return bitmap;
        }
    }
}
