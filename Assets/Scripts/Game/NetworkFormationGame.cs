using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkFormationGame : MonoBehaviour
{

    public AgentGraph agentGraph;
    public Graph<Agent> initialGraph;
    protected Graph<Agent> workingGraph;

    [SerializeField]
    protected List<NetworkEvent> workingHistory;
    [SerializeField]
    protected List<NetworkEvent> history;

    public float stepTime = 0.1f;
    private IEnumerator simCoroutine;
    private bool running = false;

    public int time = 0;

    [SerializeField]
    private List<ProposedEdge> proposedEdges;

    void Start() {
        workingHistory = new List<NetworkEvent>();
        history = new List<NetworkEvent>();
        proposedEdges = new List<ProposedEdge>();
        
        if (agentGraph != null) { LinkGraph(agentGraph); }
    }

    public void LinkGraph(AgentGraph graph) {
        agentGraph = graph;
        initialGraph = agentGraph.graph;
        workingGraph = new Graph<Agent>(initialGraph.nodes);
    }

    protected List<ProposedEdge> GetIncomingProposals(Agent agent) { return proposedEdges.FindAll(p => p.end == agent); }
    protected List<ProposedEdge> GetOutgoingProposals(Agent agent) { return proposedEdges.FindAll(p => p.start == agent); }

    // Resets the parameters and history of the game
    // Should also be called when user changes the graph.
    public virtual void Reset() {
        workingHistory.Clear();
        history.Clear();
        proposedEdges.Clear();

        workingGraph.CopyFrom(initialGraph);
        time = 0;
        if (simCoroutine != null) { StopCoroutine(simCoroutine); }
        simCoroutine = RunCoroutine();

        agentGraph.graph = workingGraph;
        agentGraph.RefreshView();
    }

    private IEnumerator RunCoroutine() {
        yield return null;
        while(true) {
            // Skip if simulation is paused
            if (!running) { yield return null;}
            // Perform the step
            float startTime = Time.realtimeSinceStartup;
            Step();
            float endTime = Time.realtimeSinceStartup;
            float waitTime = Mathf.Max(0.01f, stepTime - (endTime-startTime));
            yield return new WaitForSeconds(waitTime);
        }
    }

    public void Run() {
        // if (simCoroutine)
    }

    public void Resume() {
        agentGraph.inputEnabled = false;
        // simCoroutine
    }

    public void Stop() {
        agentGraph.inputEnabled = true;
    }

    public void Step() {
        PlanStep();
        CommitStep();
        time += 1;
    }

    protected void Plan(NetworkEvent ne) {
        ne.step = time;
        workingHistory.Add(ne);
    }

    // Represents an atomic "step" of the formation game
    // Ideally the smallest unit of decision that occurs during the game
    // Creates list of NetworkEvents representing changes to the network
    protected virtual void PlanStep() {
        return;
    }

    private void CommitStep() {
        foreach (NetworkEvent ne in workingHistory) {
            if (ne.action == NetworkAction.CONNECT) {
                agentGraph.AddEdge(ne.agentStart, ne.agentEnd);
            } else if (ne.action == NetworkAction.DISCONNECT) {
                agentGraph.RemoveEdge(agentGraph.EdgeAt(ne.agentStart, ne.agentEnd));
            } else if (ne.action == NetworkAction.PROPOSE_EDGE) {
                proposedEdges.Add(ne.proposal);
            } else if (ne.action == NetworkAction.ACCEPT_EDGE) {
                proposedEdges.Remove(ne.proposal);
                agentGraph.AddEdge(ne.proposal.start, ne.proposal.end);
            } else if (ne.action == NetworkAction.DENY_EDGE) {
                proposedEdges.Remove(ne.proposal);
            }
            history.Add(ne);
        }
        workingHistory.Clear();
    }

    private void UndoEvent(NetworkEvent ne) {
        if (ne.action == NetworkAction.CONNECT) {
            agentGraph.RemoveEdge(agentGraph.EdgeAt(ne.agentStart, ne.agentEnd));
        } else if (ne.action == NetworkAction.DISCONNECT) {
            agentGraph.AddEdge(ne.agentStart, ne.agentEnd);
        } else if (ne.action == NetworkAction.PROPOSE_EDGE) {
            proposedEdges.Remove(ne.proposal);
        } else if (ne.action == NetworkAction.ACCEPT_EDGE) {
            proposedEdges.Add(ne.proposal);
            agentGraph.RemoveEdge(agentGraph.EdgeAt(ne.proposal.start, ne.proposal.end));
        } else if (ne.action == NetworkAction.DENY_EDGE) {
            proposedEdges.Add(ne.proposal);
        }
        history.Remove(ne);
    }

    // Attempts to undo every NetworkEvent from last timestep and
    // decrements time
    public void Undo() {
        time -= 1;
        // Find events from last time step
        List<NetworkEvent> found = history.FindAll(e => e.step == time);
        // Reverse list so that the last events that were committed are the first events to be reversed
        found.Reverse();
        foreach (NetworkEvent ne in found) {
            UndoEvent(ne);
        }
    }


    protected NetworkEvent PlanConnect(Agent a1, Agent a2) {
        NetworkEvent ne = new NetworkEvent();
        if (agentGraph.graph.AreConnected(a1, a2)) {
            ne.action = NetworkAction.DO_NOTHING;
        } else{
            ne.action = NetworkAction.CONNECT;
            ne.agentStart = a1;
            ne.agentEnd = a2;
        }
        return ne;
    }

    protected NetworkEvent PlanDisconnect(Agent a1, Agent a2) {
        NetworkEvent ne = new NetworkEvent();
        if (agentGraph.graph.AreConnected(a1, a2)) {
            ne.action = NetworkAction.DISCONNECT;
            ne.agentStart = a1;
            ne.agentEnd = a2;
            
        } else{
            ne.action = NetworkAction.DO_NOTHING;
        }
        return ne;
    }

    protected NetworkEvent PlanProposeEdge(Agent a1, Agent a2) {
        NetworkEvent ne = new NetworkEvent();
        if (agentGraph.graph.AreConnected(a1, a2)) {
            ne.action = NetworkAction.DO_NOTHING;
        } else{
            ne.action = NetworkAction.PROPOSE_EDGE;
            ne.agentStart = a1;
            ne.agentEnd = a2;
            ne.proposal = new ProposedEdge(a1, a2);
        }
        return ne;
    }
    protected NetworkEvent PlanAcceptEdge(ProposedEdge pedge) {
        NetworkEvent ne = new NetworkEvent();
        if (agentGraph.graph.AreConnected(pedge.start, pedge.end)) {
            ne.action = NetworkAction.DO_NOTHING;
        } else{
            ne.action = NetworkAction.ACCEPT_EDGE;
            ne.proposal = pedge;
        }
        return ne;
    }
    protected NetworkEvent PlanDenyEdge(ProposedEdge pedge) {
        NetworkEvent ne = new NetworkEvent();
        if (agentGraph.graph.AreConnected(pedge.start, pedge.end)) {
            ne.action = NetworkAction.DO_NOTHING;
        } else{
            ne.action = NetworkAction.DENY_EDGE;
            ne.proposal = pedge;
        }
        return ne;
    }
}

[System.Serializable]
public class ProposedEdge {
    public Agent start;
    public Agent end;
    public ProposedEdge(Agent s, Agent e) {
        start = s; end = e;
    }
}

// Represents a change that the game makes to the network
[System.Serializable]
public class NetworkEvent {
    public int step;
    public NetworkAction action;

    public Agent agentStart;
    public Agent agentEnd;

    public ProposedEdge proposal;
}

// Represents the type of change made
[System.Serializable]
public enum NetworkAction {
    PROPOSE_EDGE,
    ACCEPT_EDGE,
    DENY_EDGE,
    CONNECT,
    DISCONNECT,
    DO_NOTHING,
}