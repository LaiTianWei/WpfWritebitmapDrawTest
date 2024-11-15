using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Drawing.Drawing2D;
using Point = System.Drawing.Point;
using Image = System.Windows.Controls.Image;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Security.Policy;

namespace Wpfwritebitmap
{
    public static class DrawingUtil
    {
        public static System.Drawing.SolidBrush CreateBrush(System.Windows.Media.Color color)
        {
            return new System.Drawing.SolidBrush(ConvertColor(color));
        }
        public static System.Drawing.Color ConvertColor(System.Windows.Media.Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
        public static System.Drawing.Rectangle ConvertRect(System.Windows.Int32Rect rect)
        {
            return new System.Drawing.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", CharSet = CharSet.Ansi)]
        public extern static long CopyMemory(IntPtr dest, IntPtr source, int size);


        public WriteableBitmap SourceBackground { get; private set; }

        public WriteableBitmap Source { get; private set; }


        public int DataLength { get; private set; }

        private readonly Bitmap backBitmap;

        private readonly Bitmap bitmapTest = new Bitmap("D:\\Images\\images\\largebw1.tif");

        private readonly Graphics graphics;
        public Int32Rect SourceRect { get; private set; }
        public System.Drawing.Rectangle BitmapRect { get; private set; }

        private readonly System.Drawing.Imaging.PixelFormat pixelFormat;


        public bool Continued = true;

        public MainWindow()
        {
            InitializeComponent();

            pixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppArgb;


            Source = new WriteableBitmap(1024, 1024, 96, 96, PixelFormats.Pbgra32, null);
            img.Source = Source;

            DataLength = Source.BackBufferStride * Source.PixelHeight;
            SourceRect = new Int32Rect(0, 0, Source.PixelWidth, Source.PixelHeight);
            BitmapRect = DrawingUtil.ConvertRect(SourceRect);
          
            backBitmap = new Bitmap(Source.PixelWidth, Source.PixelHeight, pixelFormat);
            graphics = Graphics.FromImage(backBitmap);

            SourceBackground = new WriteableBitmap(1024, 1024, 96, 96, PixelFormats.Pbgra32, null);
            imgBg.Source = SourceBackground;
            UpdateBackground();
        }


        private void BtnBengin_Click(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
        }


        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            Source.Lock();
            FillGraphToBitmap();
            Source.AddDirtyRect(new Int32Rect(0, 0, Source.PixelWidth, Source.PixelHeight));
            Source.Unlock();
        }


        protected void FillGraphToBitmap()
        {
            graphics.Clear(System.Drawing.Color.Transparent);
            var path = new GraphicsPath
            {
                FillMode = FillMode.Winding
            };
            AddPolyLines(path, Source.PixelWidth, Source.PixelHeight);

            //graphics.DrawImage(bitmapTest, 0, 0, Source.PixelWidth, Source.PixelHeight);

            graphics.DrawPath(new System.Drawing.Pen(System.Drawing.Color.Green, 1f), path);

            graphics.Flush();
            BitmapData block = backBitmap.LockBits(BitmapRect, ImageLockMode.ReadOnly, pixelFormat);
            MemoryCopy(block.Scan0, Source.BackBuffer, DataLength);
            backBitmap.UnlockBits(block);
        }


        private unsafe void MemoryCopy(IntPtr src, IntPtr dst, long size)
        {
            Buffer.MemoryCopy(src.ToPointer(), dst.ToPointer(), size, size);
        }


        protected void AddPolyLines(GraphicsPath gPath, int X, int Y)
        {
            Random rx = new Random();
            for (int i = 0; i < 30; i++)
            {
                Point p1 = new Point(rx.Next(X), rx.Next(Y));
                Point p2 = new Point(rx.Next(X), rx.Next(Y));
                gPath.AddLine(p1, p2);
            }
        }

        private void UpdateBackground()
        {

            graphics.Clear(System.Drawing.Color.Transparent);
            graphics.DrawImage(bitmapTest, 0, 0, Source.PixelWidth, Source.PixelHeight);

            SourceBackground.Lock();
            long length = SourceBackground.BackBufferStride * SourceBackground.PixelHeight;

            BitmapData block = backBitmap.LockBits(BitmapRect, ImageLockMode.ReadOnly, pixelFormat);
            MemoryCopy(block.Scan0, SourceBackground.BackBuffer, length);
            backBitmap.UnlockBits(block);

            SourceBackground.AddDirtyRect(new Int32Rect(0, 0, SourceBackground.PixelWidth, SourceBackground.PixelHeight));
            SourceBackground.Unlock();
        }



    }
}
