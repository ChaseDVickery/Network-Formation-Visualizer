using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

using Interactable;

public class AgentGraph : MonoBehaviour
{

    public bool undirected = true;

    public NetworkRules rules;

    public bool makeAgentsOnStartup = true;
    public int numAgents;
    public GameObject agentPrefab;
    public GameObject edgePrefab;
    public GameObject proposedEdgePrefab;

    public GameObject selectionBoxPrimary;
    private Selector selectorPrimary;
    public GameObject selectionBoxSecondary;
    private Selector selectorSecondary;

    public List<Agent> agents;
    public List<Edge> edges;
    public Graph<Agent> graph;

    public float value { get; private set; }
    public List<float> allocations { get; private set; }

    public bool inputEnabled = true;
    public GameObject focus;
    public bool selecting = false;
    public List<IInteractable> selected;
    private bool dragging = false;
    private Vector3 startDragMousePosPrimary;
    private Vector3 startDragMousePosSecondary;
    private DragType dragType;

    private bool ctrlDown;
    private bool shiftDown;

    public UnityEvent onRefreshView;

    // private Dictionary<Agent, Dictionary<Agent, Edge>> edgeDict = new Dictionary<Agent, Dictionary<Edge>>();
    private Edge[,] edgeMatrix;
    private Edge[,] pEdgeMatrix;

    void Awake() {
        selectorPrimary = selectionBoxPrimary.GetComponent<Selector>();
        selectorSecondary = selectionBoxSecondary.GetComponent<Selector>();
        selectorSecondary.isAlt = true;

        // rules.onChange.AddListener(ApplyNetworkRules);
        if (makeAgentsOnStartup && numAgents > 0) {
            makeAgentsOnStartup = false;
            agents.Clear();
            for (int i = 0; i < numAgents; i++) {
                GameObject newAgentObj = Instantiate(agentPrefab);
                // newAgentObj.transform.position = new Vector3(Random.Range(-8.0f, 8.0f), Random.Range(-5.0f, 5.0f), 0);
                newAgentObj.transform.position = new Vector3(3*Mathf.Cos(2*Mathf.PI*i/(numAgents)), 3*Mathf.Sin(2*Mathf.PI*i/(numAgents)), 0);
                Agent a = newAgentObj.GetComponent<Agent>();
                a.agentName = "a" + i;
                agents.Add(a);
            }
        }
        List<Agent> tempList = new List<Agent>();
        foreach (Agent agent in agents) {
            tempList.Add(agent);
        }
        graph = new Graph<Agent>(tempList);
        Debug.Log(graph);

        edgeMatrix = InstantiateEdgeMatrix();
        pEdgeMatrix = InstantiateEdgeMatrix();
    }

    private Edge[,] InstantiateEdgeMatrix() {
        Edge[,] mat = new Edge[agents.Count, agents.Count];
        for (int i = 0; i < agents.Count; i++) {
            for (int j = 0; j < agents.Count; j++) {
                mat[i,j] = null;
            }
        }
        return mat;
    }

