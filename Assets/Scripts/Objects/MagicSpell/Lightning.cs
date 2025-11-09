using UnityEngine;

public class Lightning : MonoBehaviour
{
    [Header("Effect Settings")]
    public float lifeTime = 1.5f; // How long the effect stays before being destroyed

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip strikeSound;

    void Start()
    {
        // Get components and play the strike sound immediately
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource && strikeSound)
        {
            audioSource.PlayOneShot(strikeSound);
        }

        // The animation will handle the visual effect.
        // We just destroy the object after its lifetime is over.
        Destroy(gameObject, lifeTime);
    }

    // This script doesn't need an Update() or OnTriggerEnter2D()
    // because it's not a moving projectile. It's a stationary effect.
    // The damage logic would be handled by adding a CircleCollider2D
    // to the prefab and having enemies react to it.
    private Animator animator;
}