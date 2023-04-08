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

    public int[,] AsGeodesic() {
        int[,] hopMatrix = new int[nodes.Count, nodes.Count];
        for (int i = 0; i < nodes.Count; i++) {
            for (int j = 0; j < nodes.Count; j++) {
                hopMatrix[i,j] = adjMatrix[i,j] > 0 ? 1 : 0;
            }
        }
        return hopMatrix;
    }

    public int GetIdx(T toFind) {
        return nodes.FindIndex(x => x==toFind);
    }

    public T GetT(int idx) {
        if (idx < 0 || idx >= nodes.Count) { return null; }
        else { return nodes[idx]; }
    }

    public void CopyFrom(Graph<T> other) {
        // Copy nodes
        nodes = new List<T>();
        foreach (T node in other.nodes) {
            nodes.Add(node);
        }
        // Copy weights
        adjMatrix = new float[nodes.Count, nodes.Count];
        for (int i = 0; i < other.adjMatrix.GetLength(0); i++) {
            for (int j = 0; j < other.adjMatrix.GetLength(1); j++) {
                adjMatrix[i,j] = other.adjMatrix[i,j];
            }
        }
    }

    public void BuildEmptyGraph() {
        adjMatrix = new float[nodes.Count, nodes.Count];
        for (int i = 0; i < nodes.Count; i++) {
            for (int j = 0; j < nodes.Count; j++) {
                adjMatrix[i,j] = 0;
            }
        }
    }

    public bool UpdateWeight(T n1, T n2, float weight) {
        if (weight <= 0) { Debug.LogError("Cannot update an edge weight to be <= 0."); return false; }
        int idx1 = nodes.FindIndex(x => x==n1);
        int idx2 = nodes.FindIndex(x => x==n2);
        if (idx1 >= 0 && idx2 >= 0) {
            return UpdateWeight(idx1, idx2, weight);
        }
        return false;
    }
    public bool UpdateWeight(int idx1, int idx2, float weight) {
        if (weight <= 0) { Debug.LogError("Cannot update an edge weight to be <= 0."); return false; }
        if (idx1 < adjMatrix.GetLength(0) && idx2 < adjMatrix.GetLength(0)) {
            if (adjMatrix[idx1, idx2] > 0) {
                adjMatrix[idx1, idx2] = weight;
            } else {
                Debug.LogError("Cannot update an edge weight if current weight is 0 (represents no connection).");
                return false;
            }
        }
        return false;
    }

    public bool AreConnected(int idx1, int idx2, bool undirected=true) {
        if (idx1 < adjMatrix.GetLength(0) && idx2 < adjMatrix.GetLength(0)) {
            bool connected = undirected ? (adjMatrix[idx1, idx2] > 0 || adjMatrix[idx2, idx1] > 0) : adjMatrix[idx1, idx2] > 0;
            return connected;
        }
        return false;
    }
    public bool AreConnected(T n1, T n2, bool undirected=true) {
        int idx1 = nodes.FindIndex(x => x==n1);
        int idx2 = nodes.FindIndex(x => x==n2);
        if (idx1 >= 0 && idx2 >= 0) {
            return AreConnected(idx1, idx2, undirected);
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

    public void AddEdge(int idx1, int idx2, float weight=1, bool undirected=true) {
        if (idx1 < adjMatrix.GetLength(0) && idx2 < adjMatrix.GetLength(0)) {
            adjMatrix[idx1, idx2] = weight;
            if (undirected) { adjMatrix[idx2, idx1] = weight; }
        }
    }

    public void AddEdge(T n1, T n2, float weight=1, bool undirected=true) {
        int idx1 = nodes.FindIndex(x => x==n1);
        int idx2 = nodes.FindIndex(x => x==n2);
        if (idx1 >= 0 && idx2 >= 0) {
            AddEdge(idx1, idx2, weight, undirected);
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

    public void RemoveEdge(int idx1, int idx2, bool undirected=true) {
        if (idx1 < adjMatrix.GetLength(0) && idx2 < adjMatrix.GetLength(0)) {
            adjMatrix[idx1, idx2] = 0;
            if (undirected) { adjMatrix[idx2, idx1] = 0; }
        }
    }
    public void RemoveEdge(T n1, T n2, bool undirected=true) {
        int idx1 = nodes.FindIndex(x => x==n1);
        int idx2 = nodes.FindIndex(x => x==n2);
        if (idx1 >= 0 && idx2 >= 0) {
            RemoveEdge(idx1, idx2, undirected);
        }
    }

    public float TotalCost() {
        float cost = 0;
        for (int i = 0; i < adjMatrix.GetLength(0); i++) {
            for (int j = 0; j < adjMatrix.GetLength(1); j++) {
                if (adjMatrix[i, j] > 0) {
                    cost += adjMatrix[i, j];
                }
            }
        }
        return cost;
    }
    public int NumConnections() {
        int sum = 0;
        for (int i = 0; i < adjMatrix.GetLength(0); i++) {
            for (int j = 0; j < adjMatrix.GetLength(1); j++) {
                if (adjMatrix[i, j] > 0) {
                    sum += 1;
                }
            }
        }
        return sum;
    }
    public float OutgoingCost(int idx) {
        float cost = 0;
        if (idx < adjMatrix.GetLength(0)) {
            for (int j = 0; j < adjMatrix.GetLength(1); j++) {
                if (adjMatrix[idx,j] > 0) { cost += adjMatrix[idx,j]; }
            }
        }
        return cost;
    }
    public float IncomingCost(int idx) {
        float cost = 0;
        if (idx < adjMatrix.GetLength(1)) {
            for (int i = 0; i < adjMatrix.GetLength(0); i++) {
                if (adjMatrix[i,idx] > 0) { cost += adjMatrix[i,idx]; }
            }
        }
        return cost;
    }


    public int NumOutgoing(int idx) {
        int num = 0;
        if (idx < adjMatrix.GetLength(0)) {
            for (int j = 0; j < adjMatrix.GetLength(1); j++) {
                if (adjMatrix[idx,j] > 0) { num += 1; }
            }
            return num;
        } else {
            return -1;
        }
    }
    public int NumIncoming(int idx) {
        int num = 0;
        if (idx < adjMatrix.GetLength(1)) {
            for (int i = 0; i < adjMatrix.GetLength(0); i++) {
                if (adjMatrix[i,idx] > 0) { num += 1; }
            }
            return num;
        } else {
            return -1;
        }
    }

    // https://stackoverflow.com/questions/8124626/finding-connected-components-of-adjacency-matrix-graph
    public List<List<int>> GetConnectedComponents(bool undirected=true) {
        bool[] visited = new bool[nodes.Count];
        for (int i=0; i<visited.Length; i++) { visited[i] = false; }
        List<int> queue = new List<int>();
        List<List<int>> comps = new List<List<int>>();
        for(int i = 0; i < visited.Length; i++) {
            if (!visited[i]) {
                List<int> component = new List<int>();
                queue.Add(i);
                while (queue.Count > 0) {
                    int v = queue[0];
                    queue.RemoveAt(0);
                    if (!visited[v]) {
                        component.Add(v);
                        visited[v] = true;
                        for (int j = 0; j < adjMatrix.GetLength(1); j++) {
                            if (!visited[j] && AreConnected(v,j,undirected)) {
                                Debug.Log(System.String.Format("{0} connected to {1}", v, j));
                                queue.Add(j);
                            }
                        }
                    }
                }
                comps.Add(component);
            }
        }
        // string s = "";
        // foreach (List<int> component in comps) {
        //     s += "Component: ";
        //     foreach (int v in component) {
        //         s += v.ToString() +", ";
        //     }
        //     s += "\n";
        // }
        // Debug.Log(s);
        
        return comps;
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
