using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class NumberDisplay : MonoBehaviour
{
    private TMP_Text numberText;

    void Awake() {
        numberText = GetComponent<TMP_Text>();
    }

    public void UpdateNumber(float number) {
        string s = "";
        s += number.ToString("n3");
        numberText.text = s;
    }
}
