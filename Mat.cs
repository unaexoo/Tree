using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;

namespace Psychological_Test
{
    // _Point 는 두개의 정수 X, Y를 저장하는 변수가 있다.
    // _Point 클래스 간 덧셈과 뺄셈을 수행하는 연산자 오버로딩만 제공한다.
    // _Point 값을 문자열로 반환하는 함수를 가지고 있다.
    class _Point
    {
        public int X { get; set; }
        public int Y { get; set; }
        public _Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        public static _Point operator +(_Point p1, _Point p2)
        {
            return new _Point(p1.X + p2.X, p1.Y + p2.Y);
        }
        public static _Point operator -(_Point p1, _Point p2)
        {
            return new _Point(p1.X - p2.X, p1.Y - p2.Y);
        }
        public String toString()
        {
            return "x: " + X + ", y: " + Y;
        }
    }


    public partial class Mat
    {
        sbyte[,] array; // [Y,X]
        private int width;
        private int height;
        private sbyte[,] matrix
        {
            get { return array; }
            set { array = value; }
        }
        private int X
        {
            get { return this.width; }
            set { this.width = value; }
        }
        
        private int Y
        {
            get { return this.height; }
            set  { this.height = value; }
        }
        public int len
        {
            get { return this.width * this.height; }
        }

        public sbyte this[int _x, int _y]
        {
            get { return matrix[_y,_x]; }
            set { matrix[_y, _x] = value; }
        }
        public Mat(int _w = 0, int _h = 0)// 기본생성자
        {
            X = _w; // x
            Y = _h; // y
            matrix = new sbyte[Y, X];
        }
        public Mat(Bitmap img) // 비트맵을 가인수로 받는 생성자
        {
            X = img.Width;
            Y = img.Height;
            matrix = new sbyte[Y, X];
            Color color;
            for (int y = 0; y < Y; y++)
            {
                for (int x = 0; x < X; x++)
                {
                    color = img.GetPixel(x, y);
                    if (color == Color.White)
                    {
                        matrix[y, x] = -1;
                    }
                    else // if ((color != Color.White)
                    {
                        matrix[y, x] = 1;
                    }
                }
            }
        }
        public Mat(Mat val) // 복사생성자
        {
            X = val.X;
            Y = val.Y;
            matrix = new sbyte[Y, X];
            for (int y = 0; y < Y; y++)
            {
                for (int x = 0; x < X; x++)
                {
                    matrix[y, x] = val.matrix[y, x];
                }
            }
        }

        void Clear()
        {
            array = null;
            width = 0;
            height = 0;
        }
        public static Mat Resize(Mat val, int dst_X, int dst_Y)
        {
            int end = dst_X * dst_Y;
            Mat res = new Mat(dst_Y, dst_X);
            for (int pixel = 0; pixel < end; pixel++)
            {
                res[pixel / dst_X, pixel % dst_X] = val.matrix[pixel / val.X, pixel % val.X];
            }
            return res;
        }
        static Mat add(Mat lval, Mat rval)
        {
            Mat res = new Mat(lval.Y, lval.X);
            if (lval.X == rval.X && lval.Y == rval.Y)
            {
                for (int y = 0; y < lval.Y; y++)
                {
                    for (int x = 0; x < lval.X; x++)
                    {
                        res.matrix[y, x] = (sbyte)(lval.matrix[y, x] + rval.matrix[y, x]);
                    }
                }
            }
            return res;
        }
        public static Mat operator +(Mat lval, Mat rval)
        {
            Mat matrix = new Mat(add(lval, rval));
            return matrix;
        }
        static Mat sub(Mat lval, Mat rval)
        {
            Mat res = new Mat(lval.Y, lval.X);
            if (lval.X == rval.X && lval.Y == rval.Y)
            {
                for (int y = 0; y < lval.Y; y++)
                {
                    for (int x = 0; x < lval.X; x++)
                    {
                        res.matrix[y, x] = (sbyte)(lval.matrix[y, x] - rval.matrix[y, x]);
                    }
                }
            }
            return res;
        }
        public static Mat operator -(Mat lval, Mat rval)
        {
            Mat matrix = new Mat(sub(lval, rval));
            return matrix;
        }
        static Mat mul(Mat lval, Mat rval)
        {
            sbyte sum;
            Mat res = new Mat(lval.Y, rval.X);
            if (lval.Y == rval.X)
                for (int x = 0; x < lval.X; x++)
                {
                    for (int y = 0; y < rval.Y; y++)
                    {
                        sum = 0;
                        for (int i = 0; i < lval.X; i++)
                        {
                            sum += (sbyte)(lval.matrix[y, i] * rval.matrix[i, x]);
                        }
                        res.matrix[y, x] = sum;
                    }
                }
            return res;
        }
        public static Mat operator *(Mat lval, Mat rval)
        {            
            Mat matrix = new Mat(mul(lval, rval));
            return matrix;
        }

        public Mat partial_Mat(int _y,int _x) 
        {
            Mat res = new Mat(_y,_x);
            for(int i = 0; i < _y; i++)
            {
                for(int j = 0; j < _x; j++)
                {
                    res[i,j] = matrix[i,j];
                }
            }
            return res;
        }
    }

    class _Hopfile
    {
        Mat Weight;
        public _Hopfile() { }
        
        public void training(Mat val) // [n,1]
        {
            Mat Wn = val * Mat.Resize(val, 1, val.len);
            Weight += Wn;
        }
        public void test()
        {

        }
        void Clear()
        {
            Weight = null;
        }
    }
}
