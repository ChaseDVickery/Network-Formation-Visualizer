using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class RandomGame : NetworkFormationGame
{
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
        SequentialPlan();
        base.PostPlanStep();
    }

    private void SequentialPlan() {
        NetworkEvent ne = null;
        // Choose random link, Check if toggling link would
        // Order over players who propose links
        Agent curr = agentGraph.agents[currPlayerIdx];
        currHighlights.Add(curr);
        // Propose random edge insertion or deletion
        if (time % 2 == 0) {

        } else {

        }
        // Accept or deny any proposed links involving you as the end
        List<Agent> accepted = new List<Agent>();
        foreach (ProposedEdge pedge in GetIncomingProposals(curr)) {
            float roll = RandomQueue.value;
            if (roll <= acceptChance) {
                ne = PlanAcceptEdge(pedge);
                // Track accepted edge to make sure you don't propose back on same planning step
                if (ne.action == NetworkAction.ACCEPT_EDGE) { accepted.Add(pedge.start); }
            } else {
                ne = PlanDenyEdge(pedge);
            }
            ne.numRandoms += 1;
            Plan(ne);
        }
        // Send out potential proposals
        for (int i = 0; i < agentGraph.agents.Count; i++) {
            Agent other = agentGraph.agents[i];
            // Chance to propose to NOT yourself and not already connected and not accepted proposal
            if (i != currPlayerIdx && !accepted.Contains(other) && !agentGraph.graph.AreConnected(curr, other, agentGraph.undirected)) {
                float roll = RandomQueue.value;
                if (roll <= proposalChance) {
                    ne = PlanProposeEdge(curr, agentGraph.agents[i]);
                } else {
                    ne = PlanDoNothing();
                }
                ne.numRandoms += 1;
                Plan(ne);
            }
        }
    }

}
