using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkFormationGame : MonoBehaviour
{

    // Represents an atomic "step" of the formation game
    // Ideally the smallest unit of decision that occurs during the game
    public virtual void Step() {
        return;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public struct CostInfo {
    public float cost;
    public bool scaleWithDistance;
}