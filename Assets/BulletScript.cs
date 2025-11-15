using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{

    GunScript gun;

    // Start is called before the first frame update
    void Start()
    {
        gun = GameObject.FindGameObjectWithTag("Gun").GetComponent<GunScript>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        Debug.Log("Bullet hit: " + collision.gameObject.name +
                  " | Tag: " + collision.gameObject.tag);

        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "CatchZone" || collision.gameObject.name == "WordSpawnArea")
        {
            return;
        }
        if(collision.gameObject.tag == "Word")
        {
            gun.TargetHit(collision.gameObject);
        }

        Destroy(gameObject);
    }
}
