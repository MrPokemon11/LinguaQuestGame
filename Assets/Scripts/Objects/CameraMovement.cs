using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target; // The target to follow
    public float smoothing; // Speed of the camera movement
    public Animator cameraAnimator; // Animator for camera effects
    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform; // Find the player by tag
        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
        cameraAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target == null) return; // If target is not set, do nothing
        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
        float distance = Vector3.Distance(transform.position, targetPosition);
        // The farther the camera is, the faster it moves (speed scales with distance)
        float dynamicSmoothing = smoothing * distance;
        transform.position = Vector3.Lerp(transform.position, targetPosition, dynamicSmoothing * Time.deltaTime);
    }

    public void DoScreenKick()
    {
        StartCoroutine(ScreenKick());
    }

    public IEnumerator ScreenKick()
    {
        cameraAnimator.SetBool("KickActive", true);
        yield return new WaitForSeconds(0.1f);
        cameraAnimator.SetBool("KickActive", false);
    }

    // Keep original for backward compatibility
    public void PayAttentionTo(GameObject thing)
    {
        StartCoroutine(PayAttentionToThings(thing, 2f));
    }

    private IEnumerator PayAttentionToThings(GameObject thing, float duration)
    {
        target = thing.transform;
        yield return new WaitForSeconds(duration);

        // Safety check: ensure Player still exists before switching back
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }
    // --- NEW: Overloaded method with custom duration ---
    public void PayAttentionTo(GameObject thing, float duration)
    {
        StartCoroutine(PayAttentionToThings(thing, duration));
    }
}
