using UnityEngine;

public class TotemMinigame : MonoBehaviour
{
    [SerializeField] private GameObject TopPiece;
    [SerializeField] private GameObject TopPieceSpot;
    [SerializeField] private GameObject MiddlePiece;
    [SerializeField] private GameObject MiddlePieceSpot;
    [SerializeField] private GameObject BottomPiece;
    [SerializeField] private GameObject BottomPieceSpot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //debug method to check if the puzzle is complete. need to make it so interacting with the Chief triggers this code instead.
        // also make it do more than outputting to the console
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (CheckTotemPieces())
            {
                Debug.Log("Correct!");
            } else
            {
                Debug.Log("Incorrect.");
            }
        }
    }

    //Is this the most effective way to do this? Probably not. Does it work? It should.
    public bool CheckTotemPieces()
    {
        //check if the totem pieces are in their corresponding spots

        if (!TopPiece.GetComponent<Rigidbody2D>().IsTouching(TopPieceSpot.GetComponent<BoxCollider2D>()))
        {
            return false;
        }

        if (!MiddlePiece.GetComponent<Rigidbody2D>().IsTouching(MiddlePieceSpot.GetComponent<BoxCollider2D>()))
        {
            return false;
        }

        if (!BottomPiece.GetComponent<Rigidbody2D>().IsTouching(BottomPieceSpot.GetComponent<BoxCollider2D>()))
        {
            return false;
        }

        return true;
    }
}
