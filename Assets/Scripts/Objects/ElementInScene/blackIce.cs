using Unity.Mathematics;
using UnityEngine;

public class blackIce : MonoBehaviour
{
    private float blackIceEdgeSize;
    private float slippingTime;

    private void Start()
    {
        // Calculate edge size based on transform scale
        blackIceEdgeSize = Mathf.Max(transform.localScale.x, transform.localScale.y) / 2f;
        // Calculate slipping time proportional to ice size
        slippingTime = blackIceEdgeSize * 1.5f; // Scale factor for slipping duration
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerExploring player = other.GetComponent<PlayerExploring>();
            if (player != null)
            {
                player.pushed(player.GetCurrentMovementDirection(), slippingTime, true);
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
                player.changeState(PlayerState.walk);
            }
        }
    }
}
