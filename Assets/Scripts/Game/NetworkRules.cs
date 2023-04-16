using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using System;
using System.Reflection;
using System.Linq;

public class NetworkRules : MonoBehaviour
{
    public delegate void GraphUpdateDelegate();
    public GraphUpdateDelegate graphUpdateDelegate;

    public UnityEvent onChange;
    public EdgeInfo edgeInfo;

    public ValueFlow direction;

    private List<Type> subclasses;

    protected void Awake() {
        subclasses = new List<Type>();
        // https://stackoverflow.com/questions/8928464/for-an-object-can-i-get-all-its-subclasses-using-reflection-or-other-ways
        foreach (Type type in typeof(NetworkRules).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(NetworkRules)))) {
            subclasses.Add(type);
        }
    }

    void Start() {
        
    }

    public void ApplyChanges() {
        graphUpdateDelegate?.Invoke();
        onChange.Invoke();
    }

    public virtual void UpdateInfo(string key, System.Object value, Type myType) {
        foreach (Type type in subclasses) {
            if (myType == type) {
                FieldInfo updateField = type.GetField(key);
                if (updateField != null) { updateField.SetValue(this, value); }
                else {
                updateField = typeof(EdgeInfo).GetField(key);
                object boxed = edgeInfo;
                if (updateField != null) { updateField.SetValue(boxed, value); edgeInfo = (EdgeInfo)boxed; }
                }
            }
        }
    }

    // Maps the graph to a real number using the defined rules
    public virtual float GetNetworkValue(Graph<Agent> network, List<float> allocations = null) {
        // List<List<int>> comps = network.GetConnectedComponents();
        // Debug.Log(System.String.Format("Network Value: {0}", comps.Count));
        int numConnections = network.NumConnections();
        float totalConnectionCost = network.TotalCost();
        // float value = Mathf.Max(0, 100 - totalConnectionCost);
        float value = 100 - totalConnectionCost;
        // Debug.Log(System.String.Format("Network Value: {0}", value));

        return value;
    }

    public List<float> GetAllocation(Graph<Agent> network) { return GetAllocation(network, GetNetworkValue(network)); }
    public virtual List<float> GetAllocation(Graph<Agent> network, float value) {
        List<float> allocations = new List<float>();
        for (int i = 0; i < network.nodes.Count; i++) {
            allocations.Add(0);
        }
        return allocations;
    }
}

[System.Serializable]
public struct EdgeInfo {
    public float weight;
    public bool scaleWithDistance;
}
[System.Serializable]
public enum ValueFlow {
    VALUE_TO_ALLOC,
    ALLOC_TO_VALUE,
};