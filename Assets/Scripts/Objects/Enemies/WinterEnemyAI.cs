using System.Collections;
using UnityEngine;

public class WinterEnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float chaseRadius = 5f;
    public float attackRadius = 0.6f;

    [Header("Territory Settings")]
    [Tooltip("The distance from the spawn point the enemy is allowed to travel.")]
    public float boundaryRadius = 7f;
    private Vector3 homePosition; // Remembers where it spawned

    [Header("Inchworm Rhythm")]
    public float moveDuration = 0.5f;
    public float pauseDuration = 1.0f;

    [Header("Targeting")]
    public string targetTag = "Player";
    private Transform target;
    private Rigidbody2D myRigidbody;
    private Animator animator;
    private bool isMovingPhase = false;

    [Header("State Effects")]
    public bool causesTripping = true;
    public float tripDuration = 1f;

    [Header("Cooldown Settings")]
    [Tooltip("How long the enemy stops moving after successfully tripping the player.")]
    public float impactCooldown = 10f;
    private bool isInCooldown = false;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // 1. Record the starting position as "Home"
        homePosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag(targetTag);
        if (playerObj != null)
        {
            target = playerObj.transform;
        }

        StartCoroutine(MoveCycleCo());
    }

    void FixedUpdate()
    {
        // 1. PRIORITY CHECK: Are we recovering from an attack?
        if (isInCooldown)
        {
            animator.SetBool("moving", false);
            return; // STOP all movement logic
        }

        // 2. Normal Movement Logic
        // Only attempt to move during the "Stretch" phase of the inchworm animation
        if (isMovingPhase)
        {
            CheckDistanceAndMove();
        }
        else
        {
            animator.SetBool("moving", false);
        }
    }

    private void CheckDistanceAndMove()
    {
        // If no target, just try to go home
        if (target == null)
        {
            ReturnHome();
            return;
        }

        float distToPlayer = Vector3.Distance(target.position, transform.position);
        float distPlayerToHome = Vector3.Distance(target.position, homePosition);
        float distSelfToHome = Vector3.Distance(transform.position, homePosition);

        // LOGIC GATE:
        // 1. Is Player close enough to see? (Chase Radius)
        // 2. Is Player inside my Territory? (Boundary Radius)
        bool shouldChase = (distToPlayer <= chaseRadius) && (distPlayerToHome <= boundaryRadius);

        if (shouldChase)
        {
            // Stop if we are practically touching the player (Attack Radius)
            if (distToPlayer > attackRadius)
            {
                MoveTowards(target.position);
            }
            else
            {
                // We are close enough to attack/trip, so stop moving
                animator.SetBool("moving", false);
            }
        }
        else
        {
            // If we aren't chasing, check if we need to return home.
            if (distSelfToHome > 0.2f)
            {
                ReturnHome();
            }
            else
            {
                // We are home and idle.
                animator.SetBool("moving", false);
            }
        }
    }

    private void ReturnHome()
    {
        MoveTowards(homePosition);
    }

    private void MoveTowards(Vector3 destination)
    {
        // Move Rigidbody
        Vector3 temp = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.fixedDeltaTime);
        myRigidbody.MovePosition(temp);

        // Update Animation
        ChangeAnim(temp - transform.position);
    }

    private IEnumerator MoveCycleCo()
    {
        while (true)
        {
            isMovingPhase = true;
            yield return new WaitForSeconds(moveDuration);

            isMovingPhase = false;
            yield return new WaitForSeconds(pauseDuration);
        }
    }

    private void ChangeAnim(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > 0 || Mathf.Abs(direction.y) > 0)
        {
            animator.SetFloat("moveX", direction.x);
            animator.SetFloat("moveY", direction.y);
            animator.SetBool("moving", true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Don't trigger if we are already in cooldown
        if (isInCooldown) return;

        if (collision.gameObject.CompareTag("Player") && causesTripping)
        {
            PlayerExploring player = collision.gameObject.GetComponent<PlayerExploring>();

            // Check if player is valid and NOT currently falling
            // This ensures we only stop moving if we actually caused a NEW fall
            if (player != null && player.currentState != PlayerState.falling)
            {
                player.TripPlayer(tripDuration);
                StartCoroutine(ImpactCooldownCo());
            }
        }
    }

    // --- NEW: Cooldown Routine ---
    private IEnumerator ImpactCooldownCo()
    {
        isInCooldown = true;

        // Optional: If you have a specific "Laugh" or "Idle" trigger, play it here
        // animator.SetTrigger("laugh"); 

        yield return new WaitForSeconds(impactCooldown);

        isInCooldown = false;
    }

    // --- VISUAL DEBUGGING ---
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRadius); // Vision range

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Application.isPlaying ? homePosition : transform.position, boundaryRadius); // Territory
    }
}