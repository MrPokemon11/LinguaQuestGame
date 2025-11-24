using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Game/Character", order = 1)]
public class Character : ScriptableObject
{
    [Header("Character Info")]
    public string characterName;
    public GameObject characterPrefab;
}
