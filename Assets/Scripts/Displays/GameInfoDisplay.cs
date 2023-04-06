using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class GameInfoDisplay : MonoBehaviour
{
    public NetworkFormationGame game;

    public TMP_Text timestepText;

    public void UpdateInfo() {
        timestepText.text = "Step: " + game.time.ToString();
    }
}
