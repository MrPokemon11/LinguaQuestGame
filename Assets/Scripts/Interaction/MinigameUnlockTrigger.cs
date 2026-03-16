using UnityEngine;

public class MinigameUnlockTrigger : MonoBehaviour
{
    [Header("Instruction Data")]
    public string minigameID; // Must match the ID in MechanicData.json
    public InstructionQueue queue;
    public Signal teachingSignal;
    public BoolValue hasBeenExplained; // To prevent spamming the wolf every time

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            Debug.Log($"Player entered trigger for {minigameID}. Has been explained: {hasBeenExplained?.runtimeValue}");
            // Only add to queue if the player hasn't heard this advice yet
            if (hasBeenExplained != null && !hasBeenExplained.runtimeValue)
            {
                TriggerWolfAdvice();
            }
        }
    }

    public void TriggerWolfAdvice()
    {
        Debug.Log($"Queueing advice for: {minigameID}");
        queue.AddToQueue(minigameID);
        teachingSignal.Raise();

        // Mark as explained so the signal doesn't fire again for this specific game
        if (hasBeenExplained != null)
        {
            hasBeenExplained.runtimeValue = true;
        }
    }
}