using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ConnectionsModel : NetworkRules
{

    [Range(0f,2f)]
    public float delta;

    void Awake() {
        base.Awake();
        direction = ValueFlow.ALLOC_TO_VALUE;
    }

    // Maps the graph to a real number using the defined rules
    public override float GetNetworkValue(Graph<Agent> network, List<float> allocations=null) {
        float value = 0;
        if (allocations != null) {
            foreach (float alloc in allocations) {
                value += alloc;
            }
        }
        return value;
    }

    public List<float> GetAllocation(Graph<Agent> network) { return GetAllocation(network, GetNetworkValue(network)); }
    public override List<float> GetAllocation(Graph<Agent> network, float value) {

        List<float> allocations = new List<float>();
        for (int i = 0; i < network.nodes.Count; i++) {
            int[] geo_distances = Dijkstra.DijkstraAlgoHops(network, i);
            float alloc = 0;
            foreach (int geo_dist in geo_distances) {
                if (geo_dist != int.MaxValue) {
                    alloc += Mathf.Pow(delta, geo_dist);
                }
            }

            // string s = "" + i + ": ";
            // foreach (int geo_dist in geo_distances) {
            //     s += geo_dist.ToString() + ", ";
            // }
            // Debug.Log(s);
            // exponent = 0, score = 1 for node connection to itself
            // Remove score from connection of node to itself
            alloc -= 1;
            // Cost
            alloc -= network.OutgoingCost(i);
            allocations.Add(alloc);
            // Debug.Log("" + i + ": " + network.OutgoingCost(i));
        }

        return allocations;
    }
}