using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

namespace ParallelCalc
{
    class Matrix
    {
        static int[,] InitAdjacencyMatrix(int verticesNumber)
        {
            Random x = new Random();
            int[,] newMatrix = new int[verticesNumber, verticesNumber];
            for (int i = 0; i < verticesNumber; i++)
            {
                for (int j = 0; j < verticesNumber; j++)
                {
                    newMatrix[i, j] = x.Next(0, 10);
                }
            }
            for (int i = 0; i < verticesNumber; i++)
            {
                newMatrix[i, i] = 0;
            }
            return newMatrix;
        }

        static int[,] InitPathMatrix(int verticesNumber)
        {
            Random x = new Random();
            int[,] newMatrix = new int[verticesNumber, verticesNumber];
            for (int i = 0; i < verticesNumber; i++)
            {
                for (int j = 0; j < verticesNumber; j++)
                {
                    newMatrix[i, j] = i;
                }
            }
            for (int i = 0; i < verticesNumber; i++)
            {
                newMatrix[i, i] = -1;
            }
            return newMatrix;
        }

        static void Linear_Floyd_Warshall(int[,] adjacencyMatrix, int[,] pathMatrix, List<List<int>> pathes)
        {
            for (int k = 0; k < adjacencyMatrix.GetLength(0); k++)
            {
                for (int i = 0; i < adjacencyMatrix.GetLength(0); i++)
                {
                    for (int j = 0; j < adjacencyMatrix.GetLength(0); j++)
                    {
                        if (adjacencyMatrix[i, k] + adjacencyMatrix[k, j] < adjacencyMatrix[i, j])
                        {
                            adjacencyMatrix[i, j] = adjacencyMatrix[i, k] + adjacencyMatrix[k, j];
                            pathMatrix[i, j] = pathMatrix[k, j];
                        }
                    }
                }
            }
            for (int i = 0; i < adjacencyMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < adjacencyMatrix.GetLength(0); j++)
                {
                    if (i != j)
                    {
                        FindWay(pathMatrix, i, j, pathes);
                    }
                }
            }
        }

        static void ForThread(int[,] adjacencyMatrix, int k, int a, int b, int[,] pathMatrix)
        {
            for (int i = a; i < b; i++)
            {
                for (int j = 0; j < adjacencyMatrix.GetLength(0); j++)
                {
                    if (adjacencyMatrix[i, k] + adjacencyMatrix[k, j] < adjacencyMatrix[i, j])
                    {
                        adjacencyMatrix[i, j] = adjacencyMatrix[i, k] + adjacencyMatrix[k, j];
                        pathMatrix[i, j] = pathMatrix[k, j];
                    }
                }
            }
        }

        static void Threaded_Floyd_Warshall(int[,] adjacencyMatrix, int[,] pathMatrix, int threadNumber, List<List<int>> pathes)
        {
            for (int k = 0; k < adjacencyMatrix.GetLength(0); k++)
            {
                Thread[] threads = new Thread[threadNumber - 1];
                int step = adjacencyMatrix.GetLength(0) / threadNumber;
                int currentPos = 0;

                for (int i = 0; i < threadNumber - 1; i++)
                {
                    int cp = currentPos;
                    threads[i] = new Thread(() => ForThread(adjacencyMatrix, k, cp, cp + step, pathMatrix));
                    threads[i].Start();
                    currentPos += step;
                }
                ForThread(adjacencyMatrix, k, currentPos, adjacencyMatrix.GetLength(0), pathMatrix);
                foreach (var item in threads)
                {
                    item.Join();
                }
            }

            for (int i = 0; i < adjacencyMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < adjacencyMatrix.GetLength(0); j++)
                {
                    if (i != j)
                    {
                        FindWay(pathMatrix, i, j, pathes);
                    }
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

        static void FindWay(int[,] pthm, int from, int to, List<List<int>> pathes)
        {
            List<int> path = new List<int>();
            path.Add(to);
            while (pthm[from, to] != from)
            {
                path.Add(pthm[from, to]);
                to = pthm[from, to];
            }
            path.Add(from);
            path.Reverse();
            pathes.Add(path);
        }

        static void Main()
        {
            int threadNumber = 4;
            int n = 1000;
            List<List<int>> pathes = new List<List<int>>();
            int[,] adjm = InitAdjacencyMatrix(n);
            int[,] pthm = InitPathMatrix(n);
            //foreach (var path in pathes)
            //{
            //    foreach (var element in path)
            //    {
            //        Console.WriteLine(element);
            //    }
            //    Console.WriteLine();
            //}
            var watch = Stopwatch.StartNew();
            Linear_Floyd_Warshall(adjm, pthm, pathes);
            watch.Stop();
            Console.WriteLine($"Linear execution time: {watch.ElapsedMilliseconds} ms");
            watch = Stopwatch.StartNew();
            Threaded_Floyd_Warshall(adjm, pthm, threadNumber, pathes);
            watch.Stop();
            Console.WriteLine($"Threaded execution time: {watch.ElapsedMilliseconds} ms");
        }
    }
}
//// for matrixes 1000x1000
////1 thread execution: 15817 ms
////2 threads execution: 10011 ms
////3 threads execution: 7613 ms
////4 threads execution: 7613 ms
////8 threads execution: 6716 ms
////100 threads execution: 24768 ms