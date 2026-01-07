using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StepSoundManager : MonoBehaviour
{
    [System.Serializable]
    public class GroundTypeSound
    {
        public string groundTypeName;
        public AudioClip[] stepSounds;
    }

    public List<GroundTypeSound> groundSounds;
    public AudioSource audioSource;

    public Tilemap[] grassTilemaps;
    public Tilemap[] stoneTilemaps;
    public Tilemap[] woodTilemaps;

    public void PlayStepSound(Vector3 playerWorldPos)
    {
        string groundType = null;

        foreach (Tilemap grassTilemap in grassTilemaps)
        {
            Vector3Int gridPos = grassTilemap.WorldToCell(playerWorldPos);
            //Debug.Log("Player grid position: " + gridPos);
            groundType = DetectGroundType(gridPos);

            if (!string.IsNullOrEmpty(groundType))
                break;
        }

        if (!string.IsNullOrEmpty(groundType))
        {
            GroundTypeSound soundSet = groundSounds.Find(g => g.groundTypeName == groundType);
            ///Debug.Log("Detected ground type: " + groundType);
            if (soundSet != null && soundSet.stepSounds.Length > 0)
            {
                //Debug.Log("Playing step sound for ground type: " + groundType);
                AudioClip clip = soundSet.stepSounds[Random.Range(0, soundSet.stepSounds.Length)];
                audioSource.PlayOneShot(clip);
            }
        }
    }

    private string DetectGroundType(Vector3Int gridPos)
    {
        foreach (var map in grassTilemaps)
            if (map != null && map.HasTile(gridPos)) return "Grass";

        foreach (var map in stoneTilemaps)
            if (map != null && map.HasTile(gridPos)) return "Stone";

        foreach (var map in woodTilemaps)
            if (map != null && map.HasTile(gridPos)) return "Wood";

        return null; // No known ground type
    }

}
