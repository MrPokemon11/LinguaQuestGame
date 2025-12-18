[System.Serializable]
public class PlayerData
{
    public string playerName;
    public int score;
    public string currentCharacterClass;
    public int syntaxPoints;
    public int morphologyPoints;
    public int phonologyPoints;

    public PlayerData()
    {
        playerName = "New Player";
        score = 0;
        currentCharacterClass = "Linguist";
    }

    public void setPlayerName(string name)
    {
        playerName = name;
    }

    public string getPlayerName()
    {
        return playerName;
    }
}
