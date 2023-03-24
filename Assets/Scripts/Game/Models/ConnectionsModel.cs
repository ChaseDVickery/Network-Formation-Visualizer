using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ConnectionsModel : NetworkRules
{
    // Maps the graph to a real number using the defined rules
    public override float GetNetworkValue(Graph<Agent> network) {
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
    public override List<float> GetAllocation(Graph<Agent> network, float value) {
        List<float> allocations = new List<float>();
        float[] weights = new float[network.nodes.Count];
        float total = 0f;
        for (int i = 0; i < network.nodes.Count; i++) {
            int no = network.NumOutgoing(i);
            weights[i] = no;
            total += no;
        }
        if (total != 0) {
            for (int i = 0; i < weights.Length; i++) {
                float allocation = value * (weights[i] / total);
                allocations.Add(allocation);
            }
        } else {
            for (int i = 0; i < weights.Length; i++) {
                allocations.Add(0);
            }
        }
        
        // string s = "Allocations:\n";
        // for (int i = 0; i < allocations.Count; i++) {
        //     s += System.String.Format("{0}: {1}\n", i, allocations[i]);
        // }
        // Debug.Log(s);

        return allocations;
    }
}