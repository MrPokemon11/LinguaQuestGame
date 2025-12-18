using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    [Header("Drag ALL your BoolValues here")]
    // By being in this list, these objects act as if they are "Global Variables"
    public List<BoolValue> persistentValues;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object alive forever

            // Only Reset logic runs ONCE when the game physically starts
            ResetValuesToInitial();
        }
        else
        {
            // If we reload the Main Menu, destroy the duplicate
            Destroy(gameObject);
        }
    }

    // Call this ONLY when starting a fresh "New Game"
    public void ResetValuesToInitial()
    {
        foreach (var val in persistentValues)
        {
            if (val != null) val.runtimeValue = val.initialValue;
        }
        Debug.Log("All Game States have been reset.");
    }
}