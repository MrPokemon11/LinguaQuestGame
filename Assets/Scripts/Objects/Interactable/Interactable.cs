using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public bool playerInRange;
    public Signal context;
    public AudioSource audioSource;
    public AudioClip interactSound;
    public Signal interactSignal;
    public BoolValue firstInteractionDone;
    public Animator flashingAnimator;

    public virtual void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        if (flashingAnimator == null)
        {
            flashingAnimator = GetComponent<Animator>();
        }
        if (firstInteractionDone.runtimeValue == true)
        {
            if (flashingAnimator != null)
            {
                flashingAnimator.SetBool("isFlashing", false);
            }
        }
    }

    public virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            context.Raise();
        }
    }

    public virtual void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            context.Raise();
        }
    }

    public virtual void Interact()
    {
        Debug.Log("Base Interact called on " + gameObject.name);
        if (audioSource != null && interactSound != null)
        {
            audioSource.PlayOneShot(interactSound);
        }

        if (interactSignal != null)
        {
            interactSignal.Raise();
        }

        if (firstInteractionDone.runtimeValue == false)
        {
            Debug.Log("First interaction done.");
            firstInteractionDone.runtimeValue = true;
            if (flashingAnimator != null)
            {
                Debug.Log("Stopping flashing animation.");
                flashingAnimator.SetBool("isFlashing", false);
            }
        }
    }
}
