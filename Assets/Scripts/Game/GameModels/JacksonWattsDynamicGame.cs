using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;

using TMPro;

public class JacksonWattsDynamicGame : NetworkFormationGame
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
        SequentialPlan();
        base.PostPlanStep();
    }

    private void SequentialPlan() {
        NetworkEvent ne = null;
        // Choose random link, Check if toggling link would
        // Order over players who propose links
        // Propose random edge insertion or deletion
        if (time % 2 == 0) {
            // Shuffle (uses randomness, account for in RandomQueue)
            List<int> idcs = Enumerable.Range(0,agentGraph.agents.Count).ToArray().ToList();
            idcs.Shuffle();
            ne = PlanDoNothing();
            ne.numRandomsInt = agentGraph.agents.Count-1;
            Plan(ne);
            Agent a1 = agentGraph.agents[idcs[0]];
            Agent a2 = agentGraph.agents[idcs[1]];
            float a1alloc = agentGraph.allocations[idcs[0]];
            float a2alloc = agentGraph.allocations[idcs[1]];
            AdjGraph<Agent> adjacent = agentGraph.GetAdjacentGraph(a1, a2);
            List<float> newAllocations = CalculateTempAllocations(adjacent);
            float a1new = newAllocations[idcs[0]];
            float a2new = newAllocations[idcs[1]];
            if (agentGraph.graph.AreConnected(a1, a2, agentGraph.undirected)) {
                // Propose Edge Destruction if a1 would be better off without it
                // should I also check for a2?
                if (a1new > a1alloc) {
                    ne = PlanDisconnect(a1, a2);
                    Plan(ne);
                } else {
                    currHighlights.Add(agentGraph.EdgeAt(a1, a2));
                }
            } else {
                // Propose Edge Insertion
                if ((a1new > a1alloc && a2new >= a2alloc) || (a1new >= a1alloc && a2new > a2alloc)) {
                    ne = PlanProposeEdge(a1, a2);
                    ne.proposal.preAccept = true;
                    Plan(ne);
                } else {
                    ne = PlanProposeEdge(a1, a2);
                    Plan(ne);
                }
            }
        } else {
            foreach (ProposedEdge pedge in GetProposals()) {
                if (pedge.preAccept) {
                    ne = PlanAcceptEdge(pedge);
                    Plan(ne);
                } else {
                    ne = PlanDenyEdge(pedge);
                    Plan(ne);
                }
            }
        }
    }

}