    public Edge[,] ResizeEdgeMatrix(Edge[,] mat) {
        // Copy existing edges to new matrix
        Edge[,] tempAdj = new Edge[agents.Count, agents.Count];
        // Makes sure we don't exceed bounds of temp or edgeMatrix whether expanding or contracting
        int rows = Mathf.Min(tempAdj.GetLength(0), mat.GetLength(0));
        int cols = Mathf.Min(tempAdj.GetLength(1), mat.GetLength(1));
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                tempAdj[i,j] = mat[i,j];
            }
        }
        // Fill in null for last row and column for new node with no connections
        // ONLY NEEDED IF EXTRA AGENT THAN PREVIOUSLY
        if (tempAdj.GetLength(0) == mat.GetLength(0) + 1) {
            for (int i = 0; i < mat.GetLength(0); i++) { tempAdj[agents.Count-1, i] = null; }
            for (int i = 0; i < mat.GetLength(0); i++) { tempAdj[i, agents.Count-1] = null; }
        }
        // Transfer temp matrix;
        return tempAdj;
    }

    public void CopyFromBackup(AgentGraph other) {
        // Destroy only edge objects that are not in other copy
        for (int i = 0; i < edgeMatrix.GetLength(0); i++) {
            for (int j = 0; j < edgeMatrix.GetLength(1); j++) {
                Edge edge = edgeMatrix[i,j];
                if (edge != null && !other.edges.Contains(edge)) {
                    Agent a1 = edge.n1.GetComponent<Agent>();
                    Agent a2 = edge.n2.GetComponent<Agent>();
                    a1.RemoveEdge(edge);
                    a2.RemoveEdge(edge);
                    graph.RemoveEdge(a1, a2);
                    edges.Remove(edge);
                    edgeMatrix[i, j] = null;
                    Destroy(edge.gameObject);
                }
            }
        }
        // Copy the edges
        edges = new List<Edge>();
        foreach (Edge edge in other.edges) { edges.Add(edge); }
        // Copy the agents
        agents = new List<Agent>();
        foreach (Agent agent in other.agents) { agents.Add(agent); }
        // Copy the graph
        graph = new Graph<Agent>(agents);
        graph.CopyFrom(other.graph);
        // Copy the edge matrices
        edgeMatrix = InstantiateEdgeMatrix();
        edgeMatrix = ResizeEdgeMatrix(other.edgeMatrix);
        pEdgeMatrix = InstantiateEdgeMatrix();
        pEdgeMatrix = ResizeEdgeMatrix(other.pEdgeMatrix);
        // edgeMatrix = other.edgeMatrix;
        // pEdgeMatrix = other.pEdgeMatrix;
    }

    // public void SetGraph(Graph<Agent> newGraph) {
    //     return;
    // }

    public void SetNetworkRules(NetworkRules newRules) {
        rules.graphUpdateDelegate -= ApplyNetworkRules;
        newRules.graphUpdateDelegate = ApplyNetworkRules;
        rules = newRules;
    }

    // Update is called once per frame
    void Update()
    {

        if (!inputEnabled) { return; }

        // if (Input.GetKeyDown("g")) {
        //     CalculateAllocation();
        // }

        if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1)) {
            SelectAllAgents();
        }
        if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2)) {
            SelectAllEdges();
        }

        if (Input.GetKeyDown("g")) {
            // GetAllAdjacentGraphs();
            IsPairwiseStable();
        }

        if (Input.GetKeyDown("r")) {
            // RemoveSelectedAgents();
            RemoveSelected();
            UpdateAllocations();
        }
        if (Input.GetKeyDown("a")) {
            ConnectAllSelected();
            UpdateAllocations();
        }
        if (Input.GetKeyDown("s")) {
            ConnectBipartite();
            UpdateAllocations();
        }
        if (Input.GetKeyDown("space")) {
            SmartAction();
            UpdateAllocations();
        }
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) { ctrlDown = true; }
        if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl)) { ctrlDown = false; }
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) { shiftDown = true; }
        if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift)) { shiftDown = false; }

        if (EventSystem.current.currentSelectedGameObject != null) {
            return;
        }

        if (Input.GetMouseButtonDown(0)) {
            RaycastHit2D rayHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            rayHit = Physics2D.GetRayIntersection(ray);
            if ( rayHit.collider != null ) {
                SelectObject(rayHit.transform.gameObject);
            } 
            else {
                SelectObject(null);
            }
            dragType = DragType.LEFT;
            startDragMousePosPrimary = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0)) {
            dragType = DragType.NONE;
            selectorPrimary.GetSnapshot();
            selectorPrimary.Deactivate();
            selectorPrimary.transform.position = new Vector3(-100, -100, 0);
        }

        if (Input.GetMouseButtonDown(1)) {
            RaycastHit2D rayHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            rayHit = Physics2D.GetRayIntersection(ray);
            if ( rayHit.collider != null ) {
                AltSelectObject(rayHit.transform.gameObject);
            } 
            else {
                AltSelectObject(null);
            }
            dragType = DragType.RIGHT;
            startDragMousePosSecondary = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(1)) {
            dragType = DragType.NONE;
            selectorSecondary.GetSnapshot();
            selectorSecondary.Deactivate();
            selectorSecondary.transform.position = new Vector3(-100, -100, 0);
        }

        if (dragType == DragType.LEFT) {
            Vector3 mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            if (selecting) {
                for (int i = 0; i < selectorPrimary.selected.Count; i++) {
                    if (selectorPrimary.selected[i].GetComponent<Agent>() != null)
                        selectorPrimary.selected[i].transform.position = selectorPrimary.positionSnapshot[i] + (mousePos - startDragMousePosPrimary);
                }
                RefreshView();
            }
            // Selection box
            else {
                Vector3 diff = (mousePos - startDragMousePosPrimary);
                Vector3 boxPos = startDragMousePosPrimary + (diff/2);
                boxPos.z = 0;
                selectionBoxPrimary.transform.position = boxPos;
                selectionBoxPrimary.transform.localScale = new Vector3(diff.x, diff.y, 1);
            }
        } else if (dragType == DragType.RIGHT) {
            Vector3 mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            if (selecting) {
                
            }
            // Selection box
            else {
                Vector3 diff = (mousePos - startDragMousePosSecondary);
                Vector3 boxPos = startDragMousePosSecondary + (diff/2);
                boxPos.z = 0;
                selectorSecondary.transform.position = boxPos;
                selectorSecondary.transform.localScale = new Vector3(diff.x, diff.y, 1);
            }
        }
    }

    public List<float> CalculateAllocation(Graph<Agent> toCalculate = null) {
        // Use my current graph if no other specified
        bool temp = true;
        if (toCalculate == null) {
            toCalculate = graph;
            temp = false;
        } 
        float v = System.Single.MinValue;
        List<float> a = null;
        // Calculate the Value of the graph as well as the per-agent allocations
        if (rules.direction == ValueFlow.VALUE_TO_ALLOC) {
            v = rules.GetNetworkValue(toCalculate, null);
            a = rules.GetAllocation(toCalculate, value);
        } else if (rules.direction == ValueFlow.ALLOC_TO_VALUE) {
            a = rules.GetAllocation(toCalculate, 0);
            v = rules.GetNetworkValue(toCalculate, allocations);
        }

        if (!temp) {
            allocations = a;
            value = v;
        }
        
        return a;
    }

    public void UpdateAllocations(Graph<Agent> toCalculate = null) {
        List<float> allocations = CalculateAllocation(toCalculate);
        for (int i = 0; i < agents.Count; i++) {
            agents[i].UpdateInfo("allocation", allocations[i]);
        }
    }

    public void DeselectAll() {
        selectorPrimary.ClearSelection();
        selectorSecondary.ClearSelection();
        SelectObject(null);
        AltSelectObject(null);
    }
    private void ClearSelections() {
        selectorPrimary.ClearSelection();
        selectorSecondary.ClearSelection();
    }
    private void SelectObject(GameObject o) {
        if (o != null) {
            if (selectorPrimary.selected.Contains(o)) {
                if (ctrlDown) { selectorPrimary.RemoveFromSelection(o); }
            }
            // If this object was NOT already in selection
            else {
                if (!ctrlDown) { selectorPrimary.ClearSelection(); }
                selectorPrimary.AddToSelection(o);
                focus = o;
            }
            selectorPrimary.GetSnapshot();
            selecting = true;
        }
        // Clicked nothing (that we care about)
        else {
            if (!ctrlDown) {
                selectorPrimary.ClearSelection();
            }
            selectorPrimary.Activate();
            selecting = false;
        }
    }
    private void AltSelectObject(GameObject o) {
        if (o != null) {
            if (selectorSecondary.selected.Contains(o)) {
                if (ctrlDown) { selectorSecondary.RemoveFromSelection(o); }
            }
            // If this object was NOT already in selection
            else {
                if (!ctrlDown) { selectorSecondary.ClearSelection(); }
                selectorSecondary.AddToSelection(o);
                focus = o;
            }
            selectorSecondary.GetSnapshot();
            selecting = true;
        }
        // Clicked nothing (that we care about)
        else {
            if (!ctrlDown) {
                selectorSecondary.ClearSelection();
            }
            selectorSecondary.Activate();
            selecting = false;
        }
    }

    public void SelectAllAgents(bool isAlt=false) {
        if (isAlt) {
            selectorSecondary.ClearSelection();
            AltSelectObject(null);
            foreach (Agent agent in agents) {
                selectorSecondary.AddToSelection(agent.gameObject);
            }
        } else {
            selectorPrimary.ClearSelection();
            SelectObject(null);
            foreach (Agent agent in agents) {
                selectorPrimary.AddToSelection(agent.gameObject);
            }
        }
    }
    public void SelectAllEdges(bool isAlt=false) {
        if (isAlt) {
            selectorSecondary.ClearSelection();
            AltSelectObject(null);
            foreach (Edge edge in edges) {
                selectorSecondary.AddToSelection(edge.gameObject);
            }
        } else {
            selectorPrimary.ClearSelection();
            SelectObject(null);
            foreach (Edge edge in edges) {
                selectorPrimary.AddToSelection(edge.gameObject);
            }
        }
    }

    public void RefreshView() {
        foreach (Edge edge in edges) {
            edge.Refresh();
            // Update graph weights based on edge costs
            graph.UpdateWeight(edge.a1, edge.a2, edge.cost);
            graph.UpdateWeight(edge.a2, edge.a1, edge.cost);
        }
        UpdateAllocations();
        onRefreshView.Invoke();
    }

    public void RemoveSelected() {
        // if (focus != null) {
        //     Agent agent = focus.GetComponent<Agent>();
        //     if (agent != null) { RemoveAgent(agent); }
        // }
        List<Agent> ags = new List<Agent>();
        List<Edge> egs = new List<Edge>();
        foreach (GameObject go in selectorPrimary.selected) {
            Agent agent = go.GetComponent<Agent>();
            if (agent != null) {
                ags.Add(agent);
                // RemoveAgent(agent);
            }
            Edge edge = go.GetComponent<Edge>();
            if (edge != null) {
                egs.Add(edge);
                // RemoveEdge(edge);
            }
        }
        foreach (Edge e in egs) { RemoveEdge(e); }
        foreach (Agent a in ags) { RemoveAgent(a); }
        ClearSelections();
    }

    public void ConnectAllSelected() {
        if (selectorPrimary.selected.Count > 1) {
            for (int i = 0; i < selectorPrimary.selected.Count; i++) {
                for (int j = i+1; j < selectorPrimary.selected.Count; j++) {
                    Agent a1 = selectorPrimary.selected[i].GetComponent<Agent>();
                    Agent a2 = selectorPrimary.selected[j].GetComponent<Agent>();
                    AddEdge(a1, a2);
                }
            }
        }
    }

    public void ConnectBipartite() {
        if (selectorPrimary.selected.Count > 0 && selectorSecondary.selected.Count > 0) {
            for (int i = 0; i < selectorPrimary.selected.Count; i++) {
                for (int j = 0; j < selectorSecondary.selected.Count; j++) {
                    Agent a1 = selectorPrimary.selected[i].GetComponent<Agent>();
                    Agent a2 = selectorSecondary.selected[j].GetComponent<Agent>();
                    AddEdge(a1, a2);
                }
            }
        }
    }

    public void SmartAction() {
        if (selectorPrimary.selected.Count > 0 && selectorSecondary.selected.Count > 0) {
            ConnectBipartite();
        }
        else if (selectorPrimary.selected.Count > 1) {
            ConnectAllSelected();
        }
        else {
            AddAgentAtMouse();
            Debug.Log(graph);
        }
    }

    // START AGENT ********************************************************************
    public void AddAgent() {
        GameObject agentObject = Instantiate(agentPrefab);
        Agent newAgent = agentObject.GetComponent<Agent>();
        if (newAgent != null) {
            graph.AddNode(newAgent);
            agents.Add(newAgent);
        }

        edgeMatrix = ResizeEdgeMatrix(edgeMatrix);
        pEdgeMatrix = ResizeEdgeMatrix(pEdgeMatrix);
        RefreshView();
    }
    public void AddAgentAtMouse() {
        GameObject agentObject = Instantiate(agentPrefab);
        Agent newAgent = agentObject.GetComponent<Agent>();
        if (newAgent != null) {
            graph.AddNode(newAgent);
            agents.Add(newAgent);
        }
        Vector3 mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        mousePos = new Vector3(mousePos.x, mousePos.y, agentObject.transform.position.z);
        agentObject.transform.position = mousePos;

        edgeMatrix = ResizeEdgeMatrix(edgeMatrix);
        pEdgeMatrix = ResizeEdgeMatrix(pEdgeMatrix);
        RefreshView();
    }
    public void RemoveAgent(Agent agent) {
        graph.RemoveNode(agent);
        // Destroy all connected edges
        List<Edge> toRemove = new List<Edge>();
        foreach (Edge edge in edges) {
            if (edge.n1 == agent.gameObject || edge.n2 == agent.gameObject) {
                toRemove.Add(edge);
            }
        }
        foreach (Edge e in toRemove) {
            edges.Remove(e);
            Destroy(e.gameObject);
        }
        // Destroy agent
        agents.Remove(agent);
        edgeMatrix = ResizeEdgeMatrix(edgeMatrix);
        pEdgeMatrix = ResizeEdgeMatrix(pEdgeMatrix);
        Destroy(agent.gameObject);
        ApplyNetworkRules();
    }
    // END AGENT **********************************************************************

    // START EDGES ********************************************************************
    public Edge EdgeAt(Agent a1, Agent a2) {
        // Debug.Log("Edge At: " + agents.IndexOf(a1) + ", " + agents.IndexOf(a2));
        if (undirected) {
            if ((edgeMatrix[agents.IndexOf(a1), agents.IndexOf(a2)]) == null) {
                return (edgeMatrix[agents.IndexOf(a2), agents.IndexOf(a1)]);
            }
        }
        return edgeMatrix[agents.IndexOf(a1), agents.IndexOf(a2)];
    }
    public bool AddEdge(Agent a1, Agent a2) {
        bool edge_created = false;
        if (a1 == null || a2 == null) { edge_created = false; return edge_created; }
        if (a1 != a2 && !graph.AreConnected(a1, a2, undirected)) {
            graph.AddEdge(a1, a2, 1, undirected);
            GameObject edgeObject = Instantiate(edgePrefab, edgePrefab.transform.position, Quaternion.identity);
            Edge edge = edgeObject.GetComponent<Edge>();
            edge.Connect(a1.transform, a2.transform);
            edges.Add(edge);
            a1.AddEdge(edge);
            a2.AddEdge(edge);
            // Debug.Log("Adding Edge At: " + agents.IndexOf(a1) + ", " + agents.IndexOf(a2));
            edgeMatrix[agents.IndexOf(a1), agents.IndexOf(a2)] = edge;
            edge_created = true;
        }
        ApplyNetworkRules();
        return edge_created;
    }
    public void RemoveEdge(Edge edge) {
        Agent a1 = edge.n1.GetComponent<Agent>();
        Agent a2 = edge.n2.GetComponent<Agent>();
        a1.RemoveEdge(edge);
        a2.RemoveEdge(edge);
        graph.RemoveEdge(a1, a2);
        edges.Remove(edge);
        edgeMatrix[agents.IndexOf(a1), agents.IndexOf(a2)] = null;
        Destroy(edge.gameObject);
        ApplyNetworkRules();
    }
    public void ClearEdges() {
        for (int i = 0; i < edgeMatrix.GetLength(0); i++) {
            for (int j = 0; j < edgeMatrix.GetLength(1); j++) {
                Edge edge = edgeMatrix[i,j];
                if (edge != null) {
                    Agent a1 = edge.n1.GetComponent<Agent>();
                    Agent a2 = edge.n2.GetComponent<Agent>();
                    a1.RemoveEdge(edge);
                    a2.RemoveEdge(edge);
                    graph.RemoveEdge(a1, a2);
                    edges.Remove(edge);
                    edgeMatrix[i, j] = null;
                    Destroy(edge.gameObject);
                }
            }
        }
    }
    // END EDGES **********************************************************************

    // START PROPOSED EDGES ********************************************************************
    public Edge PEdgeAt(Agent a1, Agent a2) {
        return pEdgeMatrix[agents.IndexOf(a1), agents.IndexOf(a2)];
    }
    public void AddPEdge(Agent a1, Agent a2) {
        if (a1 == null || a2 == null) { return; }
        // if (a1 != a2 && !graph.AreConnected(a1, a2, undirected)) {
        if (a1 != a2) {
            // graph.AddEdge(a1, a2);
            GameObject edgeObject = Instantiate(proposedEdgePrefab, proposedEdgePrefab.transform.position, Quaternion.identity);
            Edge edge = edgeObject.GetComponent<Edge>();
            edge.Connect(a1.transform, a2.transform);
            // edges.Add(edge);
            // a1.AddEdge(edge);
            // a2.AddEdge(edge);
            pEdgeMatrix[agents.IndexOf(a1), agents.IndexOf(a2)] = edge;
        }
        // ApplyNetworkRules();
    }
    public void RemovePEdge(Edge edge) {
        if (edge == null) { return; }
        Agent a1 = edge.n1.GetComponent<Agent>();
        Agent a2 = edge.n2.GetComponent<Agent>();
        // a1.RemoveEdge(edge);
        // a2.RemoveEdge(edge);
        // graph.RemoveEdge(a1, a2);
        // edges.Remove(edge);
        pEdgeMatrix[agents.IndexOf(a1), agents.IndexOf(a2)] = null;
        Destroy(edge.gameObject);
        // ApplyNetworkRules();
    }
    public void ClearPEdges() {
        for (int i = 0; i < pEdgeMatrix.GetLength(0); i++) {
            for (int j = 0; j < pEdgeMatrix.GetLength(1); j++) {
                if (pEdgeMatrix[i,j] != null) {
                    Destroy(pEdgeMatrix[i,j].gameObject);
                    pEdgeMatrix[i,j] = null;
                }
            }
        }
    }
    // END PROPOSED EDGES **********************************************************************

    public void ApplyNetworkRules() {
        foreach (Edge edge in edges) {
            edge.weight = rules.edgeInfo.weight;
            edge.distWeightScale = rules.edgeInfo.scaleWithDistance;
        }
        RefreshView();
    }

    public bool IsPairwiseStable() {
        bool stable = true;
        List<AdjGraph<Agent>> posAdjGraphs = GetPositiveAdjacentGraphs();
        List<AdjGraph<Agent>> negAdjGraphs = GetNegativeAdjacentGraphs();
        List<float> currAllocs = CalculateAllocation();
        // Check that removing an edge does not decrease any agent's allocation
        foreach (AdjGraph<Agent> negadj in negAdjGraphs) {
            List<float> newAllocs = CalculateAllocation(negadj);
            // Only pairwise stable if u_i(g) >= u_i(g - ij) for all i and ij
            // Otherwise, not PS
            if (currAllocs[negadj.i] < newAllocs[negadj.i]) { return false; }
        }

        // Check that. when adding an edge, if my utility (u_i) goes up, then my partner's
        // utility (u_j) goes down (I cannot gain without the person I'm connecting to losing)
        foreach (AdjGraph<Agent> posadj in posAdjGraphs) {
            List<float> newAllocs = CalculateAllocation(posadj);
            // If I gain, then you must lose, or we are not PS
            if (newAllocs[posadj.i] > currAllocs[posadj.i]) {
                if (newAllocs[posadj.j] >= currAllocs[posadj.j]) {
                    return false;
                }
            }
        }

        return true;
    }

    public List<AdjGraph<Agent>> GetPositiveAdjacentGraphs() {
        List<AdjGraph<Agent>> adjGraphs = new List<AdjGraph<Agent>>();
        for (int i = 0; i < agents.Count; i++) {
            for (int j = undirected ? i : 0; j < agents.Count; j++) {
                // Only calculate adj graph if two agents are not connected (we get new version in which they are connected)
                if (i != j && !graph.AreConnected(agents[i], agents[j], undirected)) {
                    adjGraphs.Add(GetAdjacentGraph(agents[i], agents[j]));
                }
            }
        }
        return adjGraphs;
    }

    public List<AdjGraph<Agent>> GetNegativeAdjacentGraphs() {
        List<AdjGraph<Agent>> adjGraphs = new List<AdjGraph<Agent>>();
        for (int i = 0; i < agents.Count; i++) {
            for (int j = undirected ? i : 0; j < agents.Count; j++) {
                // Only calculate adj graph if two agents are connected (we get new version in which they are NOT connected)
                if (i != j && graph.AreConnected(agents[i], agents[j], undirected)) {
                    adjGraphs.Add(GetAdjacentGraph(agents[i], agents[j]));
                }
            }
        }
        return adjGraphs;
    }

    public List<AdjGraph<Agent>> GetAllAdjacentGraphs() {
        List<AdjGraph<Agent>> adjGraphs = new List<AdjGraph<Agent>>();
        for (int i = 0; i < agents.Count; i++) {
            for (int j = undirected ? i : 0; j < agents.Count; j++) {
                if (i != j) {
                    adjGraphs.Add(GetAdjacentGraph(agents[i], agents[j]));
                }
            }
        }
        return adjGraphs;
    }

    // Returns a Graph<Agent> with the edge toggled
    // If edge was 0, then return graph copy with edge with weight calculated
    // If edge was !0, then return graph copy with edge with weight 0
    public AdjGraph<Agent> GetAdjacentGraph(Agent a1, Agent a2) {
        // Reset oracle graph to orig graph if oracle not provided
        AdjGraph<Agent> oracle = new AdjGraph<Agent>(agents);
        oracle.CopyFrom(graph);
        // 
        int idx1 = oracle.GetIdx(a1);
        int idx2 = oracle.GetIdx(a2);
        oracle.SetAdjacentEdge(idx1, idx2);
        if (oracle.AreConnected(idx1, idx2)) {
            oracle.RemoveEdge(idx1, idx2, undirected);
        } else {
            oracle.AddEdge(
                idx1,
                idx2,
                Edge.PredictCost(
                    a1.transform.position,
                    a2.transform.position,
                    rules.edgeInfo.weight,
                    rules.edgeInfo.scaleWithDistance
                ),
                undirected
            );
        }
        return oracle;
    }
}
