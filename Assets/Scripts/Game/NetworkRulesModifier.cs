using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkRulesModifier : MonoBehaviour
{
    public NetworkRules rules;

    // Edge Cost information
    public Slider weightSlider;
    public Toggle weightDistToggle;

    void Start() {
        // if (weightSlider != null) { weightSlider.onValueChanged.AddListener(UpdateRules); }
        // if (weightDistToggle != null) { weightDistToggle.onValueChanged.AddListener(UpdateRules); }
        SyncToRules();
    }

    private void SyncToRules() {
        weightSlider.value = rules.edgeInfo.weight;
        weightDistToggle.isOn =rules.edgeInfo.scaleWithDistance;
    }

    public void UpdateRules() {
        rules.edgeInfo.weight = weightSlider.value;
        rules.edgeInfo.scaleWithDistance = weightDistToggle.isOn;

        rules.ApplyChanges();
    }
}