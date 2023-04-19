using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class GameInfoDisplay : MonoBehaviour
{
    public GameSwitcher gameSwitcher;

    public TMP_Text timestepText;
    public TMP_Text gameNameText;

    public void UpdateInfo() {
        if (gameSwitcher.currentGame != null) {
            if (timestepText != null) timestepText.text = "Step: " + gameSwitcher.currentGame.time.ToString();
            if (gameNameText != null) gameNameText.text = "" + gameSwitcher.currentGame.gameName;
        }
    }
}
