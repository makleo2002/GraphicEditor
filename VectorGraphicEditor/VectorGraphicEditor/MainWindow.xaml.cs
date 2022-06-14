using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Microsoft.Win32;
namespace VectorGraphicEditor
{
   
    public partial class MainWindow : Window
    {
        Pen[] pen;
        int point_index = 0;
        Line line_cur = null;
        String mode;
        bool mb_press;
        Point currentPoint = new Point();
        int cnt = 0;
        public MainWindow()
        {
            InitializeComponent();
            pen = new Pen[100];
            for (int i = 0; i < 100; i++)
                pen[i]=new Pen(SystemColors.WindowFrameBrush,2f);
            mode = "Рисую линию";
            mb_press = false;
        }

        private void paintSurface_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mb_press = false;
            cnt++;
        }
        private void paintSurface_MouseDown(object sender,MouseButtonEventArgs e)
        {
            double marker_x = -1, marker_y = -1;
            mb_press = true;
            if (e.LeftButton == MouseButtonState.Pressed)
                currentPoint = e.GetPosition(this);
            Line line;
            foreach (Line line1 in paintSurface.Children.OfType<Line>()) 
            {
                line = line1;
                if(Math.Abs(line.X1-e.GetPosition(this).X)<5&& Math.Abs(e.GetPosition(this).Y-100-line.Y1 ) < 5)
                {
                    point_index = 0;
                    marker_x = line.X1;
                    marker_y = line.Y1;
                    line_cur = line;
                }
                else if(Math.Abs(line.X2 - e.GetPosition(this).X) < 5 && Math.Abs(e.GetPosition(this).Y-100 - line.Y2) < 5)
                {
                    point_index = 1;
                    marker_x = line.X2;
                    marker_y = line.Y2;
                    line_cur = line;
                }
            }
        }
        private void paintSurface_MouseMove(object sender,MouseEventArgs e)
        {
            double marker_x = -1, marker_y = -1;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Line line=null;
                if (line_cur != null)
                {
                    if (point_index == 0)
                    {
                        line_cur.X1 = e.GetPosition(this).X;
                        line_cur.Y1 = e.GetPosition(this).Y-100;
                        marker_x = line_cur.X1;
                        marker_y = line_cur.Y1;
                        Console.WriteLine("Правлю точку 0");
                    }
                    else
                    {
                        line_cur.X2 = e.GetPosition(this).X;
                        line_cur.Y2 = e.GetPosition(this).Y - 100;
                        marker_x = line_cur.X2;
                        marker_y = line_cur.Y2;
                        Console.WriteLine("Правлю точку 1");
                    }
                    foreach(System.Windows.Shapes.Rectangle rect1 in paintSurface.Children.OfType<System.Windows.Shapes.Rectangle>())
                    {
                        paintSurface.Children.Remove(rect1);
                        break;
                    }
                    System.Windows.Shapes.Rectangle rect;
                    rect = new System.Windows.Shapes.Rectangle();
                    rect.Stroke = new SolidColorBrush(Colors.Red);
                    rect.Fill = new SolidColorBrush(Colors.Transparent);
                    rect.Width = 10;
                    rect.Height = 10;
                    Canvas.SetLeft(rect, marker_x - 5);
                    Canvas.SetTop(rect, marker_y - 5);
                    paintSurface.Children.Add(rect);
                    return;
                }
                else
                {
                    foreach (Line line1 in paintSurface.Children.OfType<Line>())
                    {
                        line = line1;
                        if (line.Name == "line_" + cnt)
                        {
                            break;
                        }
                        else line = null;
                    }
                }
                if (line_cur == null)
                {
                    if (line == null)
                    {
                        line = new Line();
                        line.Stroke = SystemColors.WindowFrameBrush;
                        line.X1 = currentPoint.X;
                        line.Y1 = currentPoint.Y - 100;
                        line.X2 = e.GetPosition(this).X;
                        line.Y2 = e.GetPosition(this).Y - 100;

                        currentPoint = e.GetPosition(this);
                        line.Name = "line_" + cnt;
                       line.Stroke = pen[cnt].Brush;
                        line.StrokeThickness = pen[cnt].Thickness;
                        paintSurface.Children.Add(line);
                    }
                    else
                    {
                        line.X2 = e.GetPosition(this).X;
                        line.Y2 = e.GetPosition(this).Y - 100;
                        paintSurface.InvalidateVisual();
                    }
                }
            }
            else
            {
                Line line = null;
                foreach (Line line1 in paintSurface.Children.OfType<Line>())
                {
                    line = line1;
                    if (Math.Abs(line.X1 - e.GetPosition(this).X) < 5 && Math.Abs(e.GetPosition(this).Y - line.Y1) < 5)
                    {
                        point_index = 0;
                        marker_x = line.X1;
                        marker_y = line.Y1;
                        line_cur = line;
                    }
                    else if (Math.Abs(line.X2 - e.GetPosition(this).X) < 5 && Math.Abs(e.GetPosition(this).Y - line.Y2) < 5)
                    {
                        point_index = 1;
                        marker_x = line.X2;
                        marker_y = line.Y2;
                        line_cur = line;
                    }
                }
            }
            if (marker_x != -1)
            {
                System.Windows.Shapes.Rectangle rect;
                rect = new System.Windows.Shapes.Rectangle();
                rect.Stroke = new SolidColorBrush(Colors.Red);
                rect.Fill = new SolidColorBrush(Colors.Transparent);
                rect.Width = 10;
                rect.Height = 10;
                Canvas.SetLeft(rect, marker_x - 5);
                Canvas.SetTop(rect, marker_y - 5);
                paintSurface.Children.Add(rect);
            }
            else
            {
               
                    foreach (Rectangle rect in paintSurface.Children.OfType<Rectangle>())
                    {
                        paintSurface.Children.Remove(rect);
                        line_cur = null;
                        paintSurface.InvalidateVisual();
                        break;
                    }
               
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = cnt; i < 100; i++)
                pen[i].Brush = ((Button)sender).Background;
           
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            for (int i = cnt; i < 100; i++)
             pen[i].Thickness=slider.Value;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)paintSurface.RenderSize.Width,
(int)paintSurface.RenderSize.Height, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(paintSurface);

            var crop = new CroppedBitmap(rtb, new Int32Rect(50, 50, 250, 250));

            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(crop));

            using (var fs = System.IO.File.OpenWrite("logo.png"))
            {
                pngEncoder.Save(fs);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            paintSurface.Children.Clear();
        }
    }
}
