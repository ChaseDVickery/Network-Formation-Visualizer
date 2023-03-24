using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

[RequireComponent(typeof(Agent))]
public class AgentDisplay : MonoBehaviour
{
    private Agent agent;
    public TMP_Text allocationText;

    void Awake() {
        agent = GetComponent<Agent>();
    }

    void Start() {
        UpdateDisplay();
    }

    public void UpdateDisplay() {
        allocationText.text = agent.allocation.ToString("n2");
    }
}