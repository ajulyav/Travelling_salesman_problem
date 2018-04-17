using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_PictureBox_v2
{
    class TSPpath
    {
        // Расстояния между городами
        double[,] distance;

        // Массив из которого нжно получить все возможные варианты перестановок
        int[] A;

        // Путь с фиксированным начальным и конечным городом = 0
        int[] Path;

        /// <summary>
        /// Оптимальный путь
        /// </summary>
        public int[] BestPath;

        /// <summary>
        /// Длина минимального пути
        /// </summary>
        public double Length;


        /// <summary>
        /// Инициализирует новый экземпляр класса
        /// </summary>
        /// <param name="x">Массив координат городов по X</param>
        /// <param name="y">Массив координат городов по Y</param>
        public TSPpath(int[] x, int[] y)
        {
            //на вход передаем уже созданные города
            distance = new double[x.Length, x.Length];

            //формируем матрицу расстояний, работать в дальнейшем будем именно с ней
            for (int j = 0; j < x.Length; j++)
            {
                distance[j, j] = 0;

                for (int i = 0; i < x.Length; i++)
                {
                    double value = Math.Sqrt(Math.Pow(x[i] - x[j], 2) + Math.Pow(y[i] - y[j], 2));
                    distance[i, j] = distance[j, i] = value;
                }
            }

            A = new int[x.Length - 1];
            for (int i = 0; i < A.Length; i++) A[i] = i + 1;

            Path = new int[x.Length + 1];
            BestPath = new int[x.Length + 1];

        }

        /// <summary>
        /// Поиск кратчайшего пути полным перебором
        /// </summary>
        public void FindBestPath()
        {
            Length = int.MaxValue;

            Perestanovki(ref A, A.Length);
        }

        void Swap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }

        // Функция расчета длины пути
        void LengthPath(int[] m) 
        {
            double tmp = 0;

            for (int i = 0; i < m.Length - 1; i++)
            {
                tmp = tmp + distance[m[i], m[i + 1]];
            }

            if (tmp < Length)
            {
                Length = tmp;
                Array.Copy(m, BestPath, m.Length);
            }
        }

        void FindOnePath(int[] m)
        {
            int one = Array.IndexOf(m, 1);
            int two = Array.IndexOf(m, 2);

            if (one < two)
            {
                Array.Copy(A, 0, Path, 1, A.Length);
                LengthPath(Path);
            }
        }

        void Perestanovki(ref int[] M, int n) //M - массив, n - число переставляемых элементов
        {
            if (n == 1)  //если нечего переставлять
            {
                FindOnePath(M);
            }
            else
            {
                for (int i = 0; i < n; i++)
                {
                    Swap(ref M[i], ref M[n - 1]);  //меняем последний элемент с каждым, в том числе и с самим собой.
                    Perestanovki(ref M, n - 1);    //запускаем функцию, для n-1 элементов
                    Swap(ref M[i], ref M[n - 1]);  //Возвращаем массив в прежнее
                                                   //состояние для следующего обмена элементов
                }
            }
        }
    }
}
