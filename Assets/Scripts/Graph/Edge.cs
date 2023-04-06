using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(LineRenderer))]
public class Edge : MonoBehaviour
{

    private Transform c1;
    private Transform c2;
    private Vector3[] poss;
    private Vector3 midpt;
    public LineRenderer lren;

    public TMP_Text distText;

    public GameObject n1 {get; private set;}
    public GameObject n2 {get; private set;}
    public Agent a1 {get; private set;}
    public Agent a2 {get; private set;}

    public PolygonCollider2D myCollider;
    private float colliderWidth;

    private Color origColorStart, origColorEnd;
    public Color selectionColor;
    public Color altSelectionColor;

    public float weight = 1;
    public bool distWeightScale = false;
    private float _cost;
    public float cost {
        get => GetCost();
        set => _cost = value;
    }

    void Awake() {
        poss = new Vector3[2];
        lren = GetComponent<LineRenderer>();
        myCollider = GetComponent<PolygonCollider2D>();
        origColorStart = lren.startColor;
        origColorEnd = lren.endColor;
        colliderWidth = lren.widthMultiplier;
    }

    public void Connect(Transform t1, Transform t2) {
        c1 = t1;
        c2 = t2;
        n1 = t1.gameObject;
        n2 = t2.gameObject;
        a1 = n1.GetComponent<Agent>();
        a2 = n2.GetComponent<Agent>();
        Refresh();
    }
    public void Refresh() {
        poss[0] = c1.position;
        poss[1] = c2.position;
        midpt = (poss[0] + poss[1])/2;
        lren.SetPositions(poss);

        distText.transform.position = Camera.main.WorldToScreenPoint(midpt);
        distText.text = GetCost().ToString("n2");

        Vector2[] pts = new Vector2[4];
        Vector2 diff = new Vector2((poss[1] - poss[0]).x, (poss[1] - poss[0]).y);
        Vector2 offsetDir = Vector2.Perpendicular(diff.normalized);
        pts[0] = (Vector2)poss[0] + (colliderWidth/2)*offsetDir;
        pts[1] = (Vector2)poss[1] + (colliderWidth/2)*offsetDir;
        offsetDir = Vector2.Perpendicular((-diff).normalized);
        pts[3] = (Vector2)poss[0] + (colliderWidth/2)*offsetDir;
        pts[2] = (Vector2)poss[1] + (colliderWidth/2)*offsetDir;
        myCollider.SetPath(0, pts);
    }

    public float GetCost() {
        float cost = 0f;
        if (distWeightScale) {
            float dist = (poss[0] - poss[1]).magnitude;
            return weight * dist;
        } else {
            return weight;
        }
    }

    public static float PredictCost(Vector3 pos1, Vector3 pos2, float weight, bool distWeightScale) {
        float cost = 0f;
        if (distWeightScale) {
            float dist = (pos1 - pos2).magnitude;
            return weight * dist;
        } else {
            return weight;
        }
    }

    public void Select() {
        lren.startColor = selectionColor;
        lren.endColor = selectionColor;
    }
    public void Deselect() {
        lren.startColor = origColorStart;
        lren.endColor = origColorEnd;
    }
    public void AltSelect() {
        lren.startColor = altSelectionColor;
        lren.endColor = altSelectionColor;
    }
    public void AltDeselect() {
        lren.startColor = origColorStart;
        lren.endColor = origColorEnd;
    }
}
