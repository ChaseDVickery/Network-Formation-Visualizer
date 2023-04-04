using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class StabilityDisplay : MonoBehaviour
{
    public AgentGraph agentGraph;

    public TMP_Text stabilityText;

    public void UpdateInfo() {
        string s = "";
        bool ps = agentGraph.IsPairwiseStable();
        s += "Pairwise Stable: " + ps.ToString();
        stabilityText.text = s;
    }
}
