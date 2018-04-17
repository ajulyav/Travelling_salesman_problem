using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;



namespace TSP_PictureBox_v2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region Глобальные переменные

        Graphics g;
        Pen p1, p2, PenRed;

        // Списки, в котрых хранятся коондинаты городов
        List<int> X = new List<int>();
        List<int> Y = new List<int>();

        TSPpath salesman;

        #endregion


        #region Обработчики событий

        private void Form1_Load(object sender, EventArgs e)
        {
            g = pictureBox1.CreateGraphics();
            p1 = new Pen(Color.Black, 5);
            p2 = new Pen(Color.LightGray, 1);
            PenRed = new Pen(Color.Red, 3);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            X.Add(e.X);
            Y.Add(e.Y);

            textBox1.Text = X.Count.ToString();

            if (checkBox1.Checked) DrawPath();
            DrawTowns();

            if (X.Count >= 3) toolStripStatusLabel1.Text = "Total Path = " + (Factorial(X.Count - 1)) / 2;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int towns, Xmax, Ymax;
            Random rnd = new Random();


            if (textBox1.Text != "")
            {
                Clear();
                towns = Convert.ToInt32(textBox1.Text);
                Xmax = pictureBox1.Size.Width - 10;
                Ymax = pictureBox1.Size.Height - 10;

                for (int i = 0; i < towns; i++)
                {
                    X.Add(rnd.Next(10, Xmax));
                    Y.Add(rnd.Next(10, Ymax));
                }

                if (checkBox1.Checked) DrawAllPath();
                DrawTowns();
            }


            if (X.Count >= 3) toolStripStatusLabel1.Text = "Total Path = " + (Factorial(X.Count - 1)) / 2;

            /*Clear();
            cities = new Cities(Convert.ToInt32(textBox1.Text), pictureBox1.Size);

            for (int i = 0; i < cities.Coordinate.Length; i++)
            {
                X.Add(cities.Coordinate[i].X);
                Y.Add(cities.Coordinate[i].Y);
            }

            if (checkBox1.Checked) DrawAllPath();
            DrawTowns();

            if (X.Count >= 3) toolStripStatusLabel1.Text = "Total Path = " + (Factorial(X.Count - 1)) / 2;*/
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
            {
                g.Clear(pictureBox1.BackColor);
                DrawTowns();
            }
            else
            {
                DrawAllPath();
                DrawTowns();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            salesman = new TSPpath(X.ToArray(), Y.ToArray());

            Stopwatch sw = new Stopwatch();
            sw.Start();

            salesman.FindBestPath();

            sw.Stop();

            toolStripStatusLabel2.Text = "         Time = " + sw.Elapsed.TotalSeconds + " сек.";

            g.Clear(pictureBox1.BackColor);
            if (checkBox1.Checked) DrawAllPath();
            DrawBestPath();
            DrawTowns();
        }

        Evolution ga;
        private void RunGA_Click(object sender, EventArgs e)
        {
            
            Series plotBest = chart1.Series[0];

            // Очистка графика Chart1
            plotBest.Points.Clear();

            // Создаем объект
            ga = new Evolution(Convert.ToInt32(textBox1.Text),
                                  Convert.ToDouble(Mutation_textBox.Text)/100,
                                  X.ToArray(),
                                  Y.ToArray());

            // Генерим популяцию
            ga.Generate(Convert.ToInt32(PopulationSize.Text));

            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Запускаем ГА
            for (int i = 0; i < Convert.ToInt32(Generations_textBox.Text); i++)
            {
                ga.StepGA();
                plotBest.Points.AddXY(i, ga.NextPopulation[ga.BestChromosomeIndex].Fitness);
            }

            sw.Stop();
            toolStripStatusLabel2.Text = "         Time = " + sw.Elapsed.TotalSeconds + " сек.";


            // Рисуем лучший найденый путь
            g.Clear(pictureBox1.BackColor);
            if (checkBox1.Checked) DrawAllPath();
            for (int i = 0; i < ga.NextPopulation[0].Path.Length - 1; i++)
            {
                g.DrawLine(PenRed, X[ga.NextPopulation[ga.BestChromosomeIndex].Path[i]], Y[ga.NextPopulation[ga.BestChromosomeIndex].Path[i]],
                                   X[ga.NextPopulation[ga.BestChromosomeIndex].Path[i + 1]], Y[ga.NextPopulation[ga.BestChromosomeIndex].Path[i + 1]]);
            }
            DrawTowns();
        }


        private void button4_Click(object sender, EventArgs e)
        {
            //g.Clear(pictureBox1.BackColor);
            //if (checkBox1.Checked) DrawAllPath();
            //DrawBestPath();
            //DrawTowns();

            g.Clear(pictureBox1.BackColor);
            if (checkBox1.Checked) DrawAllPath();
            // Рисыем лучший найденый путь
            for (int i = 0; i < ga.NextPopulation[0].Path.Length - 1; i++)
            {
                g.DrawLine(PenRed, X[ga.NextPopulation[ga.BestChromosomeIndex].Path[i]], Y[ga.NextPopulation[ga.BestChromosomeIndex].Path[i]],
                                   X[ga.NextPopulation[ga.BestChromosomeIndex].Path[i + 1]], Y[ga.NextPopulation[ga.BestChromosomeIndex].Path[i + 1]]);
            }
            DrawTowns();
        }

        #endregion


        #region Функции отрисовки графики

        private void Clear()
        {
            g.Clear(pictureBox1.BackColor);

            // Очищаем списки, в котрых хранятся коондинаты городов
            X.Clear();
            Y.Clear();

            toolStripStatusLabel1.Text = "Total Path = ";
        }

        // Рисует все возможные пути между одним городом и остальными городами
        private void DrawPath()
        {
            for (int i = 0; i < X.Count; i++)
                g.DrawLine(p2, X[i], Y[i], X[X.Count - 1], Y[X.Count - 1]);
        }

        // Рисует все возможные пути между всеми городами
        private void DrawAllPath()
        {
            for (int i = 0; i < X.Count; i++)
            {
                for (int j = i + 1; j < X.Count; j++)
                {
                    g.DrawLine(p2, X[i], Y[i], X[j], Y[j]);
                }
            }
        }

        // Рисует города в виде кружков
        private void DrawTowns()
        {
            for (int i = 0; i < X.Count; i++)
                g.DrawEllipse(p1, X[i] - 5, Y[i] - 5, 10, 10);
        }

        // Рисует найденный лучший путь
        private void DrawBestPath()
        {
            for (int i = 0; i < salesman.BestPath.Length - 1; i++)
                g.DrawLine(PenRed, X[salesman.BestPath[i]], Y[salesman.BestPath[i]],
                                   X[salesman.BestPath[i + 1]], Y[salesman.BestPath[i + 1]]);
        }

        #endregion

        
        static double Factorial(double x)
        {
            if (x == 0) return 1;
            else return x * Factorial(x - 1);
        }

        
    }
}
