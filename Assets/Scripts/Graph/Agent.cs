using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Reflection;

using Interactable;

[RequireComponent(typeof(AgentDisplay))]
public class Agent : MonoBehaviour, IInteractable
{
    public string agentName;
    private AgentDisplay agentDisplay;

    private bool isHovered = false;

    SpriteRenderer spriteRenderer;

    public float allocation;
    public List<Edge> edges = new List<Edge>();

    public override string ToString() {
        return agentName;
    }

    void Awake() {
        agentDisplay = GetComponent<AgentDisplay>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void AddEdge(Edge e) {
        edges.Add(e);
    }
    public void RemoveEdge(Edge e) {
        edges.Remove(e);
    }

    public void UpdateInfo(string key, System.Object value) {
        FieldInfo updateField = typeof(Agent).GetField(key);
        updateField.SetValue(this, value);
        agentDisplay.UpdateDisplay();
    }

    void OnMouseEnter() { Hover(); }
    void OnMouseExit() { Unhover(); }

    public void Hover() {
        isHovered = true;
    }
    public void Unhover() {
        isHovered = false;
    }

    public void Select() {
        spriteRenderer.material.SetFloat("_Selected", 1f);
    }
    public void Deselect() {
        spriteRenderer.material.SetFloat("_Selected", 0f);
    }
    public void AltSelect() {
        spriteRenderer.material.SetFloat("_AltSelected", 1f);
    }
    public void AltDeselect() {
        spriteRenderer.material.SetFloat("_AltSelected", 0f);
    }
}
