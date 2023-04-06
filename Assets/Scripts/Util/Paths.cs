// ********************************************************
// Taken and slightly modified from:
// https://www.csharpstar.com/dijkstra-algorithm-csharp/
// ********************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

class Dijkstra
{
    private static int MinimumDistance(float[] distance, bool[] shortestPathTreeSet, int verticesCount)
    {
        float min = float.MaxValue;
        int minIndex = 0;

        for (int v = 0; v < verticesCount; ++v)
        {
            if (shortestPathTreeSet[v] == false && distance[v] <= min)
            {
                min = distance[v];
                minIndex = v;
            }
        }

        return minIndex;
    }

    private static int MinimumDistance(int[] distance, bool[] shortestPathTreeSet, int verticesCount)
    {
        int min = int.MaxValue;
        int minIndex = 0;

        for (int v = 0; v < verticesCount; ++v)
        {
            if (shortestPathTreeSet[v] == false && distance[v] <= min)
            {
                min = distance[v];
                minIndex = v;
            }
        }

        return minIndex;
    }

    private static void Print(float[] distance, int verticesCount)
    {
        Console.WriteLine("Vertex    Distance from source");

        for (int i = 0; i < verticesCount; ++i)
            Console.WriteLine("{0}\t  {1}", i, distance[i]);
    }

    private static void Print(int[] distance, int verticesCount)
    {
        Console.WriteLine("Vertex    Distance from source");

        for (int i = 0; i < verticesCount; ++i)
            Console.WriteLine("{0}\t  {1}", i, distance[i]);
    }

    public static float[] DijkstraAlgo(float[,] graph, int source, int verticesCount)
    {
        float[] distance = new float[verticesCount];
        bool[] shortestPathTreeSet = new bool[verticesCount];

        for (int i = 0; i < verticesCount; ++i)
        {
            distance[i] = float.MaxValue;
            shortestPathTreeSet[i] = false;
        }

        distance[source] = 0;

        for (int count = 0; count < verticesCount - 1; ++count)
        {
            int u = MinimumDistance(distance, shortestPathTreeSet, verticesCount);
            shortestPathTreeSet[u] = true;

            for (int v = 0; v < verticesCount; ++v)
                if (!shortestPathTreeSet[v] && Convert.ToBoolean(graph[u, v]) && distance[u] != float.MaxValue && distance[u] + graph[u, v] < distance[v])
                    distance[v] = distance[u] + graph[u, v];
        }

        // Print(distance, verticesCount);

        return distance;
    }

    public static int[] DijkstraAlgo(int[,] graph, int source, int verticesCount)
    {
        int[] distance = new int[verticesCount];
        bool[] shortestPathTreeSet = new bool[verticesCount];

        for (int i = 0; i < verticesCount; ++i)
        {
            distance[i] = int.MaxValue;
            shortestPathTreeSet[i] = false;
        }

        distance[source] = 0;

        for (int count = 0; count < verticesCount - 1; ++count)
        {
            int u = MinimumDistance(distance, shortestPathTreeSet, verticesCount);
            shortestPathTreeSet[u] = true;

            for (int v = 0; v < verticesCount; ++v)
                if (!shortestPathTreeSet[v] && Convert.ToBoolean(graph[u, v]) && distance[u] != int.MaxValue && distance[u] + graph[u, v] < distance[v])
                    distance[v] = distance[u] + graph[u, v];
        }

        // Print(distance, verticesCount);

        return distance;
    }

    public static float[] DijkstraAlgo(Graph<Agent> graph, int source) {
        return DijkstraAlgo(graph.adjMatrix, source, graph.nodes.Count);
    }

    public static int[] DijkstraAlgoHops(Graph<Agent> graph, int source) {
        return DijkstraAlgo(graph.AsGeodesic(), source, graph.nodes.Count);
    }
}