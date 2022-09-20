using System;
using System.Diagnostics;
using System.Linq;
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
                    newMatrix[i, j] = x.Next(1, 10000);
                }
            }
            return newMatrix;
        }
        static void findMin(int[,] matrix, int a, int b, ref int result)
        {
            int n = matrix.GetLength(1);
            for (int i = a; i < b; i++)
            {
                int min = int.MaxValue;
                for (int j = 0; j < n; j++)
                {
                    min = Math.Min(matrix[i, j], min);
                }
                result = Math.Max(result, min);
            }
        }
        static int ThreadedFindMaxMinElement(int[,] matrix, int threadNumber)
        {
            int n = matrix.GetLength(1);
            int m = matrix.GetLength(0);
            int step = m / (threadNumber - 1);
            int currentPos = 0;

            Thread[] threads = new Thread[threadNumber - 1];
            int[] mins = new int[threadNumber];
            for (int i = 0; i < threadNumber - 1; i++)
            {
                int cp = currentPos;
                threads[i] = new Thread(() => findMin(matrix, cp, cp + step, ref mins[i]));
                threads[i].Start();
                currentPos += step;
            }
            findMin(matrix, currentPos, m, ref mins[threadNumber - 1]);

            foreach (var item in threads)
            {
                item.Join();
            }

            return mins.Max();
        }
        static int LinearFindMaxMinElement(int[,] matrix)
        {
            int n = matrix.GetLength(1);
            int m = matrix.GetLength(0);
            int result = int.MinValue;
            for (int i = 0; i < m; i++)
            {
                int min = int.MaxValue;
                for (int j = 0; j < n; j++)
                {
                    min = Math.Min(matrix[i, j], min);
                }
                result = Math.Max(result, min);
            }
            return result;
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
            int threadNumber = 8;
            int n = 10000;
            int m = 10000;

            int[,] matrix = InitMatrix(n, m);
            var watch = Stopwatch.StartNew();
            int res = LinearFindMaxMinElement(matrix);
            watch.Stop();
            //Console.WriteLine(res);
            Console.WriteLine($"Linear execution time: {watch.ElapsedMilliseconds} ms");
            watch = Stopwatch.StartNew();
            res = ThreadedFindMaxMinElement(matrix, threadNumber);
            watch.Stop();
            //Console.WriteLine(res);
            Console.WriteLine($"Threaded execution time: {watch.ElapsedMilliseconds} ms");

        }
    }
}
//for n = m = 10000:
//linear execution time: 188 ms
//threaded execution time for 2 threads: 175 ms
//S = 1.07
//E = 0.54
//threaded execution time for 3 threads: 172 ms
//S = 1.09
//E = 0.36
//threaded execution time for 4 threads: 132 ms
//S = 1.42
//E = 0.36
//threaded execution time for 8 threads: 106 ms
//S = 1.77
//E = 0.22
//threaded execution time for 16 threads: 99 ms
//S = 1.90
//E = 0.12
//threaded execution time for 100 threads: 105 ms
//S = 1.79
//E = 0.02