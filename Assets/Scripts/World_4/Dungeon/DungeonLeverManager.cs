using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonLeverManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (GameState.quizCompleted)
        {
            LeverController[] levers = FindObjectsOfType<LeverController>();
            foreach (var lever in levers)
            {
                lever.PullLever();
            }
        }
    }
}
