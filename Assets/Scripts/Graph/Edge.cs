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

    void Awake() {
        poss = new Vector3[2];
        lren = GetComponent<LineRenderer>();
    }

    public void Connect(Transform t1, Transform t2) {
        c1 = t1;
        c2 = t2;
        n1 = t1.gameObject;
        n2 = t2.gameObject;
        Refresh();
    }
    public void Refresh() {
        poss[0] = c1.position;
        poss[1] = c2.position;
        midpt = (poss[0] + poss[1])/2;
        lren.SetPositions(poss);

        distText.transform.position = Camera.main.WorldToScreenPoint(midpt);
        string distStr = (poss[0] - poss[1]).magnitude.ToString("n2");
        distText.text = distStr;
    }
}
