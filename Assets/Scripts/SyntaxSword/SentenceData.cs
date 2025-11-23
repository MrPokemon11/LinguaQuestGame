using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SentenceData
{
    public string guid;
    public string sentence;
    public string topic;
    public string subtopic;
    public int difficulty;
    public List<WordEntry> entries;

    [System.Serializable]
    public class WordEntry
    {
        public string word;
        public string shownLabel;
        public bool isLabelCorrect;
    }
}