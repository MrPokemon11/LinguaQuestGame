using System;
using UnityEngine;

[Serializable]
public class CharacterStats
{
    public Character character;
    public int attack;
    public int defense;
    public int speed;
    public int health;

    public void ModifyStat(StatType stat, int amount)
    {
        switch(stat)
        {
            case StatType.Attack:
                attack += amount;
                break;

            case StatType.Defense:
                defense += amount;
                break;

            case StatType.Speed:
                speed += amount;
                break;

            case StatType.Health:
                health += amount;
                break;

            default:
                Debug.LogError("Unknown stat type passed to ModifyStat.");
                break;
        }
    }
}

public enum StatType
{
    Attack,
    Defense,
    Speed,
    Health
}
