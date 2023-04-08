using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Interactable;

public class NetworkFormationGame : MonoBehaviour
{

    private AgentGraph backupAgentGraph;
    public AgentGraph agentGraph;
    // public Graph<Agent> initialGraph;
    // protected Graph<Agent> workingGraph;

    protected List<List<IInteractable>> highlights;
    protected List<IInteractable> currHighlights;

    [SerializeField]
    protected List<NetworkEvent> workingHistory;
    [SerializeField]
    protected List<NetworkEvent> history;

    [Range(0.01f, 1f)]
    public float stepTime = 0.1f;
    private IEnumerator runCoroutine;
    private IEnumerator reverseCoroutine;
    private bool runningRun = false;
    private bool runningReverse = false;

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
        highlights = new List<List<IInteractable>>();
        
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
        highlights.Clear();

        agentGraph.DeselectAll();
        agentGraph.inputEnabled = false;
        LinkGraph(agentGraph);

        workingHistory.Clear();
        history.Clear();
        proposedEdges.Clear();

        // workingGraph.CopyFrom(initialGraph);
        time = 0;
        if (runCoroutine != null) { StopCoroutine(runCoroutine); }
        runCoroutine = RunCoroutine();
        if (reverseCoroutine != null) { StopCoroutine(reverseCoroutine); }
        reverseCoroutine = ReverseCoroutine();

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
            if (!runningRun || runningReverse || agentGraph.inputEnabled) {
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

    private IEnumerator ReverseCoroutine() {
        yield return null;
        while(true) {
            // Skip if simulation is paused or if graph is user-editable for SOME reason
            if (!runningReverse || runningRun || agentGraph.inputEnabled) {
                yield return null;
            } else {
                // Perform the step
                float startTime = Time.realtimeSinceStartup;
                Undo();
                float endTime = Time.realtimeSinceStartup;
                float waitTime = Mathf.Max(0.01f, stepTime - (endTime-startTime));
                yield return new WaitForSeconds(waitTime);
            }
        }
    }

    public void Run() {
        runningRun = true;
        runningReverse = false;
        if (reverseCoroutine != null) { StopCoroutine(reverseCoroutine); }
        if (runCoroutine == null) { runCoroutine = RunCoroutine(); }
        StartCoroutine(runCoroutine);
    }

    public void Reverse() {
        runningRun = false;
        runningReverse = true;
        if (runCoroutine != null) { StopCoroutine(runCoroutine); }
        if (reverseCoroutine == null) { reverseCoroutine = ReverseCoroutine(); }
        StartCoroutine(reverseCoroutine);
    }

    public void Resume() {
        agentGraph.inputEnabled = false;
    }

    public void Pause() {
        runningRun = false;
        runningReverse = false;
        if (runCoroutine != null) { StopCoroutine(runCoroutine); }
        if (reverseCoroutine != null) { StopCoroutine(reverseCoroutine); }
    }

    // Ends the game
    public void Stop() {
        if (highlights.Count >= 1) {
            foreach (IInteractable interactable in highlights[highlights.Count-1]) {
                interactable.Deselect();
            }
        }
        Pause();
        agentGraph.ClearPEdges();
        agentGraph.inputEnabled = true;
        agentGraph.RefreshView();
    }

    // Ends the game and resets the AgentGraph to its state before the the game
    public void StopRestoreGraph() {
        if (highlights.Count >= 1) {
            foreach (IInteractable interactable in highlights[highlights.Count-1]) {
                interactable.Deselect();
            }
            highlights.RemoveAt(highlights.Count-1);
        }
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
        currHighlights = new List<IInteractable>();
        currPlayerIdx = (currPlayerIdx + 1) % agentGraph.agents.Count;
    }

    protected void PostPlanStep() {
        highlights.Add(currHighlights);
    }

    private void CommitStep() {
        // Apply each planned action to the network
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
                // if (agentGraph.undirected) { agentGraph.RemovePEdge(agentGraph.PEdgeAt(ne.proposal.end, ne.proposal.start)); }
                agentGraph.AddEdge(ne.proposal.start, ne.proposal.end);
            } else if (ne.action == NetworkAction.DENY_EDGE) {
                agentGraph.RemovePEdge(agentGraph.PEdgeAt(ne.proposal.start, ne.proposal.end));
                proposedEdges.Remove(ne.proposal);
            }
            history.Add(ne);
        }
        workingHistory.Clear();
        ChangeHighlights();
    }

    // Remove highlights from last step and add those from this step
    private void ChangeHighlights() {
        // Deselect those from last step
        if (highlights.Count >= 2) {
            foreach (IInteractable interactable in highlights[highlights.Count-2]) {
                interactable.Deselect();
            }
        }
        // Select those from this step
        if (highlights.Count >= 1) {
            foreach (IInteractable interactable in highlights[highlights.Count-1]) {
                interactable.Select();
            }
        }
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

        // Undo random rolls if there were any
        for (int i = 0; i < ne.numRandoms; i++) {
            RandomQueue.Undo();
        }
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
        // Reverse highlights (deselect at current step, select at prior step)
        // Select those from last step
        if (highlights.Count >= 2) {
            foreach (IInteractable interactable in highlights[highlights.Count-2]) {
                interactable.Select();
            }
        }
        // Deselect those from this step and remove highlights of undone step
        if (highlights.Count >= 1) {
            foreach (IInteractable interactable in highlights[highlights.Count-1]) {
                interactable.Deselect();
            }
            highlights.RemoveAt(highlights.Count-1);
        }
        onUndoStep.Invoke();
    }

    protected NetworkEvent PlanDoNothing() {
        NetworkEvent ne = new NetworkEvent();
        ne.action = NetworkAction.DO_NOTHING;
        return ne;
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
        // if (agentGraph.graph.AreConnected(a1, a2)) {
        //     ne.action = NetworkAction.DO_NOTHING;
        // } else{
            ne.action = NetworkAction.PROPOSE_EDGE;
            ne.agentStart = a1;
            ne.agentEnd = a2;
            ne.proposal = new ProposedEdge(a1, a2);
        // }
        return ne;
    }
    protected NetworkEvent PlanAcceptEdge(ProposedEdge pedge) {
        NetworkEvent ne = new NetworkEvent();
        // if (agentGraph.graph.AreConnected(pedge.start, pedge.end)) {
        //     ne.action = NetworkAction.DO_NOTHING;
        // } else{
            ne.action = NetworkAction.ACCEPT_EDGE;
            ne.proposal = pedge;
        // }
        return ne;
    }
    protected NetworkEvent PlanDenyEdge(ProposedEdge pedge) {
        NetworkEvent ne = new NetworkEvent();
        // if (agentGraph.graph.AreConnected(pedge.start, pedge.end)) {
        //     ne.action = NetworkAction.DO_NOTHING;
        // } else{
            ne.action = NetworkAction.DENY_EDGE;
            ne.proposal = pedge;
        // }
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

    public int numRandoms = 0;
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