using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;


public class SimultaneousMoveGame : NetworkFormationGame
{
    // public float proposalChance = 0.5f;
    // public float acceptChance = 0.5f;

    // Resets the parameters and history of the game
    // Should also be called when user changes the graph.
    public override void Reset() {
        base.Reset();
    }

    // Represents an atomic "step" of the formation game
    // Ideally the smallest unit of decision that occurs during the game
    // Creates list of NetworkEvents representing changes to the network
    protected override void PlanStep() {
        base.PlanStep();
        SimultaneousPlan();
        base.PostPlanStep();
    }

    private void SimultaneousPlan() {
        NetworkEvent ne = null;
        // Simultaneous proposals on even steps
        // Edge commits on odd steps
        // PROPOSALS
        if (time >= 2) { return; }
        if (time % 2 == 0) {
            // Look at all edges,
            // If adding edge would improve my payoff, then propose edge
            for (int i = 0; i < agentGraph.agents.Count; i++) {
                Agent agent = agentGraph.agents[i];
                float myAlloc = agentGraph.allocations[i];
                float maxAlloc = myAlloc;
                Debug.Log(i + " alloc: " + myAlloc);
                for (int j = 0; j < agentGraph.agents.Count; j++) {
                    if (i != j) {
                        Agent other = agentGraph.agents[j];
                        AdjGraph<Agent> adjacent = agentGraph.GetAdjacentGraph(agent, other);
                        List<float> newAllocations = CalculateTempAllocations(adjacent);
                        // Propose an edge if that single connection would increase my payoff
                        // NOTE: this does NOT consider my payoff if I were to get multiple connections at the same time.
                        Debug.Log("My Hypothetical Alloc: (" + i + "," + j + "): " + newAllocations[i]);
                        if (newAllocations[i] > myAlloc) {
                            if (!agentGraph.graph.AreConnected(agent, other, agentGraph.undirected)) {
                                ne = PlanProposeEdge(agent, other);
                                Plan(ne);
                            }
                        }
                    }
                }
            }
        } else {
            foreach (Agent agent in agentGraph.agents) {
                List<ProposedEdge> outgoings = GetOutgoingProposals(agent);
                foreach (ProposedEdge incoming in GetIncomingProposals(agent)) {
                    // Form a link if other person also proposed a link with you, otherwise deny incoming edge
                    if (outgoings.Exists(x => x.end == incoming.start)) {
                        ne = PlanAcceptEdge(incoming);
                    } else {
                        ne = PlanDenyEdge(incoming);
                    }
                    Plan(ne);
                }
            }
        }
    }
}
