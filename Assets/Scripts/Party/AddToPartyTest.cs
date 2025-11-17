using UnityEngine;

public class AddToPartyTest : MonoBehaviour
{
    private bool onlyAddOnce;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            if(onlyAddOnce) return;

            onlyAddOnce = true;
            PartyManager.Instance.AddToParty(gameObject);
            Debug.Log($"{gameObject.name} added to the party!");
            // gameObject.SetActive(false);
        }
    }
}
