using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class StatsDisplay : MonoBehaviour
{
    public AgentGraph agentGraph;

    public TMP_Text statText;

    public void UpdateStats() {
        string s = "";
        s += "Value: " + agentGraph.value.ToString("n2");
        statText.text = s;
    }
}
