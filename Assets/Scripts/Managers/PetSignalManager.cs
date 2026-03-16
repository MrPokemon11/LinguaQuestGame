using System.Collections.Generic;
using UnityEngine;

public class PetSignalManager : MonoBehaviour
{
    private readonly HashSet<string> consumedTriggerKeys = new();

    public bool IsConsumed(string triggerKey)
    {
        return !string.IsNullOrEmpty(triggerKey) && consumedTriggerKeys.Contains(triggerKey);
    }

    public bool Consume(string triggerKey)
    {
        if (string.IsNullOrEmpty(triggerKey)) return false;
        return consumedTriggerKeys.Add(triggerKey);
    }

    public void ClearAllConsumed()
    {
        consumedTriggerKeys.Clear();
    }
}
