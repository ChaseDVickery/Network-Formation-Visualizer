using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class GameInfoDisplay : MonoBehaviour
{
    public GameSwitcher gameSwitcher;

    public TMP_Text timestepText;

    public void UpdateInfo() {
        timestepText.text = "Step: " + gameSwitcher.currentGame.time.ToString();
    }
}
