using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;

public class NetworkRulesModifier : MonoBehaviour
{
    public NetworkRules rules;

    // Edge Cost information
    public Slider weightSlider;
    public Toggle weightDistToggle;

    public List<Slider> sliders;
    public List<Toggle> toggles;

    void Start() {
        for (int i = 0; i < transform.childCount; i++) {
            GameObject go = transform.GetChild(i).gameObject;
            Slider slider = go.GetComponent<Slider>();
            if (slider != null) {
                // slider.onValueChanged.Invoke(slider.value);
                slider.onValueChanged.AddListener(delegate{UpdateRules();});
                sliders.Add(slider);
            }
            Toggle toggle = go.GetComponent<Toggle>();
            if (toggle != null) {
                // toggle.onValueChanged.Invoke(toggle.isOn);
                toggle.onValueChanged.AddListener(delegate{UpdateRules();});
                toggles.Add(toggle);
            }
        }
        SyncToRules();
    }

    public void SyncToRules() {
        // weightSlider.value = rules.edgeInfo.weight;
        // weightDistToggle.isOn =rules.edgeInfo.scaleWithDistance;
        if (rules != null) {
            // Debug.Log("Before Syncing to Rules " + rules.GetType() +". scaleWithDistance is " + rules.edgeInfo.scaleWithDistance);
            // Debug.Log("Syncing to Rules " + rules.GetType());
            foreach (Slider slider in sliders) {
                // Debug.Log("Before " + slider.name +" sync. scaleWithDistance is " + rules.edgeInfo.scaleWithDistance);
                FieldInfo updateField = rules.GetType().GetField(slider.name);
                if (updateField != null) { slider.SetValueWithoutNotify((float)updateField.GetValue(rules)); }
                // Debug.Log("After attempted base key " + slider.name +" sync. scaleWithDistance is " + rules.edgeInfo.scaleWithDistance);
                updateField = typeof(EdgeInfo).GetField(slider.name);
                // Debug.Log("After get edgeInfo field " + slider.name +". scaleWithDistance is " + rules.edgeInfo.scaleWithDistance);
                if (updateField != null) { slider.SetValueWithoutNotify((float)updateField.GetValue(rules.edgeInfo)); } 
                // Debug.Log("After " + slider.name +" sync. scaleWithDistance is " + rules.edgeInfo.scaleWithDistance);
            }
            // Debug.Log("After slider syncs " + rules.GetType() +". scaleWithDistance is " + rules.edgeInfo.scaleWithDistance);
            foreach (Toggle toggle in toggles) {
                FieldInfo updateField = rules.GetType().GetField(toggle.name);
                if (updateField != null) { toggle.SetIsOnWithoutNotify((bool)updateField.GetValue(rules)); }
                updateField = typeof(EdgeInfo).GetField(toggle.name);
                if (updateField != null) { toggle.SetIsOnWithoutNotify((bool)updateField.GetValue(rules.edgeInfo)); }
            }
            rules.ApplyChanges();
        }
        foreach (Slider slider in sliders) {
            slider.onValueChanged.Invoke(slider.value);
        }
        foreach (Toggle toggle in toggles) {
            toggle.onValueChanged.Invoke(toggle.isOn);
        }
    }

    // public void UpdateRules() {
    //     rules.edgeInfo.weight = weightSlider.value;
    //     rules.edgeInfo.scaleWithDistance = weightDistToggle.isOn;

    //     rules.ApplyChanges();
    // }

    public void UpdateRules() {
        if (rules != null) {
            foreach (Slider slider in sliders) {
                rules.UpdateInfo(slider.name, slider.value, rules.GetType());
            }
            foreach (Toggle toggle in toggles) {
                rules.UpdateInfo(toggle.name, toggle.isOn, rules.GetType());
            }
            rules.ApplyChanges();
        }
    }
}