using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(menuName = "LinguaQuest/Sentence Runtime Bank", fileName = "SentenceRuntimeBank")]
public class SentenceRuntimeBank : ScriptableObject
{
    [Tooltip("Load from StreamingAssets (runtime) or direct file path (editor)")]
    public bool useStreamingAssets = true;

    [Tooltip("JSON files to load")]
    public List<string> jsonFiles = new() { "test1.json" };

    [HideInInspector] public List<SentenceData> sentences = new();

    public void LoadAll()
    {
        sentences.Clear();

        foreach (var file in jsonFiles)
        {
            List<SentenceData> loadedSentences;

            if (useStreamingAssets)
            {
                loadedSentences = SentenceJsonLoader.LoadPackFromStreamingAssets(file);
            }
            else
            {
                // Load from project folder (editor only)
                string path = Path.Combine(Application.dataPath, "Scripts", "SyntaxSword", file);
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var pack = JsonUtility.FromJson<SentencePack>(json);
                    loadedSentences = pack?.sentences ?? new List<SentenceData>();
                    Debug.Log($"[SentenceRuntimeBank] Loaded {loadedSentences.Count} from editor path: {path}");
                }
                else
                {
                    Debug.LogError($"[SentenceRuntimeBank] File not found: {path}");
                    loadedSentences = new List<SentenceData>();
                }
            }

            sentences.AddRange(loadedSentences);
        }

        Debug.Log($"[SentenceRuntimeBank] Total loaded: {sentences.Count} sentences from {jsonFiles.Count} file(s).");
    }

    [System.Serializable]
    private class SentencePack
    {
        public string packName;
        public string language;
        public List<SentenceData> sentences;
    }
}