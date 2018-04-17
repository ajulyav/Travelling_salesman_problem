using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_PictureBox_v2
{
    /// <summary>
    /// Контейнер для данных
    /// </summary>
    class Chromosome
    {
        /// <summary>
        /// Путь
        /// </summary>
        public int[] Path;

        /// <summary>
        /// Длина пути
        /// </summary>
        public double Fitness;

        /// <summary>
        /// Первая точка интервала для рулетки
        /// </summary>
        public double A;

        /// <summary>
        /// Вторая точка интервала для рулетки
        /// </summary>
        public double B;

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public Chromosome()
        {

        }

        /// <summary>
        /// Конструктор копии для объектов
        /// </summary>
        /// <param name="ch"></param>
        public Chromosome(Chromosome ch)
        {
            A = ch.A;
            B = ch.B;
            Fitness = ch.Fitness;
            Path = ch.Path;
        }

    }
}
