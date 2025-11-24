using UnityEngine;
using TMPro;
using System.Collections;

[DisallowMultipleComponent]
public class WordBlock : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private string wordText = "run";
    [SerializeField] private string shownLabel = "Noun";
    [SerializeField] private bool isLabelCorrect = true;

    [Header("Points / Timing")]
    [SerializeField] private int pointsOnCorrectSlice = 100;
    [SerializeField] private int pointsOnWrongSlice = -50;
    [SerializeField] private int pointsOnMissed = -30;
    [SerializeField] private float destroyDelay = 0.05f;

    [Header("Lifetime & Fade")]
    [Tooltip("How long the block exists before starting to fade")]
    [SerializeField] private float lifetime = 8f;
    [Tooltip("How long the fade-out animation takes")]
    [SerializeField] private float fadeOutDuration = 2f;
    private float _timer;
    private bool _isFading;

    [Header("Boundaries")]
    [Tooltip("World space boundaries (min/max X and Y)")]
    [SerializeField] private Vector2 minBounds = new Vector2(-10f, -6f);
    [SerializeField] private Vector2 maxBounds = new Vector2(10f, 6f);
    [Tooltip("How much to bounce back when hitting boundary")]
    [SerializeField] private float bounceForce = 0.5f;

    [Header("Regeneration")]
    [Tooltip("Should this block regenerate if hit incorrectly?")]
    [SerializeField] private bool canRegenerate = true;

    [Header("Refs")]
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private TextMeshPro wordTMP;
    [SerializeField] private TextMeshPro labelTMP;
    [SerializeField] private GameObject explosionFXPrefab; // Changed from ParticleSystem to GameObject

    [Header("Colors")]
    [SerializeField] private Color neutralColor = Color.white;
    [SerializeField] private Color goodColor = new Color(0.2f, 1f, 0.4f);
    [SerializeField] private Color badColor = new Color(1f, 0.3f, 0.3f);

    [Header("Explosion")]
    [SerializeField] private float explosionForce = 5f;
    [SerializeField] private float explosionRadius = 2f;

    private bool _hasExploded;
    private Rigidbody2D _rb;
    private Color _originalBackgroundColor;
    private Color _originalWordColor;
    private Color _originalLabelColor;

    // Store original data for regeneration
    private string _originalWord;
    private string _originalLabel;
    private bool _originalIsCorrect;

    void Reset()
    {
        if (!background) background = GetComponentInChildren<SpriteRenderer>();
        if (!wordTMP) wordTMP = transform.Find("Text: Word")?.GetComponent<TextMeshPro>();
        if (!labelTMP) labelTMP = transform.Find("Text: Label")?.GetComponent<TextMeshPro>();
    }

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_rb)
        {
            _rb.freezeRotation = true;
        }
    }

    void OnEnable()
    {
        _hasExploded = false;
        _isFading = false;
        _timer = 0f;
        ApplyTexts();
        SetNeutral();
        EnableColliders(true);

        if (background) _originalBackgroundColor = background.color;
        if (wordTMP) _originalWordColor = wordTMP.color;
        if (labelTMP) _originalLabelColor = labelTMP.color;

        transform.rotation = Quaternion.identity;
    }

    void Update()
    {
        if (_hasExploded) return;

        _timer += Time.deltaTime;

        if (_timer >= lifetime && !_isFading)
        {
            _isFading = true;
            StartCoroutine(FadeOutAndDestroy());
        }

        EnforceBoundaries();
    }

    void LateUpdate()
    {
        if (transform.rotation != Quaternion.identity)
        {
            transform.rotation = Quaternion.identity;
        }
    }

    private void EnforceBoundaries()
    {
        if (_rb == null) return;

        Vector2 pos = transform.position;
        Vector2 velocity = _rb.linearVelocity;
        bool hitBoundary = false;

        if (pos.x < minBounds.x)
        {
            pos.x = minBounds.x;
            velocity.x = Mathf.Abs(velocity.x) * bounceForce;
            hitBoundary = true;
        }
        else if (pos.x > maxBounds.x)
        {
            pos.x = maxBounds.x;
            velocity.x = -Mathf.Abs(velocity.x) * bounceForce;
            hitBoundary = true;
        }

        if (pos.y < minBounds.y)
        {
            pos.y = minBounds.y;
            velocity.y = Mathf.Abs(velocity.y) * bounceForce;
            hitBoundary = true;
        }
        else if (pos.y > maxBounds.y)
        {
            pos.y = maxBounds.y;
            velocity.y = -Mathf.Abs(velocity.y) * bounceForce;
            hitBoundary = true;
        }

        if (hitBoundary)
        {
            transform.position = pos;
            _rb.linearVelocity = velocity;
        }
    }

    public void Initialize(string word, string shownLabel, bool isLabelCorrect)
    {
        this.wordText = word;
        this.shownLabel = shownLabel;
        this.isLabelCorrect = isLabelCorrect;

        _originalWord = word;
        _originalLabel = shownLabel;
        _originalIsCorrect = isLabelCorrect;

        ApplyTexts();
        SetNeutral();
        EnableColliders(true);

        _timer = 0f;
        _isFading = false;

        transform.rotation = Quaternion.identity;
        if (_rb)
        {
            _rb.angularVelocity = 0f;
            _rb.freezeRotation = true;
        }
    }

    private void ApplyTexts()
    {
        if (wordTMP) wordTMP.text = wordText;
        if (labelTMP) labelTMP.text = shownLabel;
    }

    private void SetNeutral()
    {
        if (background) background.color = neutralColor;
        if (wordTMP) wordTMP.color = Color.white;
        if (labelTMP) labelTMP.color = new Color(0.9f, 0.9f, 0.9f);
    }

    private void EnableColliders(bool enabled)
    {
        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = enabled;
    }

    public int Explode(SwordWave wave = null)
    {
        if (_hasExploded) return 0;
        _hasExploded = true;

        StopAllCoroutines();

        bool correctPlay = false;
        int scoreToReturn = 0;

        if (wave != null)
        {
            bool waveMatchesBlock = (wave.isCorrectWave == isLabelCorrect);
            correctPlay = waveMatchesBlock;

            scoreToReturn = correctPlay ? pointsOnCorrectSlice : pointsOnWrongSlice;

            Debug.Log($"[WordBlock] Wave type: {(wave.isCorrectWave ? "Correct" : "Incorrect")}, " +
                     $"Block label: {(isLabelCorrect ? "Correct" : "Incorrect")}, " +
                     $"Match: {waveMatchesBlock}, Score: {scoreToReturn}");

            if (!correctPlay)
            {
                // Incorrectly cut - requeue the block
                SwordWaveManager.Instance?.OnBlockClearedIncorrectly(_originalWord, _originalLabel, _originalIsCorrect);
            }
            else
            {
                // Correctly cut
                SwordWaveManager.Instance?.OnBlockClearedCorrectly();
            }
        }
        else
        {
            correctPlay = !isLabelCorrect;
            scoreToReturn = correctPlay ? pointsOnCorrectSlice : pointsOnWrongSlice;
        }

        // Spawn explosion GameObject with ExplosionFXController
        if (explosionFXPrefab != null)
        {
            GameObject explosionObj = Instantiate(explosionFXPrefab, transform.position, Quaternion.identity);
            ExplosionFXController explosionCtrl = explosionObj.GetComponent<ExplosionFXController>();
            if (explosionCtrl != null)
            {
                explosionCtrl.Play(correctPlay ? goodColor : badColor);
            }
        }

        if (background) background.color = correctPlay ? goodColor : badColor;

        // Apply explosion force to nearby blocks
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var col in nearbyColliders)
        {
            if (col.gameObject == gameObject) continue;

            Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = (col.transform.position - transform.position).normalized;
                float distance = Vector2.Distance(transform.position, col.transform.position);
                float forceMagnitude = explosionForce * (1f - distance / explosionRadius);
                rb.AddForce(direction * forceMagnitude, ForceMode2D.Impulse);
            }
        }

        EnableColliders(false);
        StartCoroutine(RemoveAfterDelay());

        return scoreToReturn;
    }

    private IEnumerator FadeOutAndDestroy()
    {
        float elapsed = 0f;
        EnableColliders(false);

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / fadeOutDuration);

            if (background)
            {
                Color c = _originalBackgroundColor;
                c.a = alpha;
                background.color = c;
            }

            if (wordTMP)
            {
                Color c = _originalWordColor;
                c.a = alpha;
                wordTMP.color = c;
            }

            if (labelTMP)
            {
                Color c = _originalLabelColor;
                c.a = alpha;
                labelTMP.color = c;
            }

            yield return null;
        }

        if (!_hasExploded)
        {
            SwordWaveManager.TryAddScore(pointsOnMissed);

            if (canRegenerate)
            {
                SwordWaveManager.Instance?.OnBlockClearedIncorrectly(_originalWord, _originalLabel, _originalIsCorrect);
            }

            Debug.Log($"[WordBlock] Missed: {wordText} ({pointsOnMissed} score)");
        }

        Destroy(gameObject);
    }

    private IEnumerator RemoveAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isLabelCorrect ? new Color(0f, 0.8f, 1f, 0.35f) : new Color(1f, 0.5f, 0f, 0.35f);
        Gizmos.DrawSphere(transform.position, 0.15f);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        Gizmos.color = Color.yellow;
        Vector3 bottomLeft = new Vector3(minBounds.x, minBounds.y, 0);
        Vector3 topLeft = new Vector3(minBounds.x, maxBounds.y, 0);
        Vector3 topRight = new Vector3(maxBounds.x, maxBounds.y, 0);
        Vector3 bottomRight = new Vector3(maxBounds.x, minBounds.y, 0);

        Gizmos.DrawLine(bottomLeft, topLeft);
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
    }
#endif
}