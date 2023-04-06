using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NetworkFormationGame : MonoBehaviour
{

    private AgentGraph backupAgentGraph;
    public AgentGraph agentGraph;
    // public Graph<Agent> initialGraph;
    // protected Graph<Agent> workingGraph;

    [SerializeField]
    protected List<NetworkEvent> workingHistory;
    [SerializeField]
    protected List<NetworkEvent> history;

    [Range(0.01f, 1f)]
    public float stepTime = 0.1f;
    private IEnumerator simCoroutine;
    private bool running = false;

    public int time = 0;

    [SerializeField]
    protected int currPlayerIdx;

    [SerializeField]
    private List<ProposedEdge> proposedEdges;

    public UnityEvent onReset;
    public UnityEvent onPostStep;
    public UnityEvent onUndoStep;

    void Start() {
        workingHistory = new List<NetworkEvent>();
        history = new List<NetworkEvent>();
        proposedEdges = new List<ProposedEdge>();
        
        if (agentGraph != null) { LinkGraph(agentGraph); }
    }

    public void LinkGraph(AgentGraph graph) {
        agentGraph = graph;
        // initialGraph = agentGraph.graph;
        // workingGraph = new Graph<Agent>(initialGraph.nodes);
        CopyAgentGraph();
    }

    private void CopyAgentGraph() {
        if (backupAgentGraph != null) { Destroy(backupAgentGraph.gameObject); }
        backupAgentGraph = Instantiate(agentGraph);
        backupAgentGraph.CopyFromBackup(agentGraph);
        backupAgentGraph.gameObject.SetActive(false);
    }

    protected List<ProposedEdge> GetIncomingProposals(Agent agent) { return proposedEdges.FindAll(p => p.end == agent); }
    protected List<ProposedEdge> GetOutgoingProposals(Agent agent) { return proposedEdges.FindAll(p => p.start == agent); }

    // Resets the parameters and history of the game
    // Should also be called when user changes the graph.
    public virtual void Reset() {
        agentGraph.DeselectAll();
        agentGraph.inputEnabled = false;
        LinkGraph(agentGraph);

        workingHistory.Clear();
        history.Clear();
        proposedEdges.Clear();

        // workingGraph.CopyFrom(initialGraph);
        time = 0;
        if (simCoroutine != null) { StopCoroutine(simCoroutine); }
        simCoroutine = RunCoroutine();

        // agentGraph.graph = workingGraph;
        agentGraph.RefreshView();

        currPlayerIdx = -1;

        onReset.Invoke();
        // Debug.Log("backup agent graph:\n" + backupAgentGraph.graph.ToString());
    }

    private IEnumerator RunCoroutine() {
        yield return null;
        while(true) {
            // Skip if simulation is paused or if graph is user-editable for SOME reason
            if (!running || agentGraph.inputEnabled) {
                yield return null;
            } else {
                // Perform the step
                float startTime = Time.realtimeSinceStartup;
                Step();
                float endTime = Time.realtimeSinceStartup;
                float waitTime = Mathf.Max(0.01f, stepTime - (endTime-startTime));
                yield return new WaitForSeconds(waitTime);
            }
        }
    }

    public void Run() {
        // if (simCoroutine)
        running = true;
        StartCoroutine(simCoroutine);
    }

    public void Resume() {
        agentGraph.inputEnabled = false;
        // simCoroutine
    }

    public void Pause() {
        running = false;
    }

    // Ends the game
    public void Stop() {
        Pause();
        agentGraph.ClearPEdges();
        agentGraph.inputEnabled = true;
        agentGraph.RefreshView();
    }

    // Ends the game and resets the AgentGraph to its state before the the game
    public void StopRestoreGraph() {
        Pause();
        agentGraph.ClearPEdges();
        agentGraph.CopyFromBackup(backupAgentGraph);
        agentGraph.inputEnabled = true;
        agentGraph.RefreshView();
    }

    public void Step() {
        PlanStep();
        CommitStep();
        time += 1;
        onPostStep.Invoke();
    }

    protected void Plan(NetworkEvent ne) {
        ne.step = time;
        workingHistory.Add(ne);
    }

    // Represents an atomic "step" of the formation game
    // Ideally the smallest unit of decision that occurs during the game
    // Creates list of NetworkEvents representing changes to the network
    protected virtual void PlanStep() {
        currPlayerIdx = (currPlayerIdx + 1) % agentGraph.agents.Count;
    }

    private void CommitStep() {
        foreach (NetworkEvent ne in workingHistory) {
            if (ne.action == NetworkAction.CONNECT) {
                agentGraph.AddEdge(ne.agentStart, ne.agentEnd);
            } else if (ne.action == NetworkAction.DISCONNECT) {
                agentGraph.RemoveEdge(agentGraph.EdgeAt(ne.agentStart, ne.agentEnd));
            } else if (ne.action == NetworkAction.PROPOSE_EDGE) {
                proposedEdges.Add(ne.proposal);
                agentGraph.AddPEdge(ne.agentStart, ne.agentEnd);
            } else if (ne.action == NetworkAction.ACCEPT_EDGE) {
                proposedEdges.Remove(ne.proposal);
                agentGraph.RemovePEdge(agentGraph.PEdgeAt(ne.proposal.start, ne.proposal.end));
                agentGraph.AddEdge(ne.proposal.start, ne.proposal.end);
            } else if (ne.action == NetworkAction.DENY_EDGE) {
                agentGraph.RemovePEdge(agentGraph.PEdgeAt(ne.proposal.start, ne.proposal.end));
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
            agentGraph.RemovePEdge(agentGraph.PEdgeAt(ne.proposal.start, ne.proposal.end));
        } else if (ne.action == NetworkAction.ACCEPT_EDGE) {
            // Debug.Log(ne.proposal.start);
            // Debug.Log(agentGraph.agents.IndexOf(ne.proposal.start));
            // Debug.Log(ne.proposal.end);
            // Debug.Log(agentGraph.agents.IndexOf(ne.proposal.end));
            // Remove edge from real graph first, so it looks like they are no longer
            // connected before adding pEdge back
            agentGraph.RemoveEdge(agentGraph.EdgeAt(ne.proposal.start, ne.proposal.end));
            proposedEdges.Add(ne.proposal);
            agentGraph.AddPEdge(ne.proposal.start, ne.proposal.end);
        } else if (ne.action == NetworkAction.DENY_EDGE) {
            proposedEdges.Add(ne.proposal);
            agentGraph.AddPEdge(ne.proposal.start, ne.proposal.end);
        }
        history.Remove(ne);
    }

    // Attempts to undo every NetworkEvent from last timestep and
    // decrements time
    public void Undo() {
        if (time-1 <= 0) { currPlayerIdx = -1; }
        else { currPlayerIdx = (currPlayerIdx + agentGraph.agents.Count - 1) % agentGraph.agents.Count; }

        time = Mathf.Max(0, time-1);
        // Find events from last time step
        List<NetworkEvent> found = history.FindAll(e => e.step == time);
        // Reverse list so that the last events that were committed are the first events to be reversed
        found.Reverse();
        foreach (NetworkEvent ne in found) {
            UndoEvent(ne);
        }
        onUndoStep.Invoke();
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