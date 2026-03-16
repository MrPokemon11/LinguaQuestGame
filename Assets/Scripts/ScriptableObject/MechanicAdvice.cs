using UnityEngine;

[CreateAssetMenu(menuName = "LinguaQuest/MechanicAdvice")]
public class MechanicAdvice : ScriptableObject
{
    public string minigameID; // Matches the JSON loader ID
    public BoolValue isUnlocked; // Link to your existing BoolValue system
    [TextArea(3, 10)]
    public string[] adviceLines;
    public Sprite mechanicIllustration; // Optional: visual aid for the player
}
