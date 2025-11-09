using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MagicianNPC : MonoBehaviour
{
    [Header("References")]
    public Animator animator;                 // Animator for appear, disappear, inject animations
    public PetBubble bubble;                  // Reuse PetBubble to show dialogues
    public Inventory playerInventory;         // Player's inventory (check items)
    public List<Item> requiredItems;          // Items needed to unlock wizard
    public Signal appearSignal;               // Signal to trigger his appearance
    public Signal magicUnlockedSignal;        // Signal to notify game that player unlocked magic
    public List<BoolValue> activateConditions;
    public GameObject player;

    [Header("Dialogues")]
    [TextArea]
    public List<string> introMessages = new List<string>()
    {
        "Greetings, traveler...",
        "I am the hidden magician of this land."
    };

    [TextArea]
    public List<string> needItemsMessages = new List<string>()
    {
        "You are not yet ready.",
        "Bring me the three sacred items, and I shall grant you magic."
    };

    [TextArea]
    public List<string> injectMessages = new List<string>()
    {
        "Ah, you have gathered them all!",
        "Now, feel the power of magic!"
    };

    [TextArea]
    public List<string> afterUnlockMessages = new List<string>()
    {
        "The magic is now within you.",
        "Use it wisely, hero...",
        "Farewell..."
    };

    private bool isVisible = false;
    private bool magicGiven = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player");
        //gameObject.SetActive(false); // Start invisible
        //appearSignal.OnRaise += OnAppearSignal;
    }


    public void OnAppearSignal()
    {
        // Check if all BoolValue in activateConditions are true
        foreach (var condition in activateConditions)
        {
            if (!condition.runtimeValue)
                return; // Do nothing if any condition is false
        }
        // All conditions are true, run AppearSequence
        if (!isVisible)
        {
            StartCoroutine(AppearSequence());
        }
    }

    private IEnumerator AppearSequence()
    {
        isVisible = true;
        gameObject.SetActive(true);

        animator.SetTrigger("Appear");
        yield return new WaitForSeconds(1.5f); // wait for appear animation

        // Start talking
        yield return bubble.ShowMessages(introMessages);

        if (HasAllRequiredItems())
        {
            // Inject magic
            yield return bubble.ShowMessages(injectMessages);

            animator.SetTrigger("InjectMagic");
            yield return new WaitForSeconds(2f); // wait for inject animation

            magicUnlockedSignal.Raise();
            player.GetComponent<PlayerExploring>().UpgradeMagicLevel(1);
            magicGiven = true;

            // Final dialogue
            yield return bubble.ShowMessages(afterUnlockMessages);

            // Disappear
            animator.SetTrigger("Disappear");
            yield return new WaitForSeconds(1.5f);
            gameObject.SetActive(false);
            isVisible = false;
        }
        else
        {
            // Not enough items
            yield return bubble.ShowMessages(needItemsMessages);
            // Stay visible and idle, waiting for player to come back
        }
    }

    private bool HasAllRequiredItems()
    {
        foreach (var item in requiredItems)
        {
            if (!playerInventory.HasItem(item))
                return false;
        }
        return true;
    }
}