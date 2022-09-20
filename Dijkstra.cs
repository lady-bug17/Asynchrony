using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Globalization;

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

        static void Linear_Dijkstra(int verticesNumber, int VertexFrom, List<List<KeyValuePair<int, int>>> edgesList)
        {
            List<bool> marked = new List<bool>(verticesNumber);
            List<int> distances = new List<int>(verticesNumber);
            for (int i = 0; i < verticesNumber; i++)
            {
                marked.Add(false);
                distances.Add(int.MaxValue);
            }
            distances[VertexFrom] = 0;
            for (int i = 0; i < verticesNumber; i++)
            {
                int vertex = -1;
                for (int j = 0; j < verticesNumber; j++)
                {
                    if (!marked[j] && (vertex == -1 || distances[j] < distances[vertex]))
                    {
                        vertex = j;
                    }
                }
                marked[vertex] = true;
                foreach (var v in edgesList[vertex])
                {
                    if (!marked[v.Key] && (distances[v.Key] > (distances[vertex] + v.Value)))
                    {
                        distances[v.Key] = distances[vertex] + v.Value;
                    }
                }
            }
        }

        static void Threaded_Dijkstra(int verticesNumber, int VertexFrom, List<List<KeyValuePair<int, int>>> edgesList, int threadNumber)
        {
            List<bool> marked = new List<bool>(verticesNumber);
            List<int> distances = new List<int>(verticesNumber);
            for (int i = 0; i < verticesNumber; i++)
            {
                marked.Add(false);
                distances.Add(int.MaxValue);
            }
            distances[VertexFrom] = 0;
            for (int i = 0; i < verticesNumber; i++)
            {
                int vertex = -1;
                for (int j = 0; j < verticesNumber; j++)
                {
                    if (!marked[j] && (vertex == -1 || distances[j] < distances[vertex]))
                    {
                        vertex = j;
                    }
                }
                marked[vertex] = true;
                Thread[] threads = new Thread[threadNumber - 1];
                int step = edgesList[vertex].Count / threadNumber;
                int currentVertex = 0;
                for (int j = 0; j < threadNumber - 1; j++)
                {
                    int cp = currentVertex;
                    threads[j] = new Thread(() => ForThread(edgesList, cp, step, marked, distances));
                    threads[j].Start();
                    currentVertex += step;
                }
                ForThread(edgesList, currentVertex, edgesList[vertex].Count - currentVertex, marked, distances);
            }
        }

        static void ForThread(List<List<KeyValuePair<int, int>>> edgesList, int vertex, int step, List<bool> marked, List<int> distances)
        {
            for (int i = vertex; i < vertex + step; i++)
            {
                var v = edgesList[vertex][i];
                if (!marked[v.Key] && (distances[v.Key] > (distances[vertex] + v.Value)))
                {
                    distances[v.Key] = distances[vertex] + v.Value;
                }
            }
        }


        static void Main()
        {

            int verticesNumber = 10000;
            int vertexFrom = 0;
            List<List<KeyValuePair<int, int>>> edgesList = new List<List<KeyValuePair<int, int>>>();
            InitGraph(verticesNumber, edgesList);
            int threadNumber = 2;
            var watch = Stopwatch.StartNew();
            Linear_Dijkstra(verticesNumber, vertexFrom, edgesList);
            watch.Stop();
            Console.WriteLine($"Linear execution time: {watch.ElapsedMilliseconds} ms");
            watch = Stopwatch.StartNew();
            Threaded_Dijkstra(verticesNumber, vertexFrom, edgesList, threadNumber);
            watch.Stop();
            Console.WriteLine($"Threaded execution time: {watch.ElapsedMilliseconds} ms");
        }
    }
}
//// for 10000 vertices
////1 thread execution: 877 ms
////2 threads execution: 2654 ms
////3 threads execution: 4552 ms
////4 threads execution: 6249 ms
////8 threads execution: 13835 ms