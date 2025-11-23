using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BeaverInWorld : Interactable
{
    public GameObject dialogBox;
    public TextMeshProUGUI dialogText;
    public string[] dialogs;
    public bool dialogActive;
    public int currentDialogIndex = 0;
    public GameObject player;
    public string SceneToFight;
    private bool awaitingChoice = false;
    public string choicePrompt = "Press Y to help or N to decline.";

    public override void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        if (dialogBox == null)
        {
            dialogBox = GameObject.FindGameObjectWithTag("DialogBox");
        }
        if (dialogText == null)
        {
            dialogText = dialogBox.GetComponentInChildren<TextMeshProUGUI>();
        }
        player = GameObject.FindGameObjectWithTag("Player");
    }
    public virtual void Update()
    {
        if (dialogActive && awaitingChoice)
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                CleanupDialog();
                SyntaxSwordMinigame();
                return;
            }
            if (Input.GetKeyDown(KeyCode.N))
            {
                CleanupDialog();
                return;
            }
            return;
        }

        if (dialogActive && Input.GetKeyDown(KeyCode.E))
        {
            if (audioSource != null && interactSound != null)
            {
                audioSource.PlayOneShot(interactSound);
            }

            if (!dialogBox.activeSelf)
            {
                dialogBox.SetActive(true);
                currentDialogIndex = 0;
                dialogText.text = dialogs.Length > 0 ? dialogs[currentDialogIndex] : "";
            }
            else
            {
                currentDialogIndex++;
                if (currentDialogIndex < dialogs.Length)
                {
                    dialogText.text = dialogs[currentDialogIndex];
                }
                else
                {
                    awaitingChoice = true;
                    dialogText.text = choicePrompt;
                }
            }
        }
        else if (dialogActive && Input.GetKeyDown(KeyCode.Escape))
        {
            awaitingChoice = false;
            dialogBox.SetActive(false);
            dialogActive = false;
            currentDialogIndex = 0;
        }
        else if (dialogActive && Input.GetKeyDown(KeyCode.Space))
        {
            awaitingChoice = false;
            dialogBox.SetActive(false);
            dialogActive = false;
            currentDialogIndex = 0;
        }
        else if (dialogActive && Input.GetKeyDown(KeyCode.Return))
        {
            awaitingChoice = false;
            dialogBox.SetActive(false);
            dialogActive = false;
            currentDialogIndex = 0;
        }
    }


    public override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            dialogActive = true;
            currentDialogIndex = 0;
            context.Raise();
        }
    }

    public override void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            dialogActive = false;
            dialogBox.SetActive(false);
            context.Raise();
            currentDialogIndex = 0;
            awaitingChoice = false;
        }
    }

    public void SyntaxSwordMinigame()
    {
        player.GetComponent<PlayerExploring>().StartingPosition.runtimeValue = player.transform.position;
        SceneTracker.Instance.RecordSceneAndPosition(player.transform.position);
        SceneManager.LoadScene(SceneToFight);
    }

    private void CleanupDialog()
    {
        dialogBox.SetActive(false);
        dialogActive = false;
        awaitingChoice = false;
        currentDialogIndex = 0;
    }

}
