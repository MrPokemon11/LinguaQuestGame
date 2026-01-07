using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuroraHareAI : MonoBehaviour
{
    private enum HareState { Wander, Cinematic, RunRoute, Distracted, Escaping, Tamed }

    [Header("State")]
    [SerializeField] private HareState currentState = HareState.Wander;

    [Header("Route Settings")]
    [Tooltip("Drag empty GameObjects here to define the path the rabbit runs.")]
    public Transform[] raceWaypoints;
    private int currentWaypointIndex = 0;
    [Tooltip("How close to a waypoint before moving to the next one.")]
    public float waypointTolerance = 0.5f;

    [Header("Movement Stats")]
    public float wanderSpeed = 2f;
    public float runSpeed = 7.0f;
    public float wanderRadius = 3f;

    [Header("Cinematic Trigger")]
    public float triggerRadius = 5f;
    public float cinematicDuration = 1.5f;
    private bool hasTriggeredRace = false;

    [Header("Bait Logic")]
    public float foodDetectRadius = 8f;
    public string[] baitTags = { "Carrot" };
    private GameObject targetFood;

    [Header("Rewards")]
    public GameObject eternalEmberPrefab;
    private bool hasDroppedEmber = false;

    [Header("Components")]
    private Rigidbody2D rb;
    private Animator animator;
    private CameraMovement mainCamera;
    private Transform playerTransform;

    // Wander internals
    private float lastWanderTime;
    private Vector2 wanderTarget;
    public float wanderInterval = 2f;
    public LayerMask obstacleLayerMask;

    // --- FIX: Store the spawn point to prevent drifting ---
    private Vector2 initialPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Cache references
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        mainCamera = FindFirstObjectByType<CameraMovement>();

        // --- FIX: Capture the spawn point as the anchor for wandering ---
        initialPosition = transform.position;
        wanderTarget = initialPosition;
    }

    void Update()
    {
        // 1. GLOBAL PRIORITY: Bait
        if (currentState != HareState.Tamed && currentState != HareState.Escaping && currentState != HareState.Cinematic)
        {
            if (DetectBait())
            {
                currentState = HareState.Distracted;
            }
        }

        // 2. STATE MACHINE
        switch (currentState)
        {
            case HareState.Wander:
                HandleWander();
                CheckForRaceTrigger();
                break;

            case HareState.Cinematic:
                rb.linearVelocity = Vector2.zero; // Note: updated to linearVelocity for newer Unity versions
                animator.SetBool("isMoving", false);
                break;

            case HareState.RunRoute:
                HandleRunRoute();
                break;

            case HareState.Distracted:
                HandleApproachBait();
                break;

            case HareState.Escaping:
                break;

            case HareState.Tamed:
                HandleWander();
                break;
        }
    }

    // --- LOGIC: WANDER ---
    void HandleWander()
    {
        // Pick new spot periodically
        if (Time.time > lastWanderTime + wanderInterval || Vector2.Distance(transform.position, wanderTarget) < 0.2f)
        {
            // --- FIX: Calculate random point relative to INITIAL position, not current position ---
            Vector2 randomPoint = initialPosition + (Random.insideUnitCircle * wanderRadius);

            // Basic obstacle check
            if (!Physics2D.OverlapCircle(randomPoint, 0.3f, obstacleLayerMask))
            {
                wanderTarget = randomPoint;
            }
            lastWanderTime = Time.time;
        }
        MoveTo(wanderTarget, wanderSpeed);
    }

    // --- LOGIC: CINEMATIC START ---
    void CheckForRaceTrigger()
    {
        if (hasTriggeredRace || playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist < triggerRadius)
        {
            StartCoroutine(StartCinematicRace());
        }
    }

    IEnumerator StartCinematicRace()
    {
        hasTriggeredRace = true;
        currentState = HareState.Cinematic;

        Debug.Log("Rabbit detected Player. Starting Cinematic.");

        if (mainCamera != null)
        {
            mainCamera.PayAttentionTo(this.gameObject, cinematicDuration);
        }

        yield return new WaitForSeconds(cinematicDuration);

        currentState = HareState.RunRoute;
        currentWaypointIndex = 0;
    }

    // --- LOGIC: RUN ROUTE ---
    void HandleRunRoute()
    {
        if (raceWaypoints == null || raceWaypoints.Length == 0) return;

        Transform targetPoint = raceWaypoints[currentWaypointIndex];
        float dist = Vector2.Distance(transform.position, targetPoint.position);

        MoveTo(targetPoint.position, runSpeed);

        if (dist < waypointTolerance)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= raceWaypoints.Length)
            {
                EscapeFailedHunt();
            }
        }
    }

    void EscapeFailedHunt()
    {
        Debug.Log("Rabbit reached end of route. Escaped!");
        currentState = HareState.Escaping;
        Destroy(gameObject);
    }

    // --- LOGIC: BAIT ---
    bool DetectBait()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, foodDetectRadius);
        float closestDist = Mathf.Infinity;
        GameObject closestFood = null;

        foreach (var hit in hits)
        {
            foreach (string tag in baitTags)
            {
                if (hit.CompareTag(tag))
                {
                    float d = Vector2.Distance(transform.position, hit.transform.position);
                    if (d < closestDist)
                    {
                        closestDist = d;
                        closestFood = hit.gameObject;
                    }
                }
            }
        }
        targetFood = closestFood;
        return targetFood != null;
    }

    void HandleApproachBait()
    {
        if (targetFood == null)
        {
            if (hasTriggeredRace) currentState = HareState.RunRoute;
            else currentState = HareState.Wander;
            return;
        }
        MoveTo(targetFood.transform.position, wanderSpeed);
    }

    // --- MOVEMENT CORE ---
    void MoveTo(Vector2 pos, float speed)
    {
        Vector2 dir = (pos - (Vector2)transform.position).normalized;
        rb.MovePosition(rb.position + dir * speed * Time.deltaTime);

        if (animator != null)
        {
            animator.SetBool("isMoving", true);
            animator.SetFloat("moveX", dir.x);
            animator.SetFloat("moveY", dir.y);
        }
    }

    // --- COLLISION (THE CATCH) ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CatchRabbit();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CatchRabbit();
        }

        if (currentState == HareState.Distracted && other.gameObject == targetFood)
        {
            Destroy(other.gameObject);
            currentState = HareState.Tamed;
        }
    }

    void CatchRabbit()
    {
        if (currentState == HareState.Escaping || currentState == HareState.Tamed) return;

        Debug.Log("Rabbit Caught!");

        if (!hasDroppedEmber && eternalEmberPrefab != null)
        {
            Instantiate(eternalEmberPrefab, transform.position, Quaternion.identity);
            hasDroppedEmber = true;
        }

        currentState = HareState.Tamed;
        rb.linearVelocity = Vector2.zero;
        if (animator) animator.SetBool("isMoving", false);
    }

    // --- VISUAL DEBUGGING ---
    private void OnDrawGizmosSelected()
    {
        // Draw the wander region in the editor
        Gizmos.color = Color.green;
        // Use Application.isPlaying to switch between Editor position and Runtime anchor
        Vector3 center = Application.isPlaying ? (Vector3)initialPosition : transform.position;
        Gizmos.DrawWireSphere(center, wanderRadius);

        // Draw the trigger radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}