using UnityEngine;

public class PetMovement : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 1f;
    public float followDistance = 4f;

    [Header("Orbit Offset")]
    public Vector3 behindHeadOffsetRight = new Vector3(-1f, 1f, 0); // if facing right
    public Vector3 behindHeadOffsetLeft = new Vector3(1f, 1f, 0);   // if facing left

    [Header("Wander Settings")]
    public float wanderRadius = 1.5f;
    public float wanderCooldownMin = 2f;
    public float wanderCooldownMax = 4f;

    [Header("State Transition Smoothing")]
    public float stateSwitchDelay = 0.5f; // seconds to wait before switching state
    public float stopThreshold = 1f;    // how close to target counts as "arrived"

    private Vector3 orbitCenter;
    private Vector3 wanderTarget;
    private float wanderTimer = 0f;
    private float nextWanderTime = 0f;

    public Animator animator;
    private Vector3 lastMoveDir;

    private enum PetState { Wandering, Chasing, Idle }
    private PetState currentState = PetState.Idle;
    private PetState targetState = PetState.Wandering;
    private float stateTimer = 0f;

    void Start()
    {
        PickNewWanderTarget();
    }

    void Update()
    {
        if (player == null) return;

        // Orbit center depends on which way player is facing
        Vector3 offset = (player.localScale.x > 0) ? behindHeadOffsetRight : behindHeadOffsetLeft;
        orbitCenter = player.position + offset;

        float dist = Vector3.Distance(transform.position, orbitCenter);

        float maxTeleportDistance = followDistance * 5f;
        if (dist > maxTeleportDistance)
        {
            transform.position = orbitCenter;
            animator.SetBool("isMoving", false);
            currentState = PetState.Idle;
            stateTimer = 0f;
            return;
        }

        // Decide state
        if (dist > followDistance)
            targetState = PetState.Chasing;
        else if (dist <= stopThreshold)
            targetState = PetState.Idle;
        else
            targetState = PetState.Wandering;

        // Smooth state transition
        if (targetState != currentState)
        {
            stateTimer += Time.deltaTime;
            if (stateTimer >= stateSwitchDelay)
            {
                currentState = targetState;
                stateTimer = 0f;
            }
        }
        else stateTimer = 0f;

        // Execute behavior
        switch (currentState)
        {
            case PetState.Chasing:
                MoveTowards(orbitCenter);
                break;

            case PetState.Wandering:
                WanderAround();
                break;

            case PetState.Idle:
                IdleFacingPlayer();
                break;
        }
    }

    void WanderAround()
    {
        wanderTimer += Time.deltaTime;
        if (wanderTimer >= nextWanderTime)
        {
            PickNewWanderTarget();
        }
        MoveTowards(wanderTarget);
    }

    void PickNewWanderTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        wanderTarget = orbitCenter + new Vector3(randomCircle.x, randomCircle.y, 0);
        wanderTimer = 0f;
        nextWanderTime = Random.Range(wanderCooldownMin, wanderCooldownMax);
    }

    void MoveTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, target);

        if (distance > stopThreshold)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            UpdateAnimator(direction);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }

    void IdleFacingPlayer()
    {
        Vector3 toPlayer = (player.position - transform.position);
        if (Mathf.Abs(toPlayer.x) > Mathf.Abs(toPlayer.y))
        {
            // Horizontal facing
            lastMoveDir = (toPlayer.x > 0) ? Vector3.right : Vector3.left;
        }
        else
        {
            // Vertical facing
            lastMoveDir = (toPlayer.y > 0) ? Vector3.up : Vector3.down;
        }

        animator.SetBool("isMoving", false);
        animator.SetFloat("moveX", lastMoveDir.x);
        animator.SetFloat("moveY", lastMoveDir.y);
    }

    void UpdateAnimator(Vector3 moveDir)
    {
        if (moveDir.magnitude > 0.01f)
        {
            animator.SetBool("isMoving", true);
            animator.SetFloat("moveX", moveDir.x);
            animator.SetFloat("moveY", moveDir.y);
            lastMoveDir = moveDir;
        }
    }

    public void Appear() => animator.SetTrigger("Appear");
    public void Disappear() => animator.SetTrigger("Disappear");
}
