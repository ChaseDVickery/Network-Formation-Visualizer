using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Graph<T> where T : class
{
    public List<T> nodes;
    public float[,] adjMatrix;

    public Graph(List<T> startNodes) {
        nodes = startNodes;
        BuildEmptyGraph();
    }

    public void BuildEmptyGraph() {
        adjMatrix = new float[nodes.Count, nodes.Count];
        for (int i = 0; i < nodes.Count; i++) {
            for (int j = 0; j < nodes.Count; j++) {
                adjMatrix[i,j] = 0;
            }
        }
    }

    public bool AreConnected(int idx1, int idx2) {
        if (idx1 < adjMatrix.GetLength(0) && idx2 < adjMatrix.GetLength(0)) {
            return adjMatrix[idx1, idx2] > 0;
        }
        return false;
    }
    public bool AreConnected(T n1, T n2) {
        int idx1 = nodes.FindIndex(x => x==n1);
        int idx2 = nodes.FindIndex(x => x==n2);
        if (idx1 >= 0 && idx2 >= 0) {
            return AreConnected(idx1, idx2);
        }
        return false;
    }

    public void AddNode(T node) {
        if (!nodes.Contains(node)) {
            nodes.Add(node);
            float[,] tempAdj = new float[nodes.Count, nodes.Count];
            for (int i = 0; i < adjMatrix.GetLength(0); i++) {
                for (int j = 0; j < adjMatrix.GetLength(1); j++) {
                    tempAdj[i,j] = adjMatrix[i,j];
                }
            }
            // Fill in zeros for last row and column for new node with no connections
            for (int i = 0; i < adjMatrix.GetLength(0); i++) { tempAdj[nodes.Count-1, i] = 0; }
            for (int i = 0; i < adjMatrix.GetLength(0); i++) { tempAdj[i, nodes.Count-1] = 0; }
            // Transfer temp matrix;
            adjMatrix = tempAdj;
        } else {
            Debug.Log(System.String.Format("Graph already contains node {0}", node));
        }
    }

    public void AddConnection(int idx1, int idx2, int weight=1, bool undirected=true) {
        if (idx1 < adjMatrix.GetLength(0) && idx2 < adjMatrix.GetLength(0)) {
            adjMatrix[idx1, idx2] = weight;
            if (undirected) { adjMatrix[idx2, idx1] = weight; }
        }
    }

    public void AddConnection(T n1, T n2, int weight=1, bool undirected=true) {
        int idx1 = nodes.FindIndex(x => x==n1);
        int idx2 = nodes.FindIndex(x => x==n2);
        if (idx1 >= 0 && idx2 >= 0) {
            AddConnection(idx1, idx2, weight, undirected);
        }
    }

    public void RemoveNode(int idx) {
        nodes.RemoveAt(idx);
        float[,] tempAdj = new float[nodes.Count, nodes.Count];
        // Influence from https://stackoverflow.com/questions/26303030/how-can-i-delete-rows-and-columns-from-2d-array-in-c
        for (int i = 0, u = 0; i < adjMatrix.GetLength(0); i++) {
            if (i==idx) {continue;}
            for (int j = 0, v = 0; j < adjMatrix.GetLength(1); j++) {
                if (j==idx) {continue;}
                tempAdj[u,v] = adjMatrix[i,j];
                v++;
            }
            u++;
        }
        // Transfer temp matrix;
        adjMatrix = tempAdj;
    }
    public void RemoveNode(T n1) {
        int idx1 = nodes.FindIndex(x => x==n1);
        if (idx1 >= 0) {
            RemoveNode(idx1);
        }
    }

    public override string ToString() {
        int spacing = 20;
        string s = "".PadRight(spacing);
        foreach (T node in nodes) {
            s += node.ToString().PadRight(spacing);
        }
        s += "\n";
        for (int i = 0; i < adjMatrix.GetLength(0); i++) {
            s += nodes[i].ToString().PadRight(spacing);
            for (int j = 0; j < adjMatrix.GetLength(1); j++) {
                s += adjMatrix[i,j].ToString().PadRight(spacing);
            }
            s += "\n";
        }
        return s;
    }
}
