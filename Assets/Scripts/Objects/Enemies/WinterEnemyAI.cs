using System.Collections;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

public class WinterEnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float chaseRadius = 5f;
    public float attackRadius = 0.6f;

    [Header("Inchworm Rhythm")]
    // How long the enemy moves forward (the "Stretch" frames)
    public float moveDuration = 0.5f;
    // How long the enemy waits (the "Bunch/Idle" frames)
    public float pauseDuration = 1.0f;

    [Header("Targeting")]
    public string targetTag = "Player";
    private Transform target;
    private Rigidbody2D myRigidbody;
    private Animator animator;
    private bool isMovingPhase = false;

    [Header("State Effects")]
    // If true, touching this enemy makes the player slip instead of taking damage immediately
    public bool causesSlipping = true;
    public float slipDuration = 1.5f;
    public float pushed_speed = 1f;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Find the player automatically using the tag
        GameObject playerObj = GameObject.FindGameObjectWithTag(targetTag);
        if (playerObj != null)
        {
            target = playerObj.transform;
        }

        // Start the inchworm cycle
        StartCoroutine(MoveCycleCo());
    }

    void FixedUpdate()
    {
        CheckDistanceAndMove();
    }

    private void CheckDistanceAndMove()
    {
        if (target == null) return;

        float distance = Vector3.Distance(target.position, transform.position);

        // Only move if:
        // 1. Within Chase Radius
        // 2. Outside Attack Radius (stop if touching)
        // 3. Currently in the "Moving Phase" of the rhythm
        if (distance <= chaseRadius && distance > attackRadius && isMovingPhase)
        {
            Vector3 temp = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.fixedDeltaTime);
            myRigidbody.MovePosition(temp);
            ChangeAnim(temp - transform.position);
        }
        else
        {
            // Stop movement if in Pause phase or out of range
            animator.SetBool("moving", false);
        }
    }

    // This Coroutine creates the "Inchworm" effect: Squirm -> Stop -> Squirm
    private IEnumerator MoveCycleCo()
    {
        while (true)
        {
            // Phase 1: Move (Stretch)
            isMovingPhase = true;
            yield return new WaitForSeconds(moveDuration);

            // Phase 2: Pause (Gather/Idle)
            isMovingPhase = false;
            // Stop logic is handled in FixedUpdate via the bool flag
            yield return new WaitForSeconds(pauseDuration);
        }
    }

    private void ChangeAnim(Vector2 direction)
    {
        // Only update animation if we are actually moving significantly
        if (Mathf.Abs(direction.x) > 0 || Mathf.Abs(direction.y) > 0)
        {
            animator.SetFloat("moveX", direction.x);
            animator.SetFloat("moveY", direction.y);
            animator.SetBool("moving", true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("WinterEnemyAI: Collided with " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Player") && causesSlipping)
        {
            Debug.Log("WinterEnemyAI: Collided with player, attempting to inflict slip");
            PlayerExploring player = collision.gameObject.GetComponent<PlayerExploring>();
            if (player != null && player.currentState != PlayerState.slip)
            {
                StartCoroutine(InflictSlip(player));
            }
        }
    }

    private IEnumerator InflictSlip(PlayerExploring player)
    {
        // Reference to existing state enum
        Debug.Log("WinterEnemyAI: Inflicting slip on player");
        player.pushed((player.transform.position - transform.position).normalized * pushed_speed, slipDuration);

        // Optional: Knockback could go here

        yield return new WaitForSeconds(slipDuration);

        // Return to walk state if they are still slipping
        if (player.currentState == PlayerState.slip)
        {
            player.currentState = PlayerState.walk;
        }
    }
}