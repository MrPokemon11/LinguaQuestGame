using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    walk,
    attack,
    interact,
    stagger,
    slip
}

public class PlayerExploring : MonoBehaviour
{
    // --- Serialized Fields (visible in Inspector) ---
    [Header("Movement")]
    public float speed = 5f;
    public Rigidbody2D myRigidbody;
    public VectorValue StartingPosition;

    [Header("Animation")]
    private Animator animator;

    [Header("Player State")]
    public PlayerState currentState = PlayerState.walk;
    public UnityEngine.Vector3 change = UnityEngine.Vector3.zero;

    [Header("Health & Magic")]
    public FloatValue currentHealth;
    public FloatValue magicLevel;

    [Header("Inventory & Items")]
    public Inventory inventory;
    public SpriteRenderer receiveItemSprite;

    [Header("Signals")]
    public Signal playerHealthSignal;
    public Signal playerAttackSignal;

    [Header("Step Sound")]
    public StepSoundManager stepSoundManager;
    public float stepSoundCooldown = 0.5f;
    private float lastStepSoundTime = 0f;

    [Header("Magic Attacks")]
    public GameObject fireballPrefab;
    public Transform firePoint;
    public GameObject lightningEffectPrefab;
    public float lightningCastOffset = 4.5f;

    [Header("Combat & Slip Mechanics")]
    public float pushCooldown = 1.0f; // Time before player can be pushed again
    private float lastPushTime = -10f;

    // New variables for dynamic slipping
    private float currentSlipTimer = 0f;
    [Tooltip("How much player input affects slip time. Higher = easier to brake/coast.")]
    public float slipInputInfluence = 0.8f;

    [HideInInspector] public bool isMoving;
    [Header("Visual Effects")]
    public TrailRenderer[] slipTrails; // Change from single variable to Array
    public FootprintsFromPlayerExploring footprintSystem;

    // --- Unity Methods ---
    void Start()
    {
        animator = GetComponent<Animator>();
        stepSoundManager = FindFirstObjectByType<StepSoundManager>();
        myRigidbody = GetComponent<Rigidbody2D>();
        if (footprintSystem == null)
            footprintSystem = GetComponent<FootprintsFromPlayerExploring>();
        footprintSystem.enabled = true;
        foreach (var trail in slipTrails)
        {
            if (trail != null) trail.emitting = false;
        }
        animator.SetFloat("moveX", 0);
        animator.SetFloat("moveY", -1);
        myRigidbody.position = StartingPosition.runtimeValue;
        magicLevel.runtimeValue = magicLevel.initialValue;
    }

    void Update()
    {
        lastStepSoundTime += Time.deltaTime;

        // --- State Checks ---
        if (currentState == PlayerState.interact)
            return;

        if (currentState == PlayerState.slip)
        {
            UpdateAnimationSlip();
            return; // Lock standard movement logic
        }
        else
        {
            animator.SetBool("slipping", false);
        }

        // --- Standard Input Processing ---
        change = UnityEngine.Vector3.zero;
        change.x = Input.GetAxisRaw("Horizontal");
        change.y = Input.GetAxisRaw("Vertical");
        change.z = 0;
        change.Normalize();

        if (Input.GetMouseButtonDown(0) && currentState != PlayerState.attack)
        {
            StartCoroutine(AttackCo());
        }
        else if (Input.GetMouseButtonDown(1) && currentState == PlayerState.attack)
        {
            animator.SetTrigger("swordDance");
        }
        else if (Input.GetKeyDown(KeyCode.F) && magicLevel.runtimeValue >= 1)
        {
            CastFireball();
        }
        else if (Input.GetKeyDown(KeyCode.G) && magicLevel.runtimeValue >= 2)
        {
            CastLightning();
        }
        else if (currentState == PlayerState.walk)
        {
            UpdateAnimationAndMove();
        }
    }

    // --- Movement & Animation ---
    private void UpdateAnimationAndMove()
    {
        if (change != UnityEngine.Vector3.zero)
        {
            isMoving = true;
            animator.SetFloat("moveX", change.x);
            animator.SetFloat("moveY", change.y);
            animator.SetBool("moving", true);
            foreach (var trail in slipTrails)
            {
                if (trail != null) trail.emitting = false;
            }
            myRigidbody.MovePosition(myRigidbody.position + new UnityEngine.Vector2(change.x, change.y) * speed * Time.fixedDeltaTime);
            if (stepSoundManager != null && lastStepSoundTime >= stepSoundCooldown)
            {
                lastStepSoundTime = 0f;
                stepSoundManager.PlayStepSound(transform.position);
            }
        }
        else
        {
            isMoving = false;
            animator.SetBool("moving", false);
        }
    }

    private void UpdateAnimationSlip()
    {
        // 1. Decrease timer naturally
        currentSlipTimer -= Time.deltaTime;

        // 2. Get Player Input (WASD) purely for influence calculation
        Vector2 inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        // 3. Compare Input vs Slip Direction (Dot Product)
        // Dot = 1 (Same direction), Dot = -1 (Opposite direction), Dot = 0 (Perpendicular)
        if (change != Vector3.zero && inputDir != Vector2.zero)
        {
            float alignment = Vector2.Dot(inputDir, (Vector2)change.normalized);

            // If alignment > 0 (Same dir), we add time (slide longer)
            // If alignment < 0 (Opposite dir), we subtract time (stop faster)
            currentSlipTimer += alignment * slipInputInfluence * Time.deltaTime;
        }

        // 4. Check if slip is finished
        if (currentSlipTimer <= 0)
        {
            currentSlipTimer = 0;
            animator.SetBool("slipping", false);
            changeState(PlayerState.walk);
            return;
        }

        // 5. Apply Movement
        if (change != UnityEngine.Vector3.zero)
        {
            animator.SetFloat("moveX", change.x);
            animator.SetFloat("moveY", change.y);
            animator.SetBool("slipping", true);

            // Move in the push direction
            myRigidbody.MovePosition(myRigidbody.position + new UnityEngine.Vector2(change.x, change.y) * speed * Time.fixedDeltaTime);

            if (stepSoundManager != null && lastStepSoundTime >= stepSoundCooldown)
            {
                lastStepSoundTime = 0f;
                stepSoundManager.PlayStepSound(transform.position);
            }
        }
    }

    // --- Item Handling ---
    public void RaiseItem()
    {
        if (currentState != PlayerState.interact)
        {
            animator.SetBool("receive_item", true);
            currentState = PlayerState.interact;
            receiveItemSprite.sprite = inventory.currentItem.itemSprite;
        }
        else
        {
            animator.SetBool("receive_item", false);
            currentState = PlayerState.walk;
            receiveItemSprite.sprite = null;
        }
    }

    // --- Attack ---
    private IEnumerator AttackCo()
    {
        animator.SetBool("attacking", true);
        currentState = PlayerState.attack;
        yield return new WaitForSeconds(0.26f);
        animator.SetBool("attacking", false);
        currentState = PlayerState.walk;
    }

    public void changeState(PlayerState newState)
    {
        // 1. Handle Visual Effects Toggle
        if (newState == PlayerState.slip)
        {
            // Enable Slip Trails (The Lines)
            if (slipTrails != null)
            {
                foreach (var trail in slipTrails)
                {
                    if (trail != null) trail.emitting = true;
                }
            }

            // Disable Footprints Script (Stops stamping)
            if (footprintSystem != null) footprintSystem.enabled = false;
        }
        else if (newState == PlayerState.walk)
        {
            // Disable Slip Trails
            if (slipTrails != null)
            {
                foreach (var trail in slipTrails)
                {
                    if (trail != null) trail.emitting = false;
                }
            }

            // Enable Footprints Script
            if (footprintSystem != null) footprintSystem.enabled = true;
        }
        else
        {
            // For other states (idle, attack), decide behavior.
            // Usually we turn off trails, and leave footprints enabled (but they won't stamp if not moving)
            if (slipTrails != null)
            {
                foreach (var trail in slipTrails)
                {
                    if (trail != null) trail.emitting = false;
                }
            }
            // We generally keep the footprint system enabled here so old prints can fade out
            if (footprintSystem != null) footprintSystem.enabled = true;
        }
        currentState = newState;
        if (newState != PlayerState.attack) animator.SetBool("attacking", false);
        if (newState != PlayerState.walk) animator.SetBool("moving", false);
        if (newState != PlayerState.slip) animator.SetBool("slipping", false);
    }

    // --- External Physics Interaction ---
    // UPDATED: Now takes slipping_time as a parameter
    public void pushed(Vector3 direction, float slipping_time, bool ignoreCooldown = false)
    {
        if (Time.time < lastPushTime + pushCooldown && !ignoreCooldown)
        {
            return;
        }

        Debug.Log($"Player pushed. Slipping for base time: {slipping_time}s");
        lastPushTime = Time.time;

        changeState(PlayerState.slip);
        currentSlipTimer = slipping_time; // Set the timer

        change.x = direction.x;
        change.y = direction.y;
        //change.Normalize();
    }

    // Stop slipping on collision
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == PlayerState.slip)
        {
            Debug.Log("Hit wall while slipping - Recovering.");
            currentSlipTimer = 0; // Kill the timer
            change = Vector3.zero;
            if (slipTrails != null)
            {
                foreach (var trail in slipTrails)
                {
                    if (trail != null) trail.emitting = false;
                }
            }
            changeState(PlayerState.walk);
        }
    }

    // --- Magic Attacks ---
    private void CastFireball()
    {
        animator.SetTrigger("castFireball");
        GameObject fireball = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
        fireball.GetComponent<Fireball>().SetDirection(new Vector2(animator.GetFloat("moveX"), animator.GetFloat("moveY")));
    }

    private void CastLightning()
    {
        animator.SetTrigger("castFireball");
        Vector2 castDirection = new Vector2(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        if (castDirection.sqrMagnitude < 0.01f)
            castDirection = new Vector2(0, -1);
        Vector2 strikePosition = (Vector2)transform.position + castDirection.normalized * lightningCastOffset;
        Instantiate(lightningEffectPrefab, strikePosition, Quaternion.identity);
    }

    public void UpgradeMagicLevel(int amount)
    {
        magicLevel.runtimeValue += amount;
        magicLevel.initialValue += amount;
    }

    public Vector2 GetCurrentMovementDirection()
    {
        return new Vector2(animator.GetFloat("moveX"), animator.GetFloat("moveY")).normalized;
    }
}