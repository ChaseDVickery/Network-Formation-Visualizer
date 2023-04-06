using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class RandomGame : NetworkFormationGame
{
    public bool simultaneous;
    public float proposalChance = 0.5f;
    public float acceptChance = 0.5f;

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
        if (simultaneous) {

        } else {
            NetworkEvent ne = null;
            // Order over players who propose links
            Agent curr = agentGraph.agents[currPlayerIdx];
            // Accept or deny any proposed links involving you as the end
            List<Agent> accepted = new List<Agent>();
            foreach (ProposedEdge pedge in GetIncomingProposals(curr)) {
                if (Random.value <= acceptChance) {
                    ne = PlanAcceptEdge(pedge);
                    // Track accepted edge to make sure you don't propose back on same planning step
                    if (ne.action == NetworkAction.ACCEPT_EDGE) { accepted.Add(pedge.start); }
                } else {
                    ne = PlanDenyEdge(pedge);
                }
                Plan(ne);
            }
            // Send out potential proposals
            for (int i = 0; i < agentGraph.agents.Count; i++) {
                Agent other = agentGraph.agents[i];
                // Chance to propose to NOT yourself and not already connected and not accepted proposal
                if (i != currPlayerIdx && !accepted.Contains(other) && !agentGraph.graph.AreConnected(curr, other)) {
                    if (Random.value <= proposalChance) {
                        ne = PlanProposeEdge(curr, agentGraph.agents[i]);
                        Plan(ne);
                    }
                }
            }
            
        }
        // NetworkEvent ne = PlanConnect(agentGraph.agents[0], agentGraph.agents[1]);
        // ne.step = time;
        // workingHistory.Add(ne);
    }

    // private void Simul
}
