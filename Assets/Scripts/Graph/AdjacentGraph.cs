using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Subclass for a Graph<> which keeps track of some additional information that
// describes more about the adjacent aspects
public class AdjGraph<T> : Graph<T> where T : class
{
    public int i;
    public int j;

    public AdjGraph(List<T> startNodes, int i, int j) : base(startNodes) {
        SetAdjacentEdge(i, j);
    }

    public AdjGraph(List<T> startNodes) : this(startNodes, -1, -1) { }

    public void SetAdjacentEdge(int i, int j) {
        this.i = i;
        this.j = j;
    }
}
