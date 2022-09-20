using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ParallelCalc
{
    class Matrix
    {
        static double f(double x)
        {
            return Math.Sin(x);
        }
        static double FindRectangleArea(double LeftBound, double step)
        {
            double area = f(LeftBound + step / 2) * step;
            return area;
        }
        static double LinearCalculateIntegral(double a, double b, int n)
        {
            double step = (b - a) / n;
            double result = 0;
            CalculateArea(a, b, step, ref result);
            return result;
        }
        static void CalculateArea(double a, double b, double step, ref double result)
        {
            //Console.WriteLine($"{a}, {b}");
            for (double i = a; i < b; i += step)
            {
                result += FindRectangleArea(i, step);
            }
        }
        static double ThreadedCalculateIntegral(double a, double b, int n, int threadNumber)
        {
            Thread[] threads = new Thread[threadNumber - 1];
            double[] results = new double[threadNumber];
            double step = (b - a) / n;
            double currentPos = a;
            for (int i = 0; i < threadNumber - 1; i++)
            {
                double cp = currentPos;
                int thread = i;
                threads[i] = new Thread(() => CalculateArea(cp, cp + step * Convert.ToInt32(n / threadNumber), step, ref results[thread]));
                threads[i].Start();
                currentPos += step * Convert.ToInt32(n / threadNumber);
            }
            if (currentPos < b)
            {
                CalculateArea(currentPos, b, step, ref results[threadNumber - 1]);
            }
            foreach (var item in threads)
            {
                item.Join();
            }
            return results.Sum();
        }

        static void Main()
        {
            double a = 0;
            double b = Math.Acos(-1);
            int n = 10000000;
            int threadNumber = 2;

            var watch = Stopwatch.StartNew();
            double result = LinearCalculateIntegral(a, b, n);
            watch.Stop();
            Console.WriteLine($"Linear execution time: {watch.ElapsedMilliseconds} ms");
            Console.WriteLine(result);
            watch = Stopwatch.StartNew();
            result = ThreadedCalculateIntegral(a, b, n, threadNumber);
            watch.Stop();
            Console.WriteLine($"Threaded execution time: {watch.ElapsedMilliseconds} ms");
            Console.WriteLine(result);
        }
    }
}
//for n = 10000000:
//linear execution time: 177 ms
//threaded execution time for 2 threads: 133 ms
//S = 1.33
//E = 0.67
//threaded execution time for 3 threads: 103 ms
//S = 1.72
//E = 0.57
//threaded execution time for 4 threads: 87 ms
//S = 2.03
//E = 0.51
//threaded execution time for 8 threads: 84 ms
//S = 2.11
//E = 0.26
//threaded execution time for 16 threads: 89 ms
//S = 1.99
//E = 0.12
//threaded execution time for 100 threads: 105 ms
//S = 1.69
//E = 0.02