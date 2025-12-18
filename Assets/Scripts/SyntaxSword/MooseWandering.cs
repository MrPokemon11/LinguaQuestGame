using System.Collections;
using UnityEngine;

public class MooseWandering : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float wanderRadius = 3f;
    [SerializeField] private float waitTimeMin = 1f;
    [SerializeField] private float waitTimeMax = 3f;

    [Header("Throwing Settings")]
    [SerializeField] private float throwDuration = 0.2f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;

    private Vector2 _startPosition;
    private Vector2 _currentTarget;
    private bool _isThrowing;
    private bool _isWalking;
    private Vector2 _facingDirection = Vector2.down;

    private void Start()
    {
        animator = animator ? animator : GetComponent<Animator>();
        rb = rb ? rb : GetComponent<Rigidbody2D>();

        _startPosition = rb.position;
        _isWalking = false;
        _facingDirection = Vector2.down;
        UpdateAnimation();

        //StartCoroutine(WanderRoutine());
    }

    public void BeginWandering()
    {
        StartCoroutine(WanderRoutine());
    }

    private void Update()
    {
        if (_isWalking)
        {
            UpdateAnimation();
        }
    }

    private void UpdateAnimation()
    {
        animator.SetBool("isMoving", _isWalking);
        animator.SetFloat("moveX", _facingDirection.x);
        animator.SetFloat("moveY", _facingDirection.y);
    }

    private IEnumerator WanderRoutine()
    {
        var waitFixed = new WaitForFixedUpdate();

        while (true)
        {
            // Pause movement while throwing
            if (_isThrowing)
            {
                _isWalking = false;
                yield return null;
                continue;
            }

            // Pick a target if none or reached
            if (_currentTarget == Vector2.zero || Vector2.Distance(rb.position, _currentTarget) < 0.1f)
            {
                _currentTarget = _startPosition + Random.insideUnitCircle * wanderRadius;
            }

            // Walk toward target
            _isWalking = true;
            while (!_isThrowing && Vector2.Distance(rb.position, _currentTarget) > 0.1f)
            {
                Vector2 dir = (_currentTarget - rb.position).normalized;
                if (dir.sqrMagnitude > 0.0001f) _facingDirection = dir;

                rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
                yield return waitFixed;
            }

            // Arrived; idle for a bit
            _isWalking = false;
            animator.SetBool("isMoving", false);
            float waitTime = Random.Range(waitTimeMin, waitTimeMax);
            float endTime = Time.time + waitTime;
            while (!_isThrowing && Time.time < endTime)
                yield return null;

            // Force a new target next loop
            _currentTarget = Vector2.zero;
        }
    }

    public void ThrowWordBlock()
    {
        StartCoroutine(PerformThrow());
    }

    public IEnumerator PerformThrow()
    {
        Debug.Log("[MooseWandering] Performing throw animation.");
        _isThrowing = true;
        _isWalking = false;
        animator.SetBool("isMoving", false);

        animator.SetTrigger("throw");

        yield return new WaitForSeconds(throwDuration * 0.5f);
        yield return new WaitForSeconds(throwDuration * 0.5f);

        _isThrowing = false;
        _isWalking = true;
        _currentTarget = Vector2.zero; // pick a fresh wander target
    }
}
