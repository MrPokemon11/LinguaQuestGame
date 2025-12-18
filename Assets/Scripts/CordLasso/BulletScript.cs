using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{

    GunScript gun;
    public WordLassoManager wordLassoManager;

    // Start is called before the first frame update
    void Start()
    {
        gun = GameObject.FindGameObjectWithTag("Gun").GetComponent<GunScript>();
        wordLassoManager = FindFirstObjectByType<WordLassoManager>();
    }

    void Update()
    {
        if (wordLassoManager != null && wordLassoManager.paused)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        Debug.Log("Bullet hit: " + collision.gameObject.name +
                  " | Tag: " + collision.gameObject.tag);

        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "CatchZone" || collision.gameObject.name == "WordSpawnArea")
        {
            return;
        }
        if (collision.gameObject.tag == "Word")
        {
            gun.TargetHit(collision.gameObject);
        }

        Destroy(gameObject);
    }
}
