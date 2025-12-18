using UnityEngine;

public class blackIce : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerExploring player = other.GetComponent<PlayerExploring>();
            if (player != null)
            {
                player.currentState = PlayerState.slip;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerExploring player = other.GetComponent<PlayerExploring>();
            if (player != null)
            {
                player.currentState = PlayerState.walk;
            }
        }
    }
}
