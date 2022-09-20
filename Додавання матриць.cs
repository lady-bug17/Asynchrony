using System;
using System.Diagnostics;
using System.Threading;

namespace ParallelCalc
{
    class Matrix
    {
        static int[,] InitMatrix(int a, int b)
        {
            Random x = new Random();
            int[,] newMatrix = new int[a, b];
            for (int i = 0; i < a; i++)
            {
                for (int j = 0; j < b; j++)
                {
                    newMatrix[i, j] = x.Next(1, 10);
                }
            }
            return newMatrix;
        }

        static int[,] AddMatrix(int[,] m1, int[,] m2)
        {
            int a = m1.GetLength(0);
            int b = m1.GetLength(1);
            int[,] result = new int[a, b];
            for (int i = 0; i < a; i++)
            {
                for (int j = 0; j < b; j++)
                {
                    result[i, j] = m1[i, j] + m2[i, j];
                }
            }
            return result;
        }

        static int[,] SubstrMatrix(int[,] m1, int[,] m2)
        {
            int a = m1.GetLength(0);
            int b = m1.GetLength(1);
            int[,] result = new int[a, b];
            for (int i = 0; i < a; i++)
            {
                for (int j = 0; j < b; j++)
                {
                    result[i, j] = m1[i, j] + m2[i, j];
                }
            }
            return result;
        }

        static int[,] Threaded(int threadsAmount, int[,] m1, int[,] m2)
        {
            Thread[] threads = new Thread[threadsAmount - 1];
            int step = m1.GetLength(0) / threadsAmount;
            int a = 0;
            int[,] result = new int[m1.GetLength(0), m1.GetLength(1)];
            for (int i = 0; i < threads.Length; i++)
            {
                int b = a;
                threads[i] = new Thread(() => AddPart(m1, m2, b, b + step, result));
                //threads[i] = new Thread(() => { SubstrPart(m1, m2, a, a + step, result); });
                threads[i].Start();
                a += step;
            }
            AddPart(m1, m2, a, m1.GetLength(0), result);
            //SubstrPart(m1, m2, a, m1.GetLength(0), result);
            foreach (var item in threads)
            {
                item.Join();
            }
            return result;
        }

        static void AddPart(int[,] m1, int[,] m2, int a, int step, int[,] m3)
        {
            for (int i = a; i < step; i++)
            {
                for (int j = 0; j < m1.GetLength(1); j++)
                {
                    m3[i, j] = m1[i, j] + m2[i, j];
                }
            }
        }

        static void SubstrPart(int[,] m1, int[,] m2, int a, int step, int[,] m3)
        {
            for (int i = a; i < a + step; i++)
            {
                for (int j = 0; j < m1.GetLength(1); j++)
                {
                    m3[i, j] = m1[i, j] - m2[i, j];
                }
            }
        }

        static void PrintMatrix(int[,] m)
        {
            for (int i = 0; i < m.GetLength(0); i++)
            {
                for (int j = 0; j < m.GetLength(1); j++)
                {
                    System.Console.Write($"{m[i, j]} ");
                }
                System.Console.WriteLine();
            }
        }


        static void Main()
        {
            int threadAmount = 4;
            int n = 10000;
            int m = 10000;
            int[,] m1 = InitMatrix(n, m);
            int[,] m2 = InitMatrix(n, m);
            int[,] m3 = new int[n, m];

            var watch = Stopwatch.StartNew();
            m3 = AddMatrix(m1, m2);
            //m3 = SubstrMatrix(m1, m2);
            watch.Stop();
            Console.WriteLine($"Linear execution time: {watch.ElapsedMilliseconds} ms");
            watch = Stopwatch.StartNew();
            m3 = Threaded(threadAmount, m1, m2);
            watch.Stop();
            Console.WriteLine($"Threaded execution time: {watch.ElapsedMilliseconds} ms");
        }
    }
}