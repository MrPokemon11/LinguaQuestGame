using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SentenceJsonLoader
{
    [System.Serializable]
    private class SentencePack
    {
        public string packName;
        public string language;
        public List<SentenceData> sentences;
    }

    public static List<SentenceData> LoadPackFromStreamingAssets(string filename)
    {
        string path = Path.Combine(Application.streamingAssetsPath, filename);

        if (!File.Exists(path))
        {
            Debug.LogError($"[SentenceJsonLoader] File not found: {path}");
            return new List<SentenceData>();
        }

        try
        {
            string json = File.ReadAllText(path);

            // Deserialize the pack wrapper
            SentencePack pack = JsonUtility.FromJson<SentencePack>(json);

            if (pack == null)
            {
                Debug.LogError($"[SentenceJsonLoader] Failed to parse JSON from {filename}");
                return new List<SentenceData>();
            }

            if (pack.sentences == null || pack.sentences.Count == 0)
            {
                Debug.LogWarning($"[SentenceJsonLoader] No sentences found in {filename}");
                return new List<SentenceData>();
            }

            Debug.Log($"[SentenceJsonLoader] Loaded {pack.sentences.Count} sentences from {filename} (Pack: {pack.packName})");

            return pack.sentences;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SentenceJsonLoader] Error loading {filename}: {e.Message}");
            return new List<SentenceData>();
        }
    }

    public static List<SentenceData> LoadPackFromResources(string filename)
    {
        TextAsset asset = Resources.Load<TextAsset>(filename);

        if (asset == null)
        {
            Debug.LogError($"[SentenceJsonLoader] File not found in Resources: {filename}");
            return new List<SentenceData>();
        }

        try
        {
            SentencePack pack = JsonUtility.FromJson<SentencePack>(asset.text);

            if (pack == null || pack.sentences == null)
            {
                Debug.LogError($"[SentenceJsonLoader] Failed to parse JSON from Resources: {filename}");
                return new List<SentenceData>();
            }

            Debug.Log($"[SentenceJsonLoader] Loaded {pack.sentences.Count} sentences from Resources/{filename}");

            return pack.sentences;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SentenceJsonLoader] Error loading from Resources {filename}: {e.Message}");
            return new List<SentenceData>();
        }
    }
}