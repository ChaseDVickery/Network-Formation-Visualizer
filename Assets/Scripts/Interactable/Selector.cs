using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Selector : MonoBehaviour
{

    public List<GameObject> selected;
    public List<Vector3> positionSnapshot;

    public bool active = false;

    void Awake() {
        
    }

    public void Activate() {
        active = true;
    }
    public void Deactivate() {
        active = false;
    }

    public void ClearSelection() {
        foreach (GameObject sel in selected) {
            Agent a = sel.GetComponent<Agent>();
            if (a != null) {
                a.Deselect();
            }
        }
        selected.Clear();
    }

    public void AddToSelection(GameObject obj) {
        selected.Add(obj);
        Agent a = obj.GetComponent<Agent>();
        if (a != null) {
            a.Select();
        }
        Debug.Log("Adding object to selection " + obj);
    }

    public void RemoveFromSelection(GameObject obj) {
        selected.Remove(obj);
        Agent a = obj.GetComponent<Agent>();
        if (a != null) {
            a.Deselect();
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
            AddToSelection(col.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if (active && col != null) {
            RemoveFromSelection(col.gameObject);
        }
    }
}
