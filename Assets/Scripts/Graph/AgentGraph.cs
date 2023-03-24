using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

using Interactable;

public class AgentGraph : MonoBehaviour
{
    public NetworkRules rules;

    public int numAgents;
    public GameObject agentPrefab;
    public GameObject edgePrefab;

    public GameObject selectionBoxPrimary;
    private Selector selectorPrimary;
    public GameObject selectionBoxSecondary;
    private Selector selectorSecondary;

    public List<Agent> agents;
    public List<Edge> edges;
    public Graph<Agent> graph;

    public float value { get; private set; }
    public List<float> allocations { get; private set; }

    public GameObject focus;
    public bool selecting = false;
    public List<IInteractable> selected;
    private bool dragging = false;
    private Vector3 startDragMousePosPrimary;
    private Vector3 startDragMousePosSecondary;
    private DragType dragType;

    private bool ctrlDown;

    public UnityEvent onRefreshView;

    void Awake() {
        selectorPrimary = selectionBoxPrimary.GetComponent<Selector>();
        selectorSecondary = selectionBoxSecondary.GetComponent<Selector>();
        selectorSecondary.isAlt = true;

        rules.onChange.AddListener(ApplyNetworkRules);
    }

    // Start is called before the first frame update
    void Start()
    {
        // if (agents.Count == 0) {
        //     agents.Add(Instantiate(agentPrefab).GetComponent<Agent>());
        // }
        if (numAgents > 0) {
            agents.Clear();
            for (int i = 0; i < numAgents; i++) {
                GameObject newAgentObj = Instantiate(agentPrefab);
                // newAgentObj.transform.position = new Vector3(Random.Range(-8.0f, 8.0f), Random.Range(-5.0f, 5.0f), 0);
                newAgentObj.transform.position = new Vector3(3*Mathf.Cos(2*Mathf.PI*i/(numAgents)), 3*Mathf.Sin(2*Mathf.PI*i/(numAgents)), 0);
                agents.Add(newAgentObj.GetComponent<Agent>());
            }
        }
        graph = new Graph<Agent>(agents);
        // for (int i = 0; i < 30; i++) {
        //     AddEdge();
        // }
        Debug.Log(graph);
    }

    // Update is called once per frame
    void Update()
    {

        // if (Input.GetKeyDown("g")) {
        //     CalculateAllocation();
        // }

        if (Input.GetKeyDown("r")) {
            // RemoveSelectedAgents();
            RemoveSelected();
            CalculateAllocation();
        }
        if (Input.GetKeyDown("a")) {
            ConnectAllSelected();
            CalculateAllocation();
        }
        if (Input.GetKeyDown("s")) {
            ConnectBipartite();
            CalculateAllocation();
        }
        if (Input.GetKeyDown("space")) {
            SmartConnect();
            CalculateAllocation();
        }
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) { ctrlDown = true; }
        if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl)) { ctrlDown = false; }

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

    public void CalculateAllocation() {
        // Calculate the Value of the graph as well as the per-agent allocations
        value = rules.GetNetworkValue(graph);
        allocations = rules.GetAllocation(graph, value);
        for (int i = 0; i < agents.Count; i++) {
            agents[i].UpdateInfo("allocation", allocations[i]);
        }
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

    public void RefreshView() {
        foreach (Edge edge in edges) {
            edge.Refresh();
            // Update graph weights based on edge costs
            graph.UpdateWeight(edge.a1, edge.a2, edge.cost);
            graph.UpdateWeight(edge.a2, edge.a1, edge.cost);
        }
        CalculateAllocation();
        onRefreshView.Invoke();
    }

    public void AddAgent() {
        GameObject agentObject = Instantiate(agentPrefab);
        Agent newAgent = agentObject.GetComponent<Agent>();
        if (newAgent != null) {
            agents.Add(newAgent);
            graph.AddNode(newAgent);
        }
    }

    public void RemoveSelected() {
        // if (focus != null) {
        //     Agent agent = focus.GetComponent<Agent>();
        //     if (agent != null) { RemoveAgent(agent); }
        // }
        foreach (GameObject go in selectorPrimary.selected) {
            Agent agent = go.GetComponent<Agent>();
            if (agent != null) {
                RemoveAgent(agent);
            }
            Edge edge = go.GetComponent<Edge>();
            if (edge != null) {
                RemoveEdge(edge);
            }
        }
        ClearSelections();
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
        Destroy(agent.gameObject);
        ApplyNetworkRules();
    }
    public void RemoveEdge(Edge edge) {
        Agent a1 = edge.n1.GetComponent<Agent>();
        Agent a2 = edge.n2.GetComponent<Agent>();
        a1.RemoveEdge(edge);
        a2.RemoveEdge(edge);
        graph.RemoveEdge(a1, a2);
        edges.Remove(edge);
        Destroy(edge.gameObject);
        ApplyNetworkRules();
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

    public void SmartConnect() {
        if (selectorPrimary.selected.Count > 0 && selectorSecondary.selected.Count > 0) {
            ConnectBipartite();
        }
        else if (selectorPrimary.selected.Count > 1) {
            ConnectAllSelected();
        }
    }

    public void AddEdge(Agent a1, Agent a2) {
        if (a1 == null || a2 == null) { return; }
        if (a1 != a2 && !graph.AreConnected(a1, a2)) {
            graph.AddEdge(a1, a2);
            GameObject edgeObject = Instantiate(edgePrefab, edgePrefab.transform.position, Quaternion.identity);
            Edge edge = edgeObject.GetComponent<Edge>();
            edge.Connect(a1.transform, a2.transform);
            edges.Add(edge);
            a1.AddEdge(edge);
            a2.AddEdge(edge);
        }
        ApplyNetworkRules();
    }

    public void ApplyNetworkRules() {
        foreach (Edge edge in edges) {
            edge.weight = rules.edgeInfo.weight;
            edge.distWeightScale = rules.edgeInfo.scaleWithDistance;
        }
        RefreshView();
    }
}
