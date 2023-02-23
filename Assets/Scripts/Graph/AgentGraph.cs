using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Interactable;

public class AgentGraph : MonoBehaviour
{
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

    public GameObject focus;
    public bool selecting = false;
    public List<IInteractable> selected;
    private bool dragging = false;
    private Vector3 startDragPos, startDragMousePos;
    private DragType dragType;

    private bool ctrlDown;

    void Awake() {
        selectorPrimary = selectionBoxPrimary.GetComponent<Selector>();
        selectorSecondary = selectionBoxSecondary.GetComponent<Selector>();
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
                newAgentObj.transform.position = new Vector3(Random.Range(-8.0f, 8.0f), Random.Range(-5.0f, 5.0f), 0);
                agents.Add(newAgentObj.GetComponent<Agent>());
            }
        }
        graph = new Graph<Agent>(agents);
        // for (int i = 0; i < 30; i++) {
        //     AddConnection();
        // }
        Debug.Log(graph);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("r")) {
            RemoveSelectedAgents();
        }
        if (Input.GetKeyDown("a")) {
            AddConnectionsBetweenSelected();
        }
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) { ctrlDown = true; }
        if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl)) { ctrlDown = false; }

        if (Input.GetMouseButtonDown(0)) {
            RaycastHit2D rayHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            rayHit = Physics2D.GetRayIntersection(ray);
            if ( rayHit.collider != null ) {
                SelectObject(rayHit.transform.gameObject);
            } 
            else {
                SelectObject(null);
                // UnfocusCurrent();
                // if (!ctrlDown) {
                //     // Selection box
                //     selectorPrimary.ClearSelection();
                // }
                // selectorPrimary.Activate();
            }
            // Get focus object info and set dragging
            // if (focus != null) {
            //     startDragPos = focus.transform.position;
            // }
            dragType = DragType.LEFT;
            startDragMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButtonDown(1)) {
            dragType = DragType.RIGHT;
        }

        if (Input.GetMouseButtonUp(0)) {
            dragType = DragType.NONE;
            selectorPrimary.GetSnapshot();
            selectorPrimary.Deactivate();
            selectorPrimary.transform.position = new Vector3(-100, -100, 0);
        }

        if (dragType == DragType.LEFT) {
            Vector3 mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            // Move focus
            // if (focus != null) {
            //     focus.transform.position = startDragPos + (mousePos - startDragMousePos);
            if (selecting) {
                for (int i = 0; i < selectorPrimary.selected.Count; i++) {
                    selectorPrimary.selected[i].transform.position = selectorPrimary.positionSnapshot[i] + (mousePos - startDragMousePos);
                }
                RefreshView();
            }
            // Selection box
            else {
                Vector3 diff = (mousePos - startDragMousePos);
                Vector3 boxPos = startDragMousePos + (diff/2);
                boxPos.z = 0;
                selectionBoxPrimary.transform.position = boxPos;
                selectionBoxPrimary.transform.localScale = new Vector3(diff.x, diff.y, 1);
            }
        }
    }

    private void ClearSelections() {
        // UnfocusCurrent();
        selectorPrimary.ClearSelection();
    }
    private void SelectObject(GameObject o) {
        // // UnfocusCurrent();
        // Agent agent = o.GetComponent<Agent>();
        // if (agent != null) {
        //     agent.Select();
        //     // focus = agent.gameObject;
        // }
        if (o != null) {
            if (selectorPrimary.selected.Contains(o)) {
                if (ctrlDown) { selectorPrimary.RemoveFromSelection(o); }
            }
            // If this object was NOT already in selection
            else {
                if (!ctrlDown) { ClearSelections(); }
                selectorPrimary.AddToSelection(o);
                focus = o;
            }
            selectorPrimary.GetSnapshot();
            selecting = true;
        }
        // Clicked nothing (that we care about)
        else {
            if (!ctrlDown) {
                // Selection box
                selectorPrimary.ClearSelection();
            }
            selectorPrimary.Activate();
            selecting = false;
        }
    }
    // private void UnfocusCurrent() {
    //     if (focus != null) {
    //         Agent agent = focus.GetComponent<Agent>();
    //         if (agent != null) {
    //             agent.Deselect();
    //         }

    //         if (ctrlDown) {
    //             selectorPrimary.AddToSelection(focus);
    //             selectorPrimary.GetSnapshot();
    //         }
    //         focus = null;
    //     }
    // }

    public void RefreshView() {
        foreach (Edge edge in edges) {
            edge.Refresh();
        }
    }

    public void AddAgent() {
        GameObject agentObject = Instantiate(agentPrefab);
        Agent newAgent = agentObject.GetComponent<Agent>();
        if (newAgent != null) {
            agents.Add(newAgent);
            graph.AddNode(newAgent);
        }
    }

    public void RemoveSelectedAgents() {
        // if (focus != null) {
        //     Agent agent = focus.GetComponent<Agent>();
        //     if (agent != null) { RemoveAgent(agent); }
        // }
        foreach (GameObject go in selectorPrimary.selected) {
            Agent agent = go.GetComponent<Agent>();
            if (agent != null) {
                RemoveAgent(agent);
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
    }

    public void AddConnectionsBetweenSelected() {
        if (selectorPrimary.selected.Count > 1) {
            for (int i = 0; i < selectorPrimary.selected.Count; i++) {
                for (int j = i+1; j < selectorPrimary.selected.Count; j++) {
                    Agent a1 = selectorPrimary.selected[i].GetComponent<Agent>();
                    Agent a2 = selectorPrimary.selected[j].GetComponent<Agent>();
                    AddConnection(a1, a2);
                }
            }
        }
    }

    public void AddConnection(Agent a1, Agent a2) {
        if (a1 == null || a2 == null) { return; }
        if (a1 != a2 && !graph.AreConnected(a1, a2)) {
            graph.AddConnection(a1, a2);
            GameObject edgeObject = Instantiate(edgePrefab);
            Edge edge = edgeObject.GetComponent<Edge>();
            edge.Connect(a1.transform, a2.transform);
            edges.Add(edge);
        }
    }
}
