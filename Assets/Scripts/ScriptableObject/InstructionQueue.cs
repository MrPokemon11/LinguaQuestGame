using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "LinguaQuest/InstructionQueue")]
public class InstructionQueue : ScriptableObject
{
    // List of IDs that the player needs to hear instructions for
    public List<string> pendingIDs = new List<string>();

    public void AddToQueue(string minigameID)
    {
        if (!pendingIDs.Contains(minigameID))
        {
            pendingIDs.Add(minigameID);
        }
    }

    public string GetNextID()
    {
        return pendingIDs.Count > 0 ? pendingIDs[0] : null;
    }

    public void MarkCurrentComplete()
    {
        if (pendingIDs.Count > 0) pendingIDs.RemoveAt(0);
    }
}