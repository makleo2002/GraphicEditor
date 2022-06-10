using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyGraphicEditor
{
    public partial class Form1 : Form
    {
        int cnt = 0;
        Point[][] p;
        bool mdown;
        bool point_focused;
        int catch_line_lindex;
        int catch_point_lindex;
        public Form1()
        {
            catch_line_lindex = -1;
            catch_point_lindex = -1;
            point_focused = false;
            mdown = false;
            InitializeComponent();
            p = new Point[100][];
            for (int i = 0; i < 100; i++)
                p[i] = new Point[2];
        }

     
        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            mdown = false;
            p[cnt][1].X = e.X;
            p[cnt][1].Y = e.Y;
            cnt++;
            panel1.Invalidate();
        }
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            mdown = true;
            p[cnt][0].X = e.X;
            p[cnt][0].Y = e.Y;
        }
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mdown)
            {
                mdown = false;
                p[cnt][1].X = e.X;
                p[cnt][1].Y = e.Y;
                panel1.Invalidate();
            }
            else
            {
                point_focused = false;
                catch_point_lindex = -1;
                catch_line_lindex = -1;
                for (int i = 0; i < cnt; i++)
                {
                    if(Math.Abs(p[i][0].X-e.X)<5 && Math.Abs(p[i][0].Y - e.Y) < 5)
                    {
                        point_focused = true;
                        catch_point_lindex = 0;
                        catch_line_lindex = i;
                    }
                    if (Math.Abs(p[i][1].X - e.X) < 5 && Math.Abs(p[i][1].Y - e.Y) < 5)
                    {
                        point_focused = true;
                        catch_point_lindex = 1;
                        catch_line_lindex = i;
                    }
                }
            }
            panel1.Invalidate();
        }
        private void panel1_Paint(object sender,PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (point_focused)
            {
                g.DrawRectangle(new Pen(Color.Red), p[catch_line_lindex][catch_point_lindex].X - 5, p[catch_line_lindex][catch_point_lindex].Y - 5, p[catch_line_lindex][catch_point_lindex].X + 5, p[catch_line_lindex][catch_point_lindex].Y + 5);
            }
            
            for(int i = 0; i < cnt; i++)
            {
                g.DrawLine(new Pen(Color.Black), p[i][0].X, p[i][0].Y, p[i][1].X, p[i][1].Y);
            }
            if (mdown)
            {
                g.DrawLine(new Pen(Color.Black), p[cnt][0].X, p[cnt][0].Y, p[cnt][1].X, p[cnt][1].Y);
            }
        }

       
    }
}
