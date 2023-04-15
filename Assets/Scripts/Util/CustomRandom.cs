using System.Collections.Generic;
using UnityEngine;

static class RandomQueue {
    private static Stack<float> values = new Stack<float>();
    private static Stack<float> used = new Stack<float>();

    private static Stack<int> valuesI = new Stack<int>();
    private static Stack<int> usedI = new Stack<int>();

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

    public static void Clear() {
        values.Clear();
        used.Clear();
    }

    public static int GetInt(int minI, int maxE) {
        int randVal;
        // Get value from stack or make new value
        if (valuesI.Count > 0 && valuesI.Peek() >= minI && valuesI.Peek() < maxE) { randVal = valuesI.Pop(); }
        else { randVal = Random.Range(minI, maxE); }
        usedI.Push(randVal);
        return randVal;
    }

    public static void UndoInt() {
        if (usedI.Count > 0) {
            valuesI.Push(usedI.Pop());
        }
    }

    public static void ClearInt() {
        valuesI.Clear();
        usedI.Clear();
    }
}