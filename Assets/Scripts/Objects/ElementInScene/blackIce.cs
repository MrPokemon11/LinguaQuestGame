using UnityEngine;

public class blackIce : MonoBehaviour
{
    private float blackIceEdgeSize;
    private float slippingTime;

    [Header("Settings")]
    public float speedBoostAmount = 0.5f; // How much faster to make the player if they hit ice while sliding

    private void Start()
    {
        // Calculate edge size based on transform scale
        blackIceEdgeSize = Mathf.Max(transform.localScale.x, transform.localScale.y) / 2f;
        // Calculate slipping time proportional to ice size
        slippingTime = blackIceEdgeSize * 1f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerExploring player = other.GetComponent<PlayerExploring>();

            if (player != null)
            {
                if (player.currentState != PlayerState.slip)
                {
                    // Case 1: Player walks onto ice. Start Slipping.
                    // Uses the cleaned-up Pushed method (limits checked inside if needed, though usually new slips are fine)
                    player.pushed(player.GetCurrentMovementDirection(), slippingTime, true);
                }
                else
                {
                    // Case 2: Player is already slipping.
                    // Boost the slip, but PlayerExploring script will clamp it to the new Max limits.
                    player.BoostSlip(slippingTime, speedBoostAmount);
                }
            }
        }
    }
}