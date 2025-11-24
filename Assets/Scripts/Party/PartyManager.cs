using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance { get; private set; }
    public List<CharacterStats> Party = new List<CharacterStats>();
    public List<GameObject> PartyPrefabs = new List<GameObject>();
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
        //Basically add the player to the party and bring other party members to other scenes
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
            for(int i = 1; i < PartyPrefabs.Count; i++)
            {
                GameObject[] allObjects = FindObjectsOfType<GameObject>();

                foreach(GameObject obj in allObjects)
                {
                    if (obj.name.StartsWith(PartyPrefabs[i].name))
                    {
                        Destroy(obj);
                    }
                }
                Debug.Log("PARTY SYSTEM: Creating clone");
                GameObject copy = Instantiate(PartyPrefabs[i], player.position, player.rotation);
                StartCoroutine(FollowCoroutine(copy, player));
            }
        }
        else if(creator != null && Party.Count < 1)
        {
            UpdateParty(creator.character);
        }
    }

    //This is the actual process of adding to the party. Don't need to interact with this funciton.
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
            PartyPrefabs.Add(creator.character.characterPrefab);
            //PartyObjects.Add(creator.gameObject);
        }
        else
        {
            Debug.LogError($"No character object found on character {creator.character.name}. (characterPrefab was null, did you make a prefab for this character?)");
            return;
        }
    }

    //Use PartyManager.Instance.AddToParty(Whatever GameObject); in other scripts to add them to the party
    public void AddToParty(GameObject follower)
    {
        if(player == null)
        {
            Debug.LogError("No player in the game to follow");
            return;
        }

        CharacterCreator creator = follower.GetComponent<CharacterCreator>();
        if(creator == null)
        {
            Debug.LogError($"No CharacterCreator component was found on the {follower.name} GameObject");
            return;
        }

        if(Party.Contains(creator.character))
        {
            Debug.Log("Already added character to party, returning.");
            return;
        }

        UpdateParty(creator.character);
        StartCoroutine(FollowCoroutine(follower, player));
    }

    /*
        To modify a character's stat, you must reference:
        - The character's name (string)
        - The stat you want to change (StatType)
        - The amount you want to increase or decrease by (int)

        Example:
            PartyManager.Instance.ModifyStat("Player", StatType.Attack, 5);   // Increases Player's attack by 5
            PartyManager.Instance.ModifyStat("Player", StatType.Health, -10); // Decreases Player's health by 10

        The available stat types are:
            - StatType.Attack
            - StatType.Defense
            - StatType.Speed
            - StatType.Health

        If you want to add more stats:
        1. Open CharacterStats.cs
        2. Add a new integer field (e.g., public int mana;)
        3. Add the corresponding StatType (e.g., Mana) in the StatType enum
        4. Add a case for the new stat inside ModifyStat()

        This system automatically supports any stat you add.
    */
    public void ModifyStat(string name, StatType type, int amount)
    {
        GetCharacter(name).ModifyStat(type, amount);
    }

    public CharacterStats GetCharacter(string name)
    {
        foreach(CharacterStats characterStat in Party)
        {
            if(characterStat.character.characterName == name)
            {
                return characterStat;
            }
        }

        Debug.LogError($"CHARACTER {name} NOT FOUND IN PARTY");
        return null;
    }

    private IEnumerator FollowCoroutine(GameObject follower, Transform objectToFollow)
    {
        Transform followerTransform = follower.transform;
    
        while(true)
        {
            if(objectToFollow == null || playerExploring == null) yield break;

            if(playerExploring.isMoving)
            {
                float distance = Vector3.Distance(objectToFollow.position, followerTransform.position);
                if(distance > followDistance)
                {
                    followerTransform.position = Vector3.Lerp(
                        followerTransform.position,
                        objectToFollow.position,
                        Time.deltaTime * playerExploring.speed
                    );
                }
            }
            
            yield return null;
        }
    }
}
