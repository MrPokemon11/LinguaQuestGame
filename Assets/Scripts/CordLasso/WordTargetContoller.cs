using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WordTargetController : MonoBehaviour
{
    public TMP_Text label;
    private WordLassoManager manager;
    private bool collected = false;
    private Vector3 initialPosition;
    private Rigidbody2D cachedRb;

    public void SetWord(string word, WordLassoManager mgr)
    {
        label.text = word;
        manager = mgr;
        initialPosition = transform.position;
        collected = false;
        cachedRb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collected) return;

        // When word reaches the catch zone
        if (collision.CompareTag("CatchZone"))
        {
            collected = true;
            manager.OnWordCollected(this);
            gameObject.SetActive(false);   // hide it after collection
        }
    }

    public string GetWord()
    {
        return label.text;
    }

    public void ResetWord()
    {
        collected = false;
        gameObject.SetActive(true);
        transform.position = initialPosition;

        if (cachedRb == null)
            cachedRb = GetComponent<Rigidbody2D>();

        if (cachedRb != null)
        {
            cachedRb.linearVelocity = Vector2.zero;
            cachedRb.angularVelocity = 0f;
        }
    }
}
