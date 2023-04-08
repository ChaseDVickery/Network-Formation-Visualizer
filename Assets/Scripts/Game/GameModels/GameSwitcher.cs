using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSwitcher : MonoBehaviour
{

    public AgentGraph agentGraph;

    // public NetworkRulesModifier rulesInterface;

    public List<NetworkFormationGame> gameChoices;
    public NetworkFormationGame currentGame;

    // Start is called before the first frame update
    void Start()
    {
        ChangeModel(0);
    }

    public void ChangeModel(int idx) {
        currentGame = gameChoices[idx];
        // Change Current Game
        currentGame.LinkGraph(agentGraph);
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
