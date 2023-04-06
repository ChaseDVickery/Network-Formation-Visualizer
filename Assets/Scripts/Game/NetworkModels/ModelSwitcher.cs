using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelSwitcher : MonoBehaviour
{

    public AgentGraph agentGraph;

    public NetworkRulesModifier rulesInterface;

    public List<NetworkRules> networkRuleChoices;
    public NetworkRules currentRules;

    // Start is called before the first frame update
    void Start()
    {
        ChangeModel(0);
    }

    public void ChangeModel(int idx) {
        currentRules = networkRuleChoices[idx];
        // Set interface for rules options
        rulesInterface.rules = currentRules;
        rulesInterface.SyncToRules();
        // Change & refresh graph to reflect new rules
        agentGraph.SetNetworkRules(currentRules);
        agentGraph.RefreshView();
    }
}
