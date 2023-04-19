using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSwitcher : MonoBehaviour
{

    public AgentGraph agentGraph;

    // public NetworkRulesModifier rulesInterface;

    public List<NetworkFormationGame> gameChoices;
    public NetworkFormationGame currentGame;

    public List<GameRulesModifier> modifierInterfaces;
    public GameRulesModifier currentModifier;

    // Start is called before the first frame update
    void Start()
    {
        ChangeModel(0);
    }

    public void ChangeModel(int idx) {
        if (currentGame != null) {
            Stop();
        }

        currentGame = gameChoices[idx];
        currentModifier = modifierInterfaces[idx];
        // Change Current Game
        currentGame.LinkGraph(agentGraph);
        currentGame.LinkInterface(currentModifier);

        // StopRestoreGraph();
    }

    void Update() {
        if (currentGame != null && currentGame.inGame) { 
            if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                currentGame.Pause();
                currentGame.Undo();
            }
            if (Input.GetKeyDown(KeyCode.RightArrow)) {
                currentGame.Pause();
                currentGame.Step();
            }
        }
    }

    public void ShowCurrentModifierInterface() {
        currentModifier.gameObject.SetActive(true);
    }
    public void HideCurrentModifierInterface() {
        currentModifier.gameObject.SetActive(false);
    }

    public void Run() {
        currentGame.Run();
    }
    public void Reverse() {
        currentGame.Reverse();
    }
    public void Stop() {
        currentGame.Stop();
    }
    public void StopRestoreGraph() {
        currentGame.StopRestoreGraph();
    }
    public void Pause() {
        currentGame.Pause();
    }
    public void Step() {
        currentGame.Step();
    }
    public void Undo() {
        currentGame.Undo();
    }
    public void Reset() {
        currentGame.Reset();
    }
}
