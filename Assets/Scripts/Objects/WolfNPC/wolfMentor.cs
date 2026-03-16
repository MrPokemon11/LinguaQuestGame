using UnityEngine;
using TMPro;
using System.Collections;

public class WolfMentor : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogBox;
    public TextMeshProUGUI dialogText;

    [Header("Data & Ordering")]
    public InstructionQueue instructionQueue; //

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float arrivalThreshold = 0.5f;
    public Vector3 offsetFromPlayer = new Vector3(2f, -1f, 0); // Where the wolf stops to talk
    public Vector3 exitDirection = new Vector3(-10f, 0, 0); // Where the wolf walks to disappear

    [Header("Visuals")]
    public SpriteRenderer wolfRenderer;
    public Animator animator;
    public Collider2D wolfCollider;

    private string[] currentDialogs;
    private int currentIndex = 0;
    private bool isWorking = false;
    private Transform playerTransform;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        SetVisibility(false);
    }

    public void OnTeachingSignalRaised()
    {
        if (isWorking) return;

        if (instructionQueue == null)
        {
            Debug.LogError("WolfMentor: InstructionQueue is not assigned in the Inspector!");
            return;
        }
        if (MechanicContentLoader.Instance == null)
        {
            Debug.LogError("WolfMentor: MechanicContentLoader not found in scene!");
            return;
        }

        string nextID = instructionQueue.GetNextID(); //
        currentDialogs = MechanicContentLoader.Instance.GetDialogs(nextID);

        if (currentDialogs != null && currentDialogs.Length > 0)
        {
            StartCoroutine(PerformTeachingCycle());
        }
    }

    private IEnumerator PerformTeachingCycle()
    {
        isWorking = true;

        // 1. Position Wolf off-screen and make visible
        transform.position = playerTransform.position + new Vector3(8f, 2f, 0);
        SetVisibility(true);

        // 2. Walk to the player
        Vector3 targetPos = playerTransform.position + offsetFromPlayer;
        while (Vector3.Distance(transform.position, targetPos) > arrivalThreshold)
        {
            MoveTowards(targetPos);
            yield return null;
        }

        // 3. Stop and Face Player
        StopAndFacePlayer();

        // 4. Lock Player and Open Dialog
        GameObject playerObj = playerTransform.gameObject;
        playerObj.GetComponent<PlayerExploring>().changeState(PlayerState.interact);
        OpenDialogue();

        // Wait for dialogue to finish via Update()
        while (dialogBox.activeSelf)
        {
            yield return null;
        }

        // 5. Unlock Player and Walk Away
        playerObj.GetComponent<PlayerExploring>().changeState(PlayerState.walk);
        instructionQueue.MarkCurrentComplete(); //

        Vector3 disappearancePoint = transform.position + exitDirection;
        while (Vector3.Distance(transform.position, disappearancePoint) > arrivalThreshold)
        {
            MoveTowards(disappearancePoint);
            yield return null;
        }

        SetVisibility(false);
        isWorking = false;
    }

    private void MoveTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        // Reuse animation logic from PetMovement
        animator.SetBool("isWalking", true);
        animator.SetFloat("moveX", direction.x);
        animator.SetFloat("moveY", direction.y);
    }

    private void StopAndFacePlayer()
    {
        animator.SetBool("isWalking", false);
        Vector3 dir = (playerTransform.position - transform.position).normalized;
        animator.SetFloat("moveX", dir.x);
        animator.SetFloat("moveY", dir.y);
    }

    private void OpenDialogue()
    {
        currentIndex = 0;
        dialogBox.SetActive(true);
        dialogText.text = currentDialogs[currentIndex];
    }

    void Update()
    {
        if (!isWorking || !dialogBox.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.E)) // Matches Sign.cs logic
        {
            currentIndex++;
            if (currentIndex < currentDialogs.Length)
            {
                dialogText.text = currentDialogs[currentIndex];
            }
            else
            {
                dialogBox.SetActive(false);
            }
        }
    }

    private void SetVisibility(bool visible)
    {
        wolfRenderer.enabled = visible;
        if (wolfCollider != null) wolfCollider.enabled = visible;
    }
}