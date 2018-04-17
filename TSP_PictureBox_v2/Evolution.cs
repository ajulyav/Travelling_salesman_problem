using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_PictureBox_v2
{
    class Evolution
    {
        public List<Chromosome> CurrentPopulation = new List<Chromosome>();
        public List<Chromosome> NextPopulation = new List<Chromosome>();
        public int BestChromosomeIndex;

        double sumFintess; // Сумма значений фитнесса всех хромосом
        int towns; // Кол-во городов (из textBox1)
        double mutation_Rate; // Вероятность мутации (из Mutation_textBox)
        double[,] distance; // Матрица расстояний между городами

        Random rnd_double = new Random();

        /// <summary>
        /// Конструктор. Инициализирует новый экземпляр класса
        /// </summary>
        /// <param name="numberTowns">Кол-во городов</param>
        /// <param name="mutationRate">Вероятность мутации</param>
        /// <param name="x">Массив координат городов по X</param>
        /// <param name="y">Массив координат городов по Y</param>
        public Evolution(int numberTowns, double mutationRate, int[] x, int[] y)
        {
            towns = numberTowns;
            mutation_Rate = mutationRate;

            // Формируем матрицу расстояний
            distance = new double[x.Length, x.Length];

            for (int j = 0; j < x.Length; j++)
            {
                distance[j, j] = 0;

                for (int i = 0; i < x.Length; i++)
                {
                    double value = Math.Sqrt(Math.Pow(x[i] - x[j], 2) + Math.Pow(y[i] - y[j], 2));
                    distance[i, j] = distance[j, i] = value;
                }
            }
        }

        public void Generate(int populationSize)
        {
            Random rnd = new Random();

            for (int j = 0; j < populationSize; j++)
            {
                List<int> cities = new List<int>();
                List<int> path = new List<int>();

                // Заполняем список городов не считая нулевой город
                for (int i = 1; i < towns; i++)
                {
                    cities.Add(i);
                }
                
                // Генерим случайный путь
                int b = cities.Count;
                for (int i = 0; i < b; i++)
                {
                    int r = rnd.Next(0, cities.Count);
                    path.Add(cities[r]);
                    cities.RemoveAt(r);
                }
                
                // Добавляем в начало и конец пути начальный город - 0
                path.Insert(0, 0);
                path.Add(0);

                // Создаем новую хромосому (ch) и добавляем ее популяцию (Chromosomes)
                Chromosome ch = new Chromosome();
                ch.Path = path.ToArray(); // Добавляем сгенерированный путь в хромосому
                ch.Fitness = CalculateFitness(path.ToArray()); // Вычисляем фитнесс функцию
                CurrentPopulation.Add(ch); 
            }


        }

        // Расчет длины пути
        double CalculateFitness(int[] m)
        {
            double sum = 0;

            for (int i = 0; i < m.Length - 1; i++)
            {
                sum = sum + distance[m[i], m[i + 1]];
            }
            return sum;
        }

        // Вычисление суммы значения фитнесса всех хромосом
        void CalculateSum()
        {
            sumFintess = 0;

            for (int i = 0; i < CurrentPopulation.Count; i++)
            {
                sumFintess = sumFintess + CurrentPopulation[i].Fitness;
            }
        }

        // Вычисление интервалов для рулетки
        void MakeIntervals()
        {
            // Вычисляем сумму значений фитнесса всех хромосом
            CalculateSum();

            // У первой хромосомы интервал от 0 до его вероятности попадания
            CurrentPopulation[0].A = 0.0;
            CurrentPopulation[0].B = CurrentPopulation[0].Fitness / sumFintess;

            // У любой другой, кроме последней вычисляется следующим образом...
            for (int i = 1; i < CurrentPopulation.Count; i++)
            {
                // ...начало интервала - конец интервала предыдущей хромосомы
                CurrentPopulation[i].A = CurrentPopulation[i - 1].B;

                // ...прибавляем вероятность текущей хромосомы к конецу интервала предыдущей особи
                // и получаем конец интервала текущей хромосомы
                CurrentPopulation[i].B = CurrentPopulation[i - 1].B + (CurrentPopulation[i].Fitness / sumFintess);
            }
        }

        /// <summary>
        /// Запускаем одну итерацию генетического алгоритма
        /// </summary>
        public void StepGA()
        {
            
            // Копируем популяцию из предыдущей итерации в текущую популяцию
            // Если это не первый запуск ГА
            if (NextPopulation.Count != 0)
            {
                CurrentPopulation.Clear();
                for (int i = 0; i < NextPopulation.Count; i++)
                {
                    CurrentPopulation.Add(new Chromosome(NextPopulation[i]));
                }
                NextPopulation.Clear();
            }


            // Вычисляем интервалы для рулетки
            MakeIntervals();

            int index_first = 0, index_second = 0; // Индексы хромосом для кросинговера
            for (int j = 1; j < CurrentPopulation.Count / 2; j++)
            {

                #region Выбираем из популяции хромосомы для кросинговера методом рулетки

                double rd1 = rnd_double.NextDouble();
                double rd2 = rnd_double.NextDouble();

                // Ищем в популяции индекс первой родительской хромосомы
                for (int i = 0; i < CurrentPopulation.Count; i++)
                {
                    if (rd1 >= CurrentPopulation[i].A && rd1 < CurrentPopulation[i].B)
                    {
                        index_first = i;
                        break;
                    }
                }

                // Ищем в популяции индекс второй родительской хромосомы
                for (int i = 0; i < CurrentPopulation.Count; i++)
                {
                    if (rd2 >= CurrentPopulation[i].A && rd2 < CurrentPopulation[i].B)
                    {
                        index_second = i;
                        break;
                    }
                }

                #endregion

                Crossover(index_first, index_second);

                if (rnd_double.NextDouble() <= mutation_Rate)
                {
                    Mutation();
                }
            }

            #region Добавляем 2 элитные хромосомы из текущей популяции в следующую

            // Ищем в популяции лучшую и следующую за ней хромосомы
            int SecondBestChromosomeIndex = 0;
            BestChromosomeIndex = 0;
            for (int i = 1; i < CurrentPopulation.Count; i++)
            {
                if (CurrentPopulation[i].Fitness < CurrentPopulation[i - 1].Fitness)
                {
                    SecondBestChromosomeIndex = BestChromosomeIndex;
                    BestChromosomeIndex = i;
                }
            }

            // Добавляем в ноую популяцию 2 элитные хромосомы из текущей популяции в следующую
            NextPopulation.Add(new Chromosome(CurrentPopulation[SecondBestChromosomeIndex]));
            NextPopulation.Add(new Chromosome(CurrentPopulation[BestChromosomeIndex]));

            #endregion

            FindBestChromosome();
        }

        // Поиск в новой популяции лучшей хромосомы
        void FindBestChromosome()
        {
            for (int i = 1; i < CurrentPopulation.Count; i++)
            {
                if (NextPopulation[i].Fitness < NextPopulation[i - 1].Fitness)
                {
                    BestChromosomeIndex = i;
                }
            }
        }

        void Crossover(int index_first, int index_second)
        {
            int[] child1 = new int[CurrentPopulation[0].Path.Length];
            int[] child2 = new int[CurrentPopulation[0].Path.Length];

            int[] chromosome1 = new int[CurrentPopulation[0].Path.Length];
            int[] chromosome2 = new int[CurrentPopulation[0].Path.Length];

            Array.Copy(CurrentPopulation[index_first].Path, chromosome1, chromosome1.Length);
            Array.Copy(CurrentPopulation[index_second].Path, chromosome2, chromosome2.Length);

            int a = 2; // Точка разрыва

            // Рандомная точка разрыва
            //Random r = new Random();
            //a = r.Next(2, CurrentPopulation[0].Path.Length - 3);


            // Меняем местами части хросом
            Array.Copy(chromosome1, child1, a);
            Array.Copy(chromosome1, a, child2, a, chromosome1.Length - a);

            // Список всех возможных городов
            List<int> mas = new List<int>(CurrentPopulation[0].Path.Length - 2);
            
            #region Работаем с первой новой хромосомой

            // Создаем список всех возможных городов
            for (int i = 1; i <= CurrentPopulation[0].Path.Length - 2; i++)
            {
                mas.Add(i);
            }

            // Удаляем из писока всех возможных городов уже имеющиеся в новой хромосоме
            for (int i = 1; i < a; i++)
            {
                mas.Remove(child1[i]);
            }

            // Заполняем вторую половину новой хромосомы child1 генами из родительской chromosome2
            int num = 0;
            for (int i = a; i < child1.Length - 1; i++)
            {
                num = Array.IndexOf(child1, chromosome2[i]);

                if (num < 0)
                {
                    child1[i] = chromosome2[i];
                    mas.Remove(chromosome2[i]);
                }
                else
                {
                    child1[i] = mas[0];
                    mas.RemoveAt(0);
                }
            }

            #endregion


            #region Работаем со второй новой хромосомой

            // Создаем список всех возможных городов
            for (int i = 1; i <= CurrentPopulation[0].Path.Length - 2; i++)
            {
                mas.Add(i);
            }

            // Удаляем из писока всех возможных городов уже имеющиеся в новой хромосоме
            for (int i = a; i < child2.Length - 1; i++)
            {
                mas.Remove(child2[i]);
            }

            // Заполняем вторую половину новой хромосомы child2 генами из родительской chromosome2
            int num1 = 0;
            for (int i = 1; i < a; i++)
            {
                num1 = Array.IndexOf(child2, chromosome2[i]);

                if (num1 < 0)
                {
                    child2[i] = chromosome2[i];
                    mas.Remove(chromosome2[i]);
                }
                else
                {
                    child2[i] = mas[0];
                    mas.RemoveAt(0);
                }
            }
            #endregion

            // Добавляем новые хромосы в следующую популяцию
            Chromosome c1 = new Chromosome();
            Chromosome c2 = new Chromosome();

            // !!!!!!!!!!!!!!! ОБЪЯСНИТЬ как это работает!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            c1.Path = (int[])child1.Clone();
            c2.Path = (int[])child2.Clone();

            // Вычисляем фитнесс функуию
            c1.Fitness = CalculateFitness(child1);
            c2.Fitness = CalculateFitness(child2);

            // Добавляем в новую популяцию детей
            //NextPopulation.Add(c1);
            //NextPopulation.Add(c2);

            // Добавляем в следующее поколение лушщих родителей, а не детей
            if (c1.Fitness < CurrentPopulation[index_first].Fitness) NextPopulation.Add(c1);
            else NextPopulation.Add(new Chromosome(CurrentPopulation[index_first]));

            if (c2.Fitness < CurrentPopulation[index_second].Fitness) NextPopulation.Add(c2);
            else NextPopulation.Add(new Chromosome(CurrentPopulation[index_second]));

        }

        void Mutation()
        {
            Random index = new Random();

            // Генерим индекс хромосомы для мутации
            int ch_index = index.Next(0, NextPopulation.Count - 1);

            // Генерим индексы переставляемых городов (исключая первый и последний город)
            int firstindex = index.Next(1, NextPopulation[0].Path.Length - 2);
            int secondindex = index.Next(1, NextPopulation[0].Path.Length - 2);

            // Меняем местами два случайно выбраные города
            int tmp = NextPopulation[ch_index].Path[firstindex];
            NextPopulation[ch_index].Path[firstindex] = NextPopulation[ch_index].Path[secondindex];
            NextPopulation[ch_index].Path[secondindex] = tmp;

            // Вычисляем фитнесс функуию мутированной хромосомы
            NextPopulation[ch_index].Fitness = CalculateFitness(NextPopulation[ch_index].Path);
        }

    }
}
