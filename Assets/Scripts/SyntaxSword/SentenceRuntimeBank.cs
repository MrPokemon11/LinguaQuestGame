using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking; // Required for WebGL
using UnityEngine.Scripting;

[CreateAssetMenu(menuName = "LinguaQuest/Sentence Runtime Bank", fileName = "SentenceRuntimeBank")]
public class SentenceRuntimeBank : ScriptableObject
{
    public List<string> jsonFiles = new() { "test1.json" };
    [HideInInspector] public List<SentenceData> sentences = new();

    // Change 'void' to 'IEnumerator' and add a callback
    public IEnumerator LoadAllCoroutine(System.Action onComplete)
    {
        sentences.Clear();

        foreach (var fileName in jsonFiles)
        {
            string path = Path.Combine(Application.streamingAssetsPath, fileName);

            // UNITY WEBREQUEST (Works in WebGL AND Editor)
            using (UnityWebRequest request = UnityWebRequest.Get(path))
            {
                // Wait for the "download" to finish
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;
                    ProcessJson(json, fileName);
                }
                else
                {
                    Debug.LogError($"[Error] Could not load {fileName}: {request.error}");
                }
            }
        }

        Debug.Log($"[Bank] Finished loading {sentences.Count} sentences.");

        // Tell the game we are done, so it can start spawning
        onComplete?.Invoke();
    }

    private void ProcessJson(string json, string fileName)
    {
        // Your parsing logic here
        var pack = JsonUtility.FromJson<SentencePack>(json);
        if (pack != null && pack.sentences != null)
        {
            sentences.AddRange(pack.sentences);
        }
    }

    [Preserve]
    [System.Serializable]
    public class SentencePack
    {
        public string packName;
        public List<SentenceData> sentences;
    }
}