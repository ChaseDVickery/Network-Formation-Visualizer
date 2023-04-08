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
            SimultaneousPlan();
        } else {
            SequentialPlan();
        }
        // NetworkEvent ne = PlanConnect(agentGraph.agents[0], agentGraph.agents[1]);
        // ne.step = time;
        // workingHistory.Add(ne);
        base.PostPlanStep();
    }

    private void SimultaneousPlan() {
        NetworkEvent ne = null;
        // Simultaneous proposals on even steps
        // Edge acceptance on odd steps
        if (time % 2 == 0) {
            for (int i = 0; i < agentGraph.agents.Count; i++) {
                Agent agent = agentGraph.agents[i];
                for (int j = 0; j < agentGraph.agents.Count; j++) {
                    if (i != j) {
                        Agent other = agentGraph.agents[j];
                        // Chance to propose if not already connected
                        if (!agentGraph.graph.AreConnected(i, j, agentGraph.undirected)) {
                            float roll = RandomQueue.value;
                            if (roll <= proposalChance) {
                                ne = PlanProposeEdge(agent, other);
                            } else {
                                ne = PlanDoNothing();
                            }
                            ne.numRandoms += 1;
                            Plan(ne);
                        }
                    }
                }
            }
        } else {
            foreach (Agent agent in agentGraph.agents) {
                // Accept a link
                List<ProposedEdge> outgoings = GetOutgoingProposals(agent);
                foreach (ProposedEdge incoming in GetIncomingProposals(agent)) {
                    // Form a link if other person also proposed a link with you, otherwise 
                    // ProposedEdge pedge = outgoings.Find(x => x.start == incoming.end);
                    // if (pedge != null) {
                    //     ne = PlanAcceptEdge(pedge);
                    // } else {
                    //     ne = PlanDenyEdge(incoming);
                    // }
                    if (outgoings.Exists(x => x.end == incoming.start)) {
                        ne = PlanAcceptEdge(incoming);
                    } else {
                        ne = PlanDenyEdge(incoming);
                    }
                    Plan(ne);
                }
                // List<ProposedEdge> incomings = GetIncomingProposals(agent);
                // foreach (ProposedEdge outgoing in GetOutgoingProposals(agent)) {
                //     // Accept an incoming edge if you also proposed the the proposer
                //     // Rescind your outgoing proposal if propsed did not propose back to you
                //     ProposedEdge pedge = incomings.Find(x => x.end == outgoing.start);
                //     if (pedge != null) {
                //         ne = PlanAcceptEdge(pedge);
                //     } else {
                //         ne = PlanDenyEdge(outgoing);
                //     }
                //     Plan(ne);
                // }
            }
        }
    }

    private void SequentialPlan() {
        NetworkEvent ne = null;
        // Order over players who propose links
        Agent curr = agentGraph.agents[currPlayerIdx];
        currHighlights.Add(curr);
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
