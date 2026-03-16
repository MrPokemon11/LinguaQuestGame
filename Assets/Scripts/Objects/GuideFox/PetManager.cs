using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PetManager : MonoBehaviour
{
    public PetBubble petBubble;

    [System.Serializable]
    public class PetTrigger
    {
        [Tooltip("Optional stable key. If empty, a key is built from signal+message names.")]
        public string triggerKey;
        public Signal triggerSignal;
        public MessageSequence messageSequence;
    }

    public List<PetTrigger> petTriggers = new();

    private PetSignalManager petSignalManager;

    private void Awake()
    {
        if (petBubble == null)
        {
            Debug.LogWarning($"{nameof(PetManager)} on {name} has no {nameof(petBubble)} reference.", this);
            return;
        }

        petSignalManager = GameManager.Instance != null ? GameManager.Instance.PetSignalManager : null;

        foreach (var trigger in petTriggers)
        {
            if (trigger.triggerSignal == null || trigger.messageSequence == null)
            {
                continue;
            }

            string key = BuildTriggerKey(trigger);
            if (petSignalManager != null && petSignalManager.IsConsumed(key))
            {
                continue;
            }

            SignalListener listener = gameObject.AddComponent<SignalListener>();
            listener.enabled = false;
            listener.signal = trigger.triggerSignal;

            SignalListener capturedListener = listener;
            UnityEngine.Events.UnityAction action = null;
            action = () =>
            {
                if (petSignalManager != null && !petSignalManager.Consume(key))
                {
                    capturedListener.response.RemoveListener(action);
                    Destroy(capturedListener);
                    return;
                }

                petBubble.ShowMessagesToPlayer(trigger.messageSequence.messages);
                capturedListener.response.RemoveListener(action);
                Destroy(capturedListener);
            };

            listener.response.AddListener(action);
            listener.enabled = true;
        }
    }

    private static string BuildTriggerKey(PetTrigger trigger)
    {
        if (!string.IsNullOrWhiteSpace(trigger.triggerKey))
        {
            return trigger.triggerKey;
        }

        string signalPart = trigger.triggerSignal != null ? trigger.triggerSignal.name : "null_signal";
        string sequencePart = trigger.messageSequence != null ? trigger.messageSequence.name : "null_sequence";
        return $"{signalPart}:{sequencePart}";
    }
}
