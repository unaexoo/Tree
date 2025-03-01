using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics; // 시간측정

namespace Psychological_Test
{
    public partial class Step_Draw : Form
    {
        static private Step_Draw instance;
        bool isClick = false;
        Point point;
        Pen pen = new Pen(Color.Black, 1);
        Bitmap bitmap;
        Graphics graphics;
        int flag = 1; // 현재 선택된 도구의 종류 | {0 = null , 1 = Pencil, 2 = Erase, 3 = Paint_Bucket}
        Queue<Point> queue = new Queue<Point>();

        static public void ShowDraw()
        {
            instance.Show();
        }
        static public void closeDraw()
        {
            instance.Close();
        }
        public Step_Draw()
        {
            InitializeComponent();
            setBitmap();
            Selected_Color.BackColor = Color.Black;
            bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = bitmap;
            pictureBox2.Image = bitmap;
            instance = this;
        }
        void setBitmap()
        {
            bitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
            graphics = Graphics.FromImage(bitmap);
            graphics.Clear(BackColor);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;
        }

        private void New_button_Click(object sender, EventArgs e)
        {
            pictureBox1.Refresh();
            pictureBox1.Image = null;
            pictureBox2.Image = null;
            Bitmap bitmap = new Bitmap(1, 1);
            bitmap.SetPixel(0, 0, Color.White);
            pictureBox1.Image = bitmap;
            pictureBox2.Image = pictureBox1.Image;
        }

        private void Load_button_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "All Files(*.*) |*.*|Png Files (*.png ) |*.png |Bitmap File(*.bmp) | *.bmp |Jpeg File(*.jpg) | *.jpg";
            if (open.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = (Image)Image.FromFile(open.FileName).Clone();
                pictureBox2.Image = pictureBox1.Image;
            }
            open.Dispose();
        }

        private void Save_button_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "paint.png";
            saveFileDialog.Filter = "PNG File|*.png|Bitmap File|*.bmp|JPEG File|*.jpg";

            if (pictureBox1.Image == null)
            {
                return;
            }

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = (FileStream)saveFileDialog.OpenFile();
                switch (saveFileDialog.FilterIndex)
                {
                    case 1:
                        this.pictureBox1.Image.Save(fs, ImageFormat.Png);
                        break;

                    case 2:
                        this.pictureBox1.Image.Save(fs, ImageFormat.Bmp);
                        break;

                    case 3:
                        this.pictureBox1.Image.Save(fs, ImageFormat.Jpeg);
                        break;
                }
                fs.Dispose();
                fs.Close();
            }
        }

        private new Bitmap Capture()
        {
            Point x = new Point(this.Location.X + 8, this.Location.Y + 58);
            Point y = new Point(0, 0);
            //itmap bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics graphics = Graphics.FromImage(bitmap);

            graphics.CopyFromScreen(x, y, bitmap.Size);
            graphics.Save();
            //현재 프로젝트 위치에 저장됩니다.

            return bitmap;
        }

        private void Line_button_Click(object sender, EventArgs e)
        {

        }

        private void Pencil_button_Click(object sender, EventArgs e)
        {
            //Selected_Color.BackColor = Color.Black;
            if (flag == 2)
            {
                Color temp = Selected_Color.BackColor;
                Selected_Color.BackColor = Temp_Color.BackColor;
                Temp_Color.BackColor = temp;
            }
            flag = 1; // 1
        }

        private void Erase_button_Click(object sender, EventArgs e)
        {
            if (flag == 1)
            {
                Color temp = Temp_Color.BackColor;
                Temp_Color.BackColor = Selected_Color.BackColor;
            }
            Selected_Color.BackColor = Color.White;
            flag = 2;
        }

        private void Draw_Paint(object sender, PaintEventArgs e)
        {
            if (bitmap != null)
            {
                e.Graphics.DrawImage(bitmap, 0, 0);
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            isClick = true;
            point = new Point(e.X, e.Y);

            if (flag == 1 || flag == 2) // 연필, 지우개
            {
                Brush dot_brush = new SolidBrush(Selected_Color.BackColor);
                Graphics g = pictureBox1.CreateGraphics();
                g.FillEllipse(dot_brush, point.X - pen.Width / 2, point.Y - pen.Width / 2, pen.Width, pen.Width);
            }
            if (flag == 3) // 채우기
            {
                pictureBox2.Image = pictureBox1.Image;
                Color color = ((Bitmap)pictureBox1.Image).GetPixel(point.X, point.Y);
                Stack<Point> check = new Stack<Point>();
                check.Push(point);

                while (check.Count > 0)
                {
                    Point current = check.Pop();

                    if (current.X >= 0 && current.X < pictureBox2.Image.Width && current.Y >= 0 && current.Y < pictureBox2.Image.Height && ((Bitmap)pictureBox2.Image).GetPixel(current.X, current.Y) == color)
                    {
                        ((Bitmap)pictureBox2.Image).SetPixel(current.X, current.Y, Selected_Color.BackColor);

                        check.Push(new Point(current.X - 1, current.Y));
                        check.Push(new Point(current.X + 1, current.Y));
                        check.Push(new Point(current.X, current.Y - 1));
                        check.Push(new Point(current.X, current.Y + 1));
                    }
                }

                pictureBox1.Image = pictureBox2.Image;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isClick = false;

            int cnt = queue.Count();
            PointF[] points = new PointF[cnt];
            for (int i = 0; 0 < queue.Count(); i++)
            {
                points[i] = queue.Dequeue();
            }
            Graphics g = pictureBox1.CreateGraphics();
            if (cnt > 1)
            {
                //Graphics g = pictureBox1.CreateGraphics();
                //g.DrawLines(pen, points);

                Brush dot_brush = new SolidBrush(Selected_Color.BackColor);
                g.FillEllipse(dot_brush, point.X - pen.Width / 2, point.Y - pen.Width / 2, pen.Width, pen.Width);
                pictureBox1.Image = Capture();
            }
            queue.Clear();
            //pictureBox1.Image = Capture();
            pictureBox2.Image = pictureBox1.Image;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isClick == true)
            {
                Point currentPoint = new Point(e.X, e.Y);
                pen.Color = Selected_Color.BackColor;
                if (flag == 1 || flag == 2)
                {
                    Graphics g = pictureBox1.CreateGraphics();
                    g.DrawLine(pen, point, currentPoint);
                    point = currentPoint;
                    queue.Enqueue(currentPoint);

                    Brush dot_brush = new SolidBrush(Selected_Color.BackColor);
                    g.FillEllipse(dot_brush, point.X - pen.Width / 2, point.Y - pen.Width / 2, pen.Width, pen.Width);
                }
            }
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {

        }

        internal void End_button_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("종료하시겠습니까?", "종료", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }
        
        
        private void Brown_Button_Click(object sender, EventArgs e)
        {
            if (flag == 2) return;
            Selected_Color.BackColor = Color.Brown;
        }

        private void Fill_Button_Click(object sender, EventArgs e)
        {
            flag = 3;
        }

        private void Green_Button_Click(object sender, EventArgs e)
        {
            if (flag == 2) return;
            Selected_Color.BackColor = Color.Green;
        }

        private void DarkOliveGreen_button_Click(object sender, EventArgs e)
        {
            if (flag == 2) return;
            Selected_Color.BackColor = Color.DarkOliveGreen;
        }

        private void LimeGreen_button_Click(object sender, EventArgs e)
        {
            if (flag == 2) return;
            Selected_Color.BackColor = Color.LimeGreen;
        }

        private void YellowGreen_button_Click(object sender, EventArgs e)
        {
            if (flag == 2) return;
            Selected_Color.BackColor = Color.YellowGreen;
        }

        private void SaddleBrown_button_Click(object sender, EventArgs e)
        {
            if (flag == 2) return;
            Selected_Color.BackColor = Color.SaddleBrown;
        }

        private void Sienna_button_Click(object sender, EventArgs e)
        {
            if (flag == 2) return;
            Selected_Color.BackColor = Color.Sienna;
        }

        private void Maroon_button_Click(object sender, EventArgs e)
        {
            if (flag == 2) return;
            Selected_Color.BackColor = Color.Maroon;
        }

        private void Black_button_Click(object sender, EventArgs e)
        {
            if (flag == 2) return;
            Selected_Color.BackColor = Color.Black;
        }

        private void White_Button_Click(object sender, EventArgs e)
        {
            if (flag == 2) return;
            Selected_Color.BackColor = Color.White;
        }

        private void pen_size1_Click(object sender, EventArgs e)
        {
            pen.Width = 1;
        }

        private void pen_size5_Click(object sender, EventArgs e)
        {
            pen.Width = 5;
        }

        private void pen_size10_Click(object sender, EventArgs e)
        {
            pen.Width = 10;
        }

        private void pen_size16_Click(object sender, EventArgs e)
        {
            pen.Width = 16;
        }

        private void pen_size20_Click(object sender, EventArgs e)
        {
            pen.Width = 20;
        }

        private void pen_size25_Click(object sender, EventArgs e)
        {
            pen.Width = 25;
        }

        private void pen_size50_Click(object sender, EventArgs e)
        {
            pen.Width = 50;
        }

        private void pen_size70_Click(object sender, EventArgs e)
        {
            pen.Width = 70;
        }

        private void pen_size90_Click(object sender, EventArgs e)
        {
            pen.Width = 90;
        }

        private void pen_size100_Click(object sender, EventArgs e)
        {
            pen.Width = 100;
        }
    
        private void Colors_Enter(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Color temp = Selected_Color.BackColor;
            Selected_Color.BackColor = Temp_Color.BackColor;
            Temp_Color.BackColor = temp;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null || pictureBox2.Image != null)
            {
                Stopwatch stopwatch = new Stopwatch(); //객체 선언
                stopwatch.Start(); // 시간측정 시작
                Bitmap bmp = (Bitmap)pictureBox2.Image;
                Step_loading loading = new Step_loading(bmp);
                this.Hide();
                loading.Show();
                int loading_ch = loading.jaeuk();
                if(loading_ch != 0) 
                {
                    System.Console.WriteLine("문제발생...\n");

                    loading.Close();
                    stopwatch.Stop(); //시간측정 끝
                    System.Console.WriteLine("time : " + stopwatch.ElapsedMilliseconds + "ms");
                    return;
                }
                loading.Close();
                stopwatch.Stop(); //시간측정 끝

                System.Console.WriteLine("time : " + stopwatch.ElapsedMilliseconds + "ms");
            }
        }
    }
}

