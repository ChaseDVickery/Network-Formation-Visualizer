using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Selector : MonoBehaviour
{

    public List<GameObject> selectCache;
    public List<GameObject> selected;
    public List<Vector3> positionSnapshot;
    public bool isAlt;

    public bool active = false;

    void Awake() {
        selectCache = new List<GameObject>();
    }

    public void Activate() {
        active = true;
        selectCache.Clear();
    }
    public void Deactivate() {
        active = false;
        selectCache.Clear();
    }

    public void ClearSelection() {
        foreach (GameObject sel in selected) {
            Agent a = sel.GetComponent<Agent>();
            if (a != null) {
                if (isAlt) { a.AltDeselect(); }
                else { a.Deselect(); }
            }
            Edge e = sel.GetComponent<Edge>();
            if (e != null) {
                if (isAlt) { e.AltDeselect(); }
                else { e.Deselect(); }
            }
        }
        selected.Clear();
    }

    public void AddToSelection(GameObject obj) {
        if (obj == gameObject) { return; }
        selected.Add(obj);
        Agent a = obj.GetComponent<Agent>();
        if (a != null) {
            if (isAlt) { a.AltSelect(); }
            else { a.Select(); }
        }
        Edge e = obj.GetComponent<Edge>();
        if (e != null) {
            if (isAlt) { e.AltSelect(); }
            else { e.Select(); }
        }
        Debug.Log("Adding object to selection " + obj);
    }

    public void RemoveFromSelection(GameObject obj) {
        selected.Remove(obj);
        Agent a = obj.GetComponent<Agent>();
        if (a != null) {
            if (isAlt) { a.AltDeselect(); }
            else { a.Deselect(); }
        }
        Edge e = obj.GetComponent<Edge>();
        if (e != null) {
            if (isAlt) { e.AltDeselect(); }
            else { e.Deselect(); }
        }
        Debug.Log("Removing object from selection " + obj);
    }

    public void GetSnapshot() {
        positionSnapshot.Clear();
        foreach (GameObject sel in selected) {
            positionSnapshot.Add(sel.transform.position);
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (active) {
            if (selected.Contains(col.gameObject)) { selectCache.Add(col.gameObject); RemoveFromSelection(col.gameObject); }
            else { AddToSelection(col.gameObject); }
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if (active && col != null) {
            if (selectCache.Contains(col.gameObject)) { selectCache.Remove(col.gameObject); AddToSelection(col.gameObject); }
            else { RemoveFromSelection(col.gameObject); }
        }
    }
}
