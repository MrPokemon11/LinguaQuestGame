using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static bool quizCompleted = false;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject); // keep this alive across scenes
    }
}