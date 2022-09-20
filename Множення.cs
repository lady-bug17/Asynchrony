using System;
using System.Diagnostics;
using System.Threading;

namespace ParallelCalc
{
    class Love
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

        static int FindElement(int[,] m1, int[,] m2, int r, int c)
        {
            int result = 0;
            for (int i = 0; i < m1.GetLength(1); i++)
            {
                result += m1[r, i] * m2[i, c];
            }
            return result;
        }

        static void MultiplyMatrixes(int[,] m1, int[,] m2, int[,] result)
        {
            for (int i = 0; i < m1.GetLength(0); i++)
            {
                for (int j = 0; j < m2.GetLength(1); j++)
                {
                    result[i, j] = FindElement(m1, m2, i, j);
                }
            }
        }

        static void ThreadedMult(int[,] m1, int[,] m2, int[,] result, int threadNumber)
        {
            int currentPos = 0;
            int step = m1.GetLength(0) / threadNumber;
            Thread[] threads = new Thread[threadNumber - 1];
            for (int i = 0; i < threadNumber - 1; i++)
            {
                threads[i] = new Thread(() => MultiplyPart(m1, m2, result, currentPos, currentPos + step));
                threads[i].Start();
                currentPos += step;
            }
            MultiplyPart(m1, m2, result, currentPos, m1.GetLength(0));
            foreach (var item in threads)
            {
                item.Join();
            }
        }

        static void MultiplyPart(int[,] m1, int[,] m2, int[,] result, int currentPos, int end)
        {
            for (int i = currentPos; i < end; i++)
            {
                for (int j = 0; j < m2.GetLength(1); j++)
                {
                    result[i, j] = FindElement(m1, m2, i, j);
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
            int threadNumber = 100;
            int n = 1000;
            int m = 1000;
            int[,] m1 = InitMatrix(n, m);
            int[,] m2 = InitMatrix(m, n);
            int[,] m3 = new int[n, n];


            var watch = Stopwatch.StartNew();
            MultiplyMatrixes(m1, m2, m3);
            watch.Stop();
            Console.WriteLine($"Linear execution time: {watch.ElapsedMilliseconds} ms");
            watch = Stopwatch.StartNew();
            ThreadedMult(m1, m2, m3, threadNumber);
            watch.Stop();
            Console.WriteLine($"Threaded execution time: {watch.ElapsedMilliseconds} ms");
        }
    }
}

// for matrixes 1000x1000
//1 thread execution: 10431 ms
//2 threads execution: 6617 ms
//4 threads execution: 5060 ms
//8 threads execution: 4454 ms
//100 threads execution: 4615 ms
