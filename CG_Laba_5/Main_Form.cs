using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using static System.Collections.Specialized.BitVector32;

namespace CG_Laba_5
{
    public partial class Main_Form : Form
    {
        float pictureBoxWidth;
        float pictureBoxHeight;
        Graphics g;
        Pen axisPen = new Pen(Color.Black, 2);
        Pen gridPen = new Pen(Color.Black, 0.5f);
        Font Fon = new Font("Arial", 9, FontStyle.Regular);
        Brush brush = new SolidBrush(Color.Black);
        Brush fillArea = new SolidBrush(Color.Red);
        Brush noFillArea = new SolidBrush(Color.Gray);
        float divX;
        float divY;
        const int countX = 20;
        const int countY = 20;
        float centerX, centerY;
        int xPartition;
        List<List<int>> area = new List<List<int>>();
        public Main_Form()
        {
            InitializeComponent();
            g = pictureBox1.CreateGraphics();
            pictureBoxWidth = pictureBox1.Width;
            pictureBoxHeight = pictureBox1.Height;
            divX = pictureBoxWidth / countX;
            divY = pictureBoxHeight / countY;
            centerX = pictureBoxWidth / 2;
            centerY = pictureBoxHeight / 2;
            xPartition = 13;
        }

        private void DrawCurrentIteration(List<Point> points)
        {
            FullFill(area);
        }
        private async void Test()
        {
            DrawAxis();
            DrawGrid();
            List<Point> points = InputFile();

            for (int i = 0; i < countX; i++)
            {
                area.Add(new List<int>());
                for (int j = 0; j < countY; j++)
                {
                    area[i].Add(0); // Инициализация белым цветом
                }
            }

            // Отрисовка всех линий и заполнение их в area
            for (int i = 0; i < points.Count; i++)
            {
                int x1 = points[i].X;
                int y1 = points[i].Y;
                int x2 = points[(i + 1) % points.Count].X; // Обработка замкнутого контура
                int y2 = points[(i + 1) % points.Count].Y;

                List<Point> bresenhamPoints = BresenhamLine(x1, y1, x2, y2);
                foreach (Point point in bresenhamPoints)
                {
                    area[point.X][point.Y] = 1;
                }
                FillSquare(Brushes.Black, Convert(bresenhamPoints));
            }

            // Добавление черной перегородки
            for (int i = 0; i < countY; i++)
            {
                area[xPartition][i] = 1; // Черная перегородка
            }

            OutputFile1("2");

            Point lastPoint = Point.Empty;
            bool last = false;

            for (int i = 0; i < points.Count; i++)
            {
                if (i == points.Count - 1) last = true;
                Point startPoint = points[i];
                Point endPoint = points[(i + 1) % points.Count]; // Обработка замкнутого контура
                if (startPoint.Y == endPoint.Y) continue;
                Filling(startPoint, endPoint, lastPoint, last, points);
                lastPoint = startPoint;
                await Task.Delay(300);
                DrawCurrentIteration(points);
            }

            // Повторная отрисовка всех линий для финализации изображения
            for (int i = 0; i < points.Count; i++)
            {
                int x1 = points[i].X;
                int y1 = points[i].Y;
                int x2 = points[(i + 1) % points.Count].X; // Обработка замкнутого контура
                int y2 = points[(i + 1) % points.Count].Y;

                List<Point> bresenhamPoints = BresenhamLine(x1, y1, x2, y2);
                foreach (Point point in bresenhamPoints)
                {
                    area[point.X][point.Y] = 1;
                }
                FillSquare(Brushes.Black, Convert(bresenhamPoints));
            }

            OutputFile1("1");
        }


        private List<PointF> Convert(List<PointF> point)
        {
            List<PointF> result = new List<PointF>();
            for (int i = 0; i < point.Count; i++)
            {
                result.Add(Convert(point[i]));
            }
            return result;
        }
        private List<Point> Convert(List<Point> point)
        {
            List<Point> result = new List<Point>();
            for (int i = 0; i < point.Count; i++)
            {
                result.Add(Convert(point[i]));
            }
            return result;
        }
        private PointF Convert(PointF point)
        {
            return new PointF(divX + (point.X * divX), (pictureBoxHeight - divY) - point.Y * divY);
        }
        private Point Convert(Point point)
        {
            return new Point((int)(divX + (point.X * divX)), (int)((pictureBoxHeight - divY) - point.Y * divY));
        }

        private void Filling(Point p1, Point p2, Point p1Last, bool last, List<Point> points)
        {
            List<Point> bresenhamLine = BresenhamLine(p1.X, p1.Y, p2.X, p2.Y);

            // Если направление поменялось
            if (p1Last != Point.Empty && Math.Sign(p1.Y - p1Last.Y) != Math.Sign(p1.Y - p2.Y))
            {
                bresenhamLine = bresenhamLine.Where(p => p.Y != p1.Y).ToList();
            }

            // Если это последний сегмент, обрабатываем его правильно
            if (last)
            {
                Point secondFig = new Point(points[1].X, points[1].Y);
                if (p1Last != Point.Empty && Math.Sign(p2.Y - secondFig.Y) != Math.Sign(p2.Y - p1.Y))
                {
                    bresenhamLine = bresenhamLine.Where(p => p.Y != p2.Y).ToList();
                }
            }

            List<Point> leftPoints = new List<Point>();
            List<Point> rightPoints = new List<Point>();

            // Разделяем точки на левые и правые относительно перегородки
            foreach (Point point in bresenhamLine)
            {
                if (point.X < xPartition)
                {
                    leftPoints.Add(point);
                }
                else
                {
                    rightPoints.Add(point);
                }
            }

            Point lastPoint = Point.Empty;

            // Заполнение левой части
            for (int i = 0; i < leftPoints.Count; i++)
            {
                if (lastPoint != Point.Empty && lastPoint.Y == leftPoints[i].Y) continue;
                while (leftPoints[i].X < xPartition)
                {
                    InvertPixel(leftPoints[i].X, leftPoints[i].Y);
                    Point tmp = leftPoints[i];
                    leftPoints[i] = new Point(tmp.X + 1, tmp.Y);
                }
                lastPoint = leftPoints[i];
            }
            lastPoint = Point.Empty;

            // Заполнение правой части
            for (int i = 0; i < rightPoints.Count; i++)
            {
                if (lastPoint != Point.Empty && lastPoint.Y == rightPoints[i].Y) continue;
                while (rightPoints[i].X > xPartition)
                {
                    InvertPixel(rightPoints[i].X, rightPoints[i].Y);
                    Point tmp = rightPoints[i];
                    rightPoints[i] = new Point(tmp.X - 1, tmp.Y);
                }
                lastPoint = rightPoints[i];
            }
        }


        private void InvertPixel(int x, int y)
        {
            if (area[x][y] != 1)
            {
                area[x][y] = 1; // изменяем на черный, если не черный
            }
            else
            {
                area[x][y] = 0; // иначе белый
            }    
        }
        public void FillSquare(Brush brush, List<Point> points)
        {
            foreach (var point in points)
            {
                g.FillRectangle(brush, point.X, point.Y, divX, divY);
            }
        }

        public async void FullFill(List<List<int>> area)
        {
            for(int j = 0; j < area.Count; j++)
            {
                for (int i = 0; i < area.Count; i++)
                {
                    //await Task.Delay(25);
                    if (area[i][j] == 1)
                    {
                        g.FillRectangle(Brushes.Black, (int)(divX + (int)(i * divX)), (int)((pictureBoxHeight - divY) - (int)(j * divY)), divX, divY);
                    }
                    else
                    {
                        g.FillRectangle(Brushes.White, (int)(divX + (int)(i * divX)), (int)((pictureBoxHeight - divY) - (int)(j * divY)), divX, divY);
                    }    
                }
            }
        }

        private void OutputFile1(string a)
        {
            try
            {
                //Pass the filepath and filename to the StreamWriter Constructor
                StreamWriter sw = new StreamWriter("output" + a + ".txt");
                //Write a line of text
                for (int i = 0; i < area.Count; i++)
                {
                    for (int j = 0; j < area[i].Count; j++)
                    {
                        sw.Write(area[i][j]);
                    }
                    sw.Write('\n');
                }
                //Close the file
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }
        private void DrawAxis()
        {
            g.Clear(Color.White);
            PointF axisXStart = new PointF(divX, pictureBoxHeight - divY);
            PointF axisXEnd = new PointF(pictureBoxWidth, pictureBoxHeight - divY);
            PointF axisYStart = new PointF(divX, 0);
            PointF axisYEnd = new PointF(divX, pictureBoxHeight - divY);
            g.DrawLine(axisPen, axisXStart, axisXEnd);
            g.DrawLine(axisPen, axisYStart, axisYEnd);
            for (int i = 1; i <= countY; i++)
            {
                g.DrawString((i).ToString(), Fon, brush, divX - 17, pictureBoxHeight + divY * -i - divY);
            }
            for (int i = 1; i <= countX; i++)
            {
                g.DrawString(i.ToString(), Fon, brush, divX * i + 5, pictureBoxHeight - 15);
            }
        }
        private void DrawGrid()
        {
            for (int i = 0; i <= countY; i++)
            {
                g.DrawLine(gridPen, 0, divY * i, pictureBoxWidth, divY * i);
            }
            for (int i = 0; i <= countX; i++)
            {
                g.DrawLine(gridPen, divX * i, 0, divX * i, pictureBoxHeight);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Test();
        }

        List<Point> BresenhamLine(int x0, int y0, int x1, int y1)
        {
            List<Point> bresenhamPoints = new List<Point>();
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                bresenhamPoints.Add(new Point(x0, y0));
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
            return bresenhamPoints;
        }


        private void ClearCanvas()
        {
            pictureBox1.Refresh();
            g.Clear(Color.White);
        }

        List<Point> InputFile()
        {
            string line;
            List<Point> points = new List<Point>();
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader("input.txt");
                //Read the first line of text
                line = sr.ReadLine();
                //Continue to read until you reach end of file
                while (line != null)
                {
                    string[] coordinates = line.Split(' ');
                    int x = int.Parse(coordinates[0]);
                    int y = int.Parse(coordinates[1]);
                    points.Add(new Point(x, y));
                    line = sr.ReadLine();
                }
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            return points;
        }
    }
}