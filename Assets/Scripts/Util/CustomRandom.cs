using System.Collections.Generic;
using UnityEngine;

static class RandomQueue {
    private static Stack<float> values = new Stack<float>();
    private static Stack<float> used = new Stack<float>();

    public static float value {
        get => GetValue();
    }

    public static float GetValue() {
        float randVal;
        // Get value from stack or make new value
        if (values.Count > 0) { randVal = values.Pop(); }
        else { randVal = Random.value; }
        used.Push(randVal);
        return randVal;
    }

    public static void Undo() {
        if (used.Count > 0) {
            values.Push(used.Pop());
        }
    }
}