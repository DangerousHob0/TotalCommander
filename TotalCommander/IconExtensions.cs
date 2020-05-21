using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.IconPacks;

namespace TotalCommander
{
    public static class IconExtensions
    {
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);
        public static ImageSource ToImageSource(this Icon icon)
        {
            Bitmap bitmap = icon.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))
            {
                throw new Win32Exception();
            }

            return wpfBitmap;
        }

        public static ImageSource ToImageSource(this PackIconModernKind icon, System.Windows.Media.Brush brush=null)
        {
            GeometryDrawing geoDrawing = new GeometryDrawing();

            geoDrawing.Brush = brush == null? System.Windows.Media.Brushes.Black : brush;
            geoDrawing.Pen = new System.Windows.Media.Pen(geoDrawing.Brush, 0.25);

           
           geoDrawing.Geometry = Geometry.Parse(new PackIconModern { Kind = icon }.Data);


            var drawingGroup = new DrawingGroup { Children = { geoDrawing }, Transform = new ScaleTransform(1, 1) };

            return new DrawingImage { Drawing = drawingGroup };
        }
    }   
}