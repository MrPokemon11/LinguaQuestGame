using UnityEngine;

public class PrairieNPCTracker : MonoBehaviour
{
    public static PrairieNPCTracker Instance;

    public int totalNPCs = 6;
    private bool[] visited;
    private int visitedCount = 0;

    void Awake()
    {
        Instance = this;
        visited = new bool[totalNPCs];
    }

    public void MarkVisited(int id)
    {
        if (visited[id]) return;

        visited[id] = true;
        visitedCount++;

        PrairieNPCUI.Instance.UpdateCounter(visitedCount, totalNPCs);
    }

    public bool AllVisited()
    {
        return visitedCount >= totalNPCs;
    }
}