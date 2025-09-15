using UnityEngine;

public class Fireball : MonoBehaviour
{
    [Header("Motion")]
    public float speed = 8f;
    public float lifeTime = 0.85f;
    public float horizontalGravity = -4f;   // used only for horizontal shots (negative pulls downward)
    public float verticalSpeedModification = -8f;

    [Header("Visual rotation (degrees CCW)")]
    public float angleRight = 90f;           // base orientation
    public float angleLeft = -90f;          // as you requested
    public float angleUp = 180f;
    public float angleDown = 0f;

    private Vector2 velocity;
    private bool applyArc;                  // true only for mostly-horizontal shots
    private Animator animator;
    private bool hasExploded;
    private float lifeTimer;
    public AudioSource audioSource;
    public AudioClip gatherSound;
    public AudioClip throwSound;
    public AudioClip explodeSound;
    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        hasExploded = false;
        lifeTimer = lifeTime; // Initialize the timer with the lifetime value
    }

    public void SetDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
        dir.Normalize();

        velocity = dir * speed;

        // Arc only if shot is mostly horizontal (|x| >= |y|)
        applyArc = Mathf.Abs(dir.x) >= Mathf.Abs(dir.y);

        if (dir == Vector2.up)
        {
            // Apply specific behavior for up direction
            velocity.y /= 2.2f;

        }
        else if (dir == Vector2.down)
        {
            // Apply specific behavior for down direction
            velocity.y /= 1.5f;
        }

        // Snap rotation to cardinal using your angles:
        float angle;
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            angle = (dir.x >= 0f) ? angleRight : angleLeft;   // Right / Left
        else
            angle = (dir.y >= 0f) ? angleUp : angleDown;    // Up / Down

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Update()
    {
        if (hasExploded) return;

        if (applyArc)
            velocity.y += horizontalGravity * Time.deltaTime; // arc only for horizontal shots

        transform.position += (Vector3)(velocity * Time.deltaTime);

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
            Explode();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            return;
        }
        if (!hasExploded)
        {
            Explode();
        }
    }

    private void Explode()
    {
        hasExploded = true;
        audioSource.PlayOneShot(explodeSound);
        animator.SetTrigger("Explode"); // play explosion
        Destroy(gameObject, 1f); // give time for animation
    }
}
