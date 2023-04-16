using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameRulesModifier : MonoBehaviour
{
    public NetworkFormationGame game;

    public List<Slider> sliders;
    public List<Toggle> toggles;

    void Start() {
        Initialize();
    }

    public void Initialize() {
        for (int i = 0; i < transform.childCount; i++) {
            GameObject go = transform.GetChild(i).gameObject;
            Slider slider = go.GetComponent<Slider>();
            if (slider != null) {
                slider.onValueChanged.Invoke(slider.value);
                slider.onValueChanged.AddListener(delegate{UpdateRules();});
                sliders.Add(slider);
            }
            Toggle toggle = go.GetComponent<Toggle>();
            if (toggle != null) {
                toggle.onValueChanged.Invoke(toggle.isOn);
                toggle.onValueChanged.AddListener(delegate{UpdateRules();});
                toggles.Add(toggle);
            }
        }
    }

    public void UpdateRules() {
        if (game != null) {
            foreach (Slider slider in sliders) {
                game.UpdateInfo(slider.name, slider.value);
            }
            foreach (Toggle toggle in toggles) {
                game.UpdateInfo(toggle.name, toggle.isOn);
            }
        }
    }
}