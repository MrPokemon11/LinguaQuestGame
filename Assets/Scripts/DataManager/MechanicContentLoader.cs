using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[System.Serializable] public class MechanicData { public List<MechanicAdviceRaw> advices; }
[System.Serializable] public class MechanicAdviceRaw { public string minigameID; public string[] dialogs; }

public class MechanicContentLoader : MonoBehaviour
{
    public static MechanicContentLoader Instance;
    private Dictionary<string, string[]> adviceCache = new Dictionary<string, string[]>();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); StartCoroutine(LoadData()); }
        else Destroy(gameObject);
    }

    IEnumerator LoadData()
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "CurrMiGame.json");
        using (UnityWebRequest webRequest = UnityWebRequest.Get(path))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                MechanicData data = JsonUtility.FromJson<MechanicData>(webRequest.downloadHandler.text);
                foreach (var item in data.advices) adviceCache[item.minigameID] = item.dialogs;
            }
        }
    }

    public string[] GetDialogs(string id) => (id != null && adviceCache.ContainsKey(id)) ? adviceCache[id] : null;
}