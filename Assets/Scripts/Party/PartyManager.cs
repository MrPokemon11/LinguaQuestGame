using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance { get; private set; }
    public List<CharacterStats> Party = new List<CharacterStats>();
    public List<GameObject> PartyObjects = new List<GameObject>();

    [Header("Follow Settings")]
    public float followDistance = 2f;

    [HideInInspector] public PlayerExploring playerExploring;
    [HideInInspector] public PlayerMovement playerMovement;
    [HideInInspector] public Transform player;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable() => SceneManager.activeSceneChanged += Initialize;
    void OnDisable() => SceneManager.activeSceneChanged -= Initialize;

    // Called each time a scene changes
    void Initialize(Scene oldScene, Scene newScene)
    {
        playerExploring = null;
        playerMovement = null;
        StopAllCoroutines();
        playerExploring = FindObjectOfType<PlayerExploring>();

        if(playerExploring != null)
        {
            player = playerExploring.transform;
        }
        else
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
            
            if(playerMovement != null)
            {
                player = playerMovement.transform;
            }
            else
            {
                Debug.LogError("No PlayerExploring or PlayerMovement script found in scene " + newScene.name);
                return;
            }
        }

        CharacterCreator creator = player.GetComponent<CharacterCreator>();
        
        if(Party.Count > 1 && creator != null)
        {
            for(int i = 1; i < PartyObjects.Count; i++)
            {
                GameObject[] allObjects = FindObjectsOfType<GameObject>();

                foreach(GameObject obj in allObjects)
                {
                    if (obj.name.StartsWith(PartyObjects[i].name))
                    {
                        Destroy(obj);
                    }
                }
                Debug.Log("PARTY SYSTEM: Creating clone");
                GameObject copy = Instantiate(PartyObjects[i], player.position, player.rotation);
                StartCoroutine(FollowCoroutine(copy));
            }
        }
        else if(creator != null)
        {
            UpdateParty(creator.character);
        }
    }

    public void UpdateParty(CharacterStats creator)
    {
        if (creator == null)
        {
            Debug.LogError("No character found to update party. (character was null)");
            return;
        }

        if (creator.character.characterPrefab != null)
        {
            Party.Add(creator);
            PartyObjects.Add(creator.character.characterPrefab);
        }
        else
        {
            Debug.LogError($"No character object found on character {creator.character.name}. (characterPrefab was null, did you make a prefab for this character?)");
            return;
        }
    }

    public void AddToParty(GameObject follower)
    {
        if(player == null)
        {
            Debug.LogError("No player in the game to follow");
            return;
        }

        CharacterCreator creator = follower.GetComponent<CharacterCreator>();

        if(Party.Contains(creator.character))
        {
            Debug.Log("Already added character to party, returning.");
            return;
        }

        UpdateParty(creator.character);
        StartCoroutine(FollowCoroutine(follower));
    }

    private IEnumerator FollowCoroutine(GameObject follower)
    {
        Transform followerTransform = follower.transform;
    
        while(true)
        {
            if(player == null || playerExploring == null) yield break;

            if(playerExploring.isMoving)
            {
                float distance = Vector3.Distance(player.position, followerTransform.position);
                if(distance > followDistance)
                {
                    followerTransform.position = Vector3.Lerp(
                        followerTransform.position,
                        player.position,
                        Time.deltaTime * playerExploring.speed
                    );
                }
            }
            
            yield return null;
        }
    }
}
