using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.Data;

namespace ParallelCalc
{
    class Matrix
    {
        static void InitGraph(int verticesNumber, List<List<KeyValuePair<int, int>>> edgesList)
        {
            for (int i = 0; i < verticesNumber; i++)
            {
                edgesList.Add(new List<KeyValuePair<int, int>>());
            }
            Random x = new Random();
            for (int i = 0; i < verticesNumber; i++)
            {
                for (int j = i + 1; j < verticesNumber; j++)
                {
                    int value = x.Next(0, 10);
                    edgesList[i].Add(new KeyValuePair<int, int>(j, value));
                    edgesList[j].Add(new KeyValuePair<int, int>(i, value));
                }
            }
        }

        static void LinearPrim(int verticesNumber, int VertexFrom, List<List<KeyValuePair<int, int>>> edgesList)
        {
            int v = VertexFrom - 1;
            bool[] used_vertices = new bool[verticesNumber];
            int[] distances = new int[verticesNumber];
            for (int i = 0; i < verticesNumber; i++)
            {
                distances[i] = int.MaxValue;
            }
            distances[v] = 0;
            for (int i = 0; i < verticesNumber; i++)
            {
                v = -1;
                for (int j = 0; j < verticesNumber; j++)
                {
                    if (!used_vertices[j] && (v == -1 || distances[j] < distances[v]))
                    {
                        v = j;
                    }
                }
                used_vertices[v] = true;
                foreach (var item in edgesList[v])
                {
                    if (distances[item.Key] > item.Value)
                    {
                        distances[item.Key] = item.Value;
                    }
                }
            }
        }

        static void ThreadedPrim(int verticesNumber, int VertexFrom, List<List<KeyValuePair<int, int>>> edgesList, int threadNumber)
        {
            int v = VertexFrom - 1;
            bool[] used_vertices = new bool[verticesNumber];
            int[] distances = new int[verticesNumber];
            for (int i = 0; i < verticesNumber; i++)
            {
                distances[i] = int.MaxValue;
            }
            distances[v] = 0;
            for (int i = 0; i < verticesNumber; i++)
            {
                v = -1;
                for (int j = 0; j < verticesNumber; j++)
                {
                    if (!used_vertices[j] && (v == -1 || distances[j] < distances[v]))
                    {
                        v = j;
                    }
                }
                used_vertices[v] = true;
                Thread[] threads = new Thread[threadNumber - 1];
                int step = edgesList[v].Count / threadNumber;
                int currentVertex = 0;
                for (int j = 0; j < threadNumber - 1; j++)
                {
                    int cp = currentVertex;
                    threads[j] = new Thread(() => ForThread(edgesList, cp, step, used_vertices, distances, v));
                    threads[j].Start();
                    currentVertex += step;
                }
                ForThread(edgesList, currentVertex, edgesList[v].Count - currentVertex, used_vertices, distances, v);
                foreach (var item in threads)
                {
                    item.Join();
                }
            }
        }

        static void ForThread(List<List<KeyValuePair<int, int>>> edgesList, int vertex, int step, bool[] used_vertices, int[] distances, int ver)
        {
            for (int i = vertex; i < vertex + step; i++)
            {
                var v = edgesList[ver][i];
                if (distances[v.Key] > v.Value)
                {
                    distances[v.Key] = v.Value;
                }
            }
        }


        static void Main()
        {

            int verticesNumber = 10000;
            int vertexFrom = 1;
            List<List<KeyValuePair<int, int>>> edgesList = new List<List<KeyValuePair<int, int>>>();
            InitGraph(verticesNumber, edgesList);
            int threadNumber = 8;
            var watch = Stopwatch.StartNew();
            LinearPrim(verticesNumber, vertexFrom, edgesList);
            watch.Stop();
            Console.WriteLine($"Linear execution time: {watch.ElapsedMilliseconds} ms");
            watch = Stopwatch.StartNew();
            ThreadedPrim(verticesNumber, vertexFrom, edgesList, threadNumber);
            watch.Stop();
            Console.WriteLine($"Threaded execution time: {watch.ElapsedMilliseconds} ms");
        }
    }
}
// for 10000 vertices
//1 thread execution: 769 ms
//2 threads execution: 3234 ms
//3 threads execution: 5228 ms
//4 threads execution: 7824 ms
//8 threads execution: 14644 ms
