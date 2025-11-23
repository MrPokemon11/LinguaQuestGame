// Assets/Scripts/FX/ExplosionFXController.cs
using UnityEngine;

public class ExplosionFXController : MonoBehaviour
{
    [Header("Optional auto-despawn")]
    [SerializeField] private float killAfter = 0.6f;

    ParticleSystem[] systems;

    void Awake()
    {
        systems = GetComponentsInChildren<ParticleSystem>(true);
    }

    public void Play(Color baseColor)
    {
        // push color to each child system
        foreach (var ps in systems)
        {
            var main = ps.main;
            // If Color over Lifetime is used, StartColor becomes the base tint
            main.startColor = baseColor;
            ps.Clear(true);
            ps.Play(true);
        }
        if (killAfter > 0f) Destroy(gameObject, killAfter);
    }
}
