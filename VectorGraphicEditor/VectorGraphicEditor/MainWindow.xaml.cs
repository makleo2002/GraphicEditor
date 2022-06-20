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
using System.Diagnostics;


namespace VectorGraphicEditor
{
    
    public partial class MainWindow : Window
    {
        
        Pen[] pen;
        List<ImageBrush> Img;
        List<List<Line>> freelines;
        int point_index = 0;
        Line line_cur = null;
        bool pen_mode = false;
        String mode = "line";
        Point currentPoint = new Point();
        int cnt = 0, l_cnt = 0;
        double x1, y1;

        private Point startPoint;
        private Point finalPoint;
        private Rectangle rectangle;
        private Ellipse circle;
        public MainWindow()
        {
            Img = new List<ImageBrush>(100);
            freelines = new List<List<Line>>(100);
            for (int i = 0; i < 100; i++)
                freelines.Add(new List<Line>(100));
            x1 = y1 = 0;
            InitializeComponent();
            pen = new Pen[100];
            for (int i = 0; i < 100; i++)
                pen[i] = new Pen(SystemColors.WindowFrameBrush, 2f);
            paintSurface.Background = new SolidColorBrush(Colors.White);


        }

        private void paintSurface_MouseUp(object sender, MouseButtonEventArgs e)
        {


            circle = null;
            rectangle = null;

            if(mode=="pen") l_cnt++;
              else cnt++;
           



        }
        private void paintSurface_MouseDown(object sender, MouseButtonEventArgs e)
        {


            if (mode == "rectangle")
            {
                startPoint = e.GetPosition(paintSurface);

                rectangle = new Rectangle
                {
                    Stroke = pen[cnt].Brush,
                    StrokeThickness = pen[cnt].Thickness
                };
                Canvas.SetLeft(rectangle, startPoint.X);
                Canvas.SetTop(rectangle, startPoint.Y);
                paintSurface.Children.Add(rectangle);

            }
            else if (mode == "circle")
            {
                startPoint = e.GetPosition(paintSurface);

                circle = new Ellipse
                {
                    Stroke = pen[cnt].Brush,
                    StrokeThickness = pen[cnt].Thickness
                };
                Canvas.SetLeft(circle, startPoint.X);
                Canvas.SetTop(circle, startPoint.Y);
                paintSurface.Children.Add(circle);

            }
            else if (mode == "triangle")
            {
                startPoint = e.GetPosition(paintSurface);

                finalPoint.X = e.GetPosition(paintSurface).X;
                finalPoint.Y = e.GetPosition(paintSurface).Y+30;
                double smX = startPoint.X < finalPoint.X ? startPoint.X : finalPoint.X;
                double bgX = startPoint.X < finalPoint.X ? finalPoint.X : startPoint.X;

                double smY = startPoint.Y < finalPoint.Y ? startPoint.Y : finalPoint.Y;
                double bgY = startPoint.Y < finalPoint.Y ? finalPoint.Y : startPoint.Y;

                Line line1 = new Line();
                Line line2 = new Line();
                Line line3 = new Line();
                line1.X1 = smX;
                line1.Y1 = bgY;
                line2.X1 = bgX;
                line2.Y1 = bgY;
                line3.X1 = smX + ((bgX - smX) / 2);
                line3.Y1 = smY;
                line1.Stroke = pen[cnt].Brush;
                line1.StrokeThickness = pen[cnt].Thickness;
                line2.Stroke = pen[cnt].Brush;
                line2.StrokeThickness = pen[cnt].Thickness;
                line3.Stroke = pen[cnt].Brush;
                line3.StrokeThickness = pen[cnt].Thickness;

                paintSurface.Children.Add(line1);
                paintSurface.Children.Add(line2);
                paintSurface.Children.Add(line3);
            }
            

           else if (mode=="pen")
            {

                x1 = e.GetPosition(this).X;
                y1 = e.GetPosition(this).Y;
            }
            else
            {
                double marker_x = -1, marker_y = -1;

                if (e.LeftButton == MouseButtonState.Pressed)
                    currentPoint = e.GetPosition(this);
                Line line;
                foreach (Line line1 in paintSurface.Children.OfType<Line>())
                {
                    line = line1;
                    if (Math.Abs(line.X1 - e.GetPosition(this).X) < 5 && Math.Abs(e.GetPosition(this).Y - 106 - line.Y1) < 5)
                    {
                        point_index = 0;
                        marker_x = line.X1;
                        marker_y = line.Y1;
                        line_cur = line;
                    }
                    else if (Math.Abs(line.X2 - e.GetPosition(this).X) < 5 && Math.Abs(e.GetPosition(this).Y - 106 - line.Y2) < 5)
                    {
                        point_index = 1;
                        marker_x = line.X2;
                        marker_y = line.Y2;
                        line_cur = line;
                    }
                }

            }

        }
        private void paintSurface_MouseMove(object sender, MouseEventArgs e)
        {
            if (mode == "pen")
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {

                    Line line_ = new Line();
                    line_.X1 = e.GetPosition(this).X;
                    line_.Y1 = e.GetPosition(this).Y - 106;
                    line_.X2 = x1;
                    line_.Y2 = y1 - 106;
                    line_.StrokeThickness = pen[cnt].Thickness;
                    line_.Stroke = pen[cnt].Brush;

                    paintSurface.Children.Add(line_);

                    freelines[l_cnt].Add(line_);


                    x1 = e.GetPosition(this).X;
                    y1 = e.GetPosition(this).Y;
                }



            }
            else
            {
                double marker_x = -1, marker_y = -1;
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Line line = null;
                    if (line_cur != null&&mode=="line")
                    {
                        if (point_index == 0)
                        {
                            line_cur.X1 = e.GetPosition(this).X;
                            line_cur.Y1 = e.GetPosition(this).Y - 106;
                            marker_x = line_cur.X1;
                            marker_y = line_cur.Y1;
                            Console.WriteLine("Правлю точку 0");
                        }
                        else
                        {
                            line_cur.X2 = e.GetPosition(this).X;
                            line_cur.Y2 = e.GetPosition(this).Y - 106;
                            marker_x = line_cur.X2;
                            marker_y = line_cur.Y2;
                            Console.WriteLine("Правлю точку 1");
                        }


                        for (int i = 0; i < 10; i++)
                            foreach (System.Windows.Shapes.Rectangle rect1 in paintSurface.Children.OfType<System.Windows.Shapes.Rectangle>())
                            {
                                if (rect1.Width == 10)
                                {
                                    paintSurface.Children.Remove(rect1);
                                    break;
                                }

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
                            if (mode == "rectangle")
                            {
                                if (e.LeftButton == MouseButtonState.Released || rectangle == null)
                                    return;

                                var pos = e.GetPosition(paintSurface);

                                var x = Math.Min(pos.X, startPoint.X);
                                var y = Math.Min(pos.Y, startPoint.Y);

                                var w = Math.Max(pos.X, startPoint.X) - x;
                                var h = Math.Max(pos.Y, startPoint.Y) - y;

                                rectangle.Width = w;
                                rectangle.Height = h;

                                Canvas.SetLeft(rectangle, x);
                                Canvas.SetTop(rectangle, y);

                            }
                            else if (mode == "circle")
                            {
                                if (e.LeftButton == MouseButtonState.Released || circle == null)
                                    return;

                                var pos = e.GetPosition(paintSurface);

                                var x = Math.Min(pos.X, startPoint.X);
                                var y = Math.Min(pos.Y, startPoint.Y);

                                var w = Math.Max(pos.X, startPoint.X) - x;
                                var h = Math.Max(pos.Y, startPoint.Y) - y;

                                circle.Width = w;
                                circle.Height = h;

                                Canvas.SetLeft(circle, x);
                                Canvas.SetTop(circle, y);

                            }
                            else if (mode == "triangle")
                            {
                                finalPoint.Y = e.GetPosition(paintSurface).Y + 30;
                                if (e.LeftButton == MouseButtonState.Released)
                                    return;


                                double smX = startPoint.X < finalPoint.X ? startPoint.X : finalPoint.X;
                                double bgX = startPoint.X < finalPoint.X ? finalPoint.X : startPoint.X;

                                double smY = startPoint.Y < finalPoint.Y ? startPoint.Y : finalPoint.Y;
                                double bgY = startPoint.Y < finalPoint.Y ? finalPoint.Y : startPoint.Y;

                                Line line1 = new Line();
                                Line line2 = new Line();
                                Line line3 = new Line();
                                line1.X1 = smX;
                                line1.Y1 = bgY;
                                line2.X1 = bgX;
                                line2.Y1 = bgY;
                                line3.X1 = smX + ((bgX - smX) / 2);
                                line3.Y1 = smY;

                                line1.Stroke = pen[cnt].Brush;
                                line1.StrokeThickness = pen[cnt].Thickness;
                                line2.Stroke = pen[cnt].Brush;
                                line2.StrokeThickness = pen[cnt].Thickness;
                                line3.Stroke = pen[cnt].Brush;
                                line3.StrokeThickness = pen[cnt].Thickness;

                                paintSurface.Children.Add(line1);
                                paintSurface.Children.Add(line2);
                                paintSurface.Children.Add(line3);


                            }
                            else
                            {
                                line = new Line();
                                line.Stroke = SystemColors.WindowFrameBrush;
                                line.X1 = currentPoint.X;
                                line.Y1 = currentPoint.Y - 106;
                                line.X2 = e.GetPosition(this).X;
                                line.Y2 = e.GetPosition(this).Y - 106;

                                currentPoint = e.GetPosition(this);
                                line.Name = "line_" + cnt;
                                line.Stroke = pen[cnt].Brush;
                                line.StrokeThickness = pen[cnt].Thickness;
                                paintSurface.Children.Add(line);
                            }


                        }
                        else
                        {
                            line.X2 = e.GetPosition(this).X;
                            line.Y2 = e.GetPosition(this).Y - 106;
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
                        if ((Math.Abs(line.X1 - e.GetPosition(this).X) < 5) && (Math.Abs(e.GetPosition(this).Y - 106 - line.Y1) < 5))
                        {
                            point_index = 0;
                            marker_x = line.X1;
                            marker_y = line.Y1;
                            line_cur = line;
                        }
                        else if ((Math.Abs(line.X2 - e.GetPosition(this).X)) < 5 && (Math.Abs(e.GetPosition(this).Y - 106 - line.Y2) < 5))
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
                        if (rect.Width == 10)
                        {
                            paintSurface.Children.Remove(rect);
                            line_cur = null;
                            paintSurface.InvalidateVisual();
                            break;
                        }

                    }


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
                pen[i].Thickness = slider.Value;
        }


        private void NewFile(object sender, RoutedEventArgs e)
        {
            paintSurface.Background = new SolidColorBrush(Colors.White);
            paintSurface.Children.Clear();
        }
        private void OpenFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dl1 = new Microsoft.Win32.OpenFileDialog();
            dl1.FileName = "MYFileSave";
            dl1.DefaultExt = ".png";
            dl1.Filter = "Image documents (.png)|*.png";
            Nullable<bool> result = dl1.ShowDialog();

            if (result == true)
            {
                string filename = dl1.FileName;
                ImageBrush brush = new ImageBrush();
                brush.ImageSource = new BitmapImage(new Uri(@filename, UriKind.Relative));
                paintSurface.Background = brush;
            }
        }
        public static void ToImageSource(Canvas canvas, string filename)
        {
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, 96d, 96d, PixelFormats.Pbgra32);
            canvas.Measure(new Size((int)canvas.ActualWidth, (int)canvas.ActualHeight));
            canvas.Arrange(new Rect(new Size((int)canvas.ActualWidth, (int)canvas.ActualHeight)));
            bmp.Render(canvas);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));

            using (FileStream file = File.Create(filename))
            {
                encoder.Save(file);
            }
        }
        private void SaveFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveimg = new Microsoft.Win32.SaveFileDialog();
            //Canvas can = new Canvas();  // канвас уже есть
            saveimg.DefaultExt = ".PNG";
            saveimg.Filter = "Image (.PNG)|*.PNG";
            if (saveimg.ShowDialog() == true)
            {
                ToImageSource(paintSurface, saveimg.FileName);  //DragArena  - имя имеющегося канваса
            }
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            SaveFile(sender, e);
            Close();
        }

        private void line_Click(object sender, RoutedEventArgs e)
        {

            mode = "line";

        }

        private void circle_Click(object sender, RoutedEventArgs e)
        {

            mode = "circle";
        }

        private void rectangle_Click(object sender, RoutedEventArgs e)
        {

            mode = "rectangle";
        }

        private void triangle1_Click(object sender, RoutedEventArgs e)
        {

            mode = "triangle";
        }
        private void bucket_Click(object sender, RoutedEventArgs e)
        {
            mode = "fill";
        }
        private void eraser_Click(object sender, RoutedEventArgs e)
        {

            mode = "pen";

            for (int i = cnt; i < 100; i++)
            {
                pen[i].Brush = new SolidColorBrush(Colors.White);
                pen[i].Thickness = 3f;
            }

        }

       

        private void undo_Click_(object sender, RoutedEventArgs e)
        {
            if (paintSurface.Children.Count != 0)
                paintSurface.Children.RemoveAt(paintSurface.Children.Count - 1);
            else return;


        }

        private void Pencil_Click_(object sender, RoutedEventArgs e)
        {
            mode = "pen";

            for (int i = 0; i < 100; i++)
                pen[i] = new Pen(SystemColors.WindowFrameBrush, 2f);
        }



    }

}
