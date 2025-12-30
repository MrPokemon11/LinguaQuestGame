using UnityEngine;

public class SnowFollowCamera2D : MonoBehaviour
{
    [Header("Camera")]
    public Camera cam;

    [Header("Particle Systems (Far/Mid/Near)")]
    public ParticleSystem farPS;
    public ParticleSystem midPS;
    public ParticleSystem nearPS;

    [Header("PPU / Pixel Sizes")]
    public int pixelsPerUnit = 4;

    [Header("Coverage Padding")]
    [Range(0f, 1f)] public float paddingPercent = 0.30f; // 30% off-screen margin
    public float spawnHeightAboveView = 2.0f;           // spawn slightly above view (world units)

    [Header("Global Wind (world units/sec)")]
    public float windX = 0.15f; // positive = drift right, negative = left

    void Reset()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null || !cam.orthographic) return;

        // Follow camera position (2D)
        Vector3 p = cam.transform.position;
        p.z = transform.position.z;
        transform.position = p;

        // Visible world size
        float viewH = 2f * cam.orthographicSize;        // = 10 for ortho size 5
        float viewW = viewH * cam.aspect;

        // Add padding so flakes spawn off-screen
        float padW = viewW * paddingPercent;
        float padH = viewH * paddingPercent;

        float boxW = viewW + padW * 2f;
        float boxH = viewH + padH * 2f;

        // Pixel unit
        float px = 1f / Mathf.Max(1, pixelsPerUnit);    // 0.0625 if PPU=16

        ConfigureLayer(farPS, boxW, boxH, spawnHeightAboveView,
            rate: 45f, speed: 0.6f, sizeMin: 1f * px, sizeMax: 1f * px,
            noiseStrength: 0.08f);

        ConfigureLayer(midPS, boxW, boxH, spawnHeightAboveView,
            rate: 110f, speed: 1.0f, sizeMin: 1f * px, sizeMax: 2f * px,
            noiseStrength: 0.14f);

        ConfigureLayer(nearPS, boxW, boxH, spawnHeightAboveView,
            rate: 170f, speed: 1.6f, sizeMin: 2f * px, sizeMax: 3f * px,
            noiseStrength: 0.22f);
    }

    private void ConfigureLayer(
        ParticleSystem ps,
        float boxW, float boxH, float heightAbove,
        float rate, float speed, float sizeMin, float sizeMax,
        float noiseStrength)
    {
        if (ps == null) return;

        var main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startSpeed = speed;
        main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
        main.startLifetime = Mathf.Clamp(boxH / Mathf.Max(0.01f, speed), 3f, 14f);

        var emission = ps.emission;
        emission.rateOverTime = rate;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(boxW, boxH, 0.1f);
        shape.position = new Vector3(0f, heightAbove, 0f); // spawn slightly above camera center

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.x = windX;      // wind drift
        vel.y = 0f;         // use startSpeed downward direction via gravity or rotation

        // Instead of gravity, orient particles to fall down by setting Gravity Modifier
        main.gravityModifier = 0.3f;

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = noiseStrength;
        noise.frequency = 0.35f;
        noise.scrollSpeed = 0.2f;

        // Ensure stable max particles
        main.maxParticles = Mathf.Max(main.maxParticles, 2000);
    }
}
