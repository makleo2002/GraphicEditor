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
using System.IO;
public static class RenderVisualService
{
    private const double defaultDpi = 96.0;

    private static BitmapSource GetRenderTargetBitmapFromControl(Visual targetControl, double dpi = defaultDpi)
        {
            if (targetControl == null) return null;

            var bounds = VisualTreeHelper.GetDescendantBounds(targetControl);
            var renderTargetBitmap = new RenderTargetBitmap((int)(bounds.Width * dpi / 96.0),
                                                            (int)(bounds.Height * dpi / 96.0),
                                                            dpi,
                                                            dpi,
                                                            PixelFormats.Pbgra32);

            var drawingVisual = new DrawingVisual();

            using (var drawingContext = drawingVisual.RenderOpen())
            {
                var visualBrush = new VisualBrush(targetControl);
                drawingContext.DrawRectangle(visualBrush, null, new Rect(new Point(), bounds.Size));
            }

            renderTargetBitmap.Render(drawingVisual);
            return renderTargetBitmap;
        }
    public static void RenderToPNGFile(Visual targetControl, string filename)
    {
        var renderTargetBitmap = GetRenderTargetBitmapFromControl(targetControl);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

        var result = new BitmapImage();

        try
        {
            using (var fileStream = new FileStream(filename, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"There was an error saving the file: {ex.Message}");
        }
    }
}

     


namespace VectorGraphicEditor
{
   
    public partial class MainWindow : Window
    {
        Pen[] pen;
        Line[] lines;
        int point_index = 0;
        Line line_cur = null;
        bool pen_mode = false;
        String mode="";
        bool mb_press;
        Point currentPoint = new Point();
        int cnt = 0, lcnt = -1;
        double x1, y1;
        public MainWindow()
        {
            lines = new Line[100];
            x1 = y1 = 0;
            InitializeComponent();
            pen = new Pen[100];
            for (int i = 0; i < 100; i++)
                pen[i]=new Pen(SystemColors.WindowFrameBrush,2f);
            paintSurface.Background = new SolidColorBrush(Colors.White);
            mb_press = false;
        }

        private void paintSurface_MouseUp(object sender, MouseButtonEventArgs e)
        {
           
            if (!pen_mode)
            {
                mb_press = false;
                cnt++;
            }
                    
           
        }
        private void paintSurface_MouseDown(object sender,MouseButtonEventArgs e)
        {
            if (pen_mode)
            {
                x1 = e.GetPosition(this).X;
                y1 = e.GetPosition(this).Y;
            }
            else
            {
                double marker_x = -1, marker_y = -1;
                mb_press = true;

                if (e.LeftButton == MouseButtonState.Pressed)
                    currentPoint = e.GetPosition(this);
                Line line;
                foreach (Line line1 in paintSurface.Children.OfType<Line>())
                {
                    line = line1;
                    if (Math.Abs(line.X1 - e.GetPosition(this).X) < 5 && Math.Abs(e.GetPosition(this).Y - 91 - line.Y1) < 5)
                    {
                        point_index = 0;
                        marker_x = line.X1;
                        marker_y = line.Y1;
                        line_cur = line;
                    }
                    else if (Math.Abs(line.X2 - e.GetPosition(this).X) < 5 && Math.Abs(e.GetPosition(this).Y - 91 - line.Y2) < 5)
                    {
                        point_index = 1;
                        marker_x = line.X2;
                        marker_y = line.Y2;
                        line_cur = line;
                    }
                }

            }

        }
        private void paintSurface_MouseMove(object sender,MouseEventArgs e)
        {
            if (pen_mode)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Line line_ = new Line();
                    line_.X1 = e.GetPosition(this).X;
                    line_.Y1 = e.GetPosition(this).Y - 91;
                    line_.X2 = x1;
                    line_.Y2 = y1 - 91;
                    line_.StrokeThickness = pen[cnt].Thickness;
                    line_.Stroke = pen[cnt].Brush;

                    paintSurface.Children.Add(line_);

                    for(int i = 0; i < lines.Length-1; i++)
                    {
                        if (lines[i] == null)
                        {
                            lines[i] = line_;
                            break;
                        }
                    }
                    x1 = e.GetPosition(this).X;
                    y1 = e.GetPosition(this).Y;
                }
                  


            }
            else {
                double marker_x = -1, marker_y = -1;
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Line line = null;
                    if (line_cur != null)
                    {
                        if (point_index == 0)
                        {
                            line_cur.X1 = e.GetPosition(this).X;
                            line_cur.Y1 = e.GetPosition(this).Y - 91;
                            marker_x = line_cur.X1;
                            marker_y = line_cur.Y1;
                            Console.WriteLine("Правлю точку 0");
                        }
                        else
                        {
                            line_cur.X2 = e.GetPosition(this).X;
                            line_cur.Y2 = e.GetPosition(this).Y - 91;
                            marker_x = line_cur.X2;
                            marker_y = line_cur.Y2;
                            Console.WriteLine("Правлю точку 1");
                        }
                        for (int i = 0; i < 10; i++)
                            foreach (System.Windows.Shapes.Rectangle rect1 in paintSurface.Children.OfType<System.Windows.Shapes.Rectangle>())
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
                            line.Y1 = currentPoint.Y - 91;
                            line.X2 = e.GetPosition(this).X;
                            line.Y2 = e.GetPosition(this).Y - 91;

                            currentPoint = e.GetPosition(this);
                            line.Name = "line_" + cnt;
                            line.Stroke = pen[cnt].Brush;
                            line.StrokeThickness = pen[cnt].Thickness;
                            paintSurface.Children.Add(line);
                            for (int i = 0; i < lines.Length-1; i++)
                            {
                                if (lines[i] == null)
                                {
                                    lines[i] = line;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            line.X2 = e.GetPosition(this).X;
                            line.Y2 = e.GetPosition(this).Y - 91;
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
                        if ((Math.Abs(line.X1 - e.GetPosition(this).X) < 5) && (Math.Abs(e.GetPosition(this).Y - 91 - line.Y1) < 5))
                        {
                            point_index = 0;
                            marker_x = line.X1;
                            marker_y = line.Y1;
                            line_cur = line;
                        }
                        else if ((Math.Abs(line.X2 - e.GetPosition(this).X)) < 5 && (Math.Abs(e.GetPosition(this).Y - 91 - line.Y2) < 5))
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
           

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (mode != "Erase")
            {
                for (int i = cnt; i < 100; i++)
                    pen[i].Brush = ((Button)sender).Background;
            }
          
           
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            for (int i = cnt; i < 100; i++)
                pen[i].Thickness = slider.Value;
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            RenderVisualService.RenderToPNGFile(paintSurface, "myawesomeimage.png");
           
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            paintSurface.Children.Clear();
        }

        private void eraser_Click(object sender, RoutedEventArgs e)
        {
            if (mode == "")
            {
                mode = "Erase";
                for (int i = cnt; i < 100; i++)
                    pen[i].Brush = new SolidColorBrush(Colors.White);
            }
            
            else if(mode=="Erase")
            {
                mode = "";
                for (int i = cnt; i < 100; i++)
                    pen[i].Brush = new SolidColorBrush(Colors.Black);
            }
            
               
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (pen_mode) pen_mode = false;
            else pen_mode = true;
        }
        private void NewFile(object sender, RoutedEventArgs e)
        {
            paintSurface.Background = new SolidColorBrush(Colors.White);
        }
        private void OpenFile(object sender, RoutedEventArgs e)
        {
            paintSurface.Background = new SolidColorBrush(Colors.White);
        }
        private void SaveFile(object sender, RoutedEventArgs e)
        {
            RenderVisualService.RenderToPNGFile(paintSurface, "myawesomeimage.png");
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Line line_to_del=null;
            for(int i = lines.Length-1; i>=0; i--)
                    {
                        if (lines[i] != null)
                        {
                           line_to_del= lines[i];
                            break;
                        }
                    }
            foreach (Line line1 in paintSurface.Children.OfType<Line>())
            {
                if(line1==line_to_del)
                    paintSurface.Children.Remove(line_to_del);
            }

        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            RenderVisualService.RenderToPNGFile(paintSurface, "myawesomeimage.png");
            Close();
        }
    }

}
