using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordSpawnerPoint : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject wordBlockPrefab;

    [Header("Spawn Point (scene transform)")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Vertical spacing between spawned blocks to prevent overlap")]
    [SerializeField] private float verticalSpacing = 1.5f;

    [Header("Initial Launch (slower, goes left)")]
    [Tooltip("Negative X speeds so blocks start moving left.")]
    [SerializeField] private Vector2 leftSpeedRange = new Vector2(-1.5f, -0.6f);

    [Tooltip("Much smaller upward lift so blocks barely drift vertically.")]
    [SerializeField] private Vector2 upSpeedRange = new Vector2(0.05f, 0.15f);

    [Header("Cadence")]
    [SerializeField] private float spawnInterval = 0.5f;

    private Queue<BlockData> _spawnQueue = new Queue<BlockData>();
    private SentenceData _currentSentence;
    private float _lastSpawnY;
    private bool _isSpawning = false;

    // Helper class to store block data
    [System.Serializable]
    private class BlockData
    {
        public string word;
        public string label;
        public bool isCorrect;

        public BlockData(string word, string label, bool isCorrect)
        {
            this.word = word;
            this.label = label;
            this.isCorrect = isCorrect;
        }
    }

    void Start()
    {
        _lastSpawnY = spawnPoint ? spawnPoint.position.y : 0f;
        Debug.Log("[WordSpawner] Started, waiting for first sentence...");
    }

    public void OnNewSentence(object obj)
    {
        _currentSentence = obj as SentenceData;

        if (_currentSentence != null && _currentSentence.entries != null && _currentSentence.entries.Count > 0)
        {
            Debug.Log($"[WordSpawner] ✓ Received new sentence: {_currentSentence.sentence} ({_currentSentence.entries.Count} entries)");

            // Build the spawn queue
            BuildSpawnQueue();

            // Start spawning if not already spawning
            if (!_isSpawning)
            {
                StartCoroutine(SpawnLoop());
            }
        }
        else
        {
            Debug.LogWarning("[WordSpawner] ⚠️ Received sentence with no entries!");
        }
    }

    private void BuildSpawnQueue()
    {
        // Clear existing queue
        _spawnQueue.Clear();

        // Create a list of all blocks
        List<BlockData> blocks = new List<BlockData>();
        foreach (var entry in _currentSentence.entries)
        {
            blocks.Add(new BlockData(entry.word, entry.shownLabel, entry.isLabelCorrect));
        }

        // Shuffle the list randomly
        for (int i = blocks.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            BlockData temp = blocks[i];
            blocks[i] = blocks[randomIndex];
            blocks[randomIndex] = temp;
        }

        // Add shuffled blocks to queue
        foreach (var block in blocks)
        {
            _spawnQueue.Enqueue(block);
        }

        Debug.Log($"[WordSpawner] Built spawn queue with {_spawnQueue.Count} blocks in random order");
    }

    private IEnumerator SpawnLoop()
    {
        _isSpawning = true;
        var wait = new WaitForSeconds(spawnInterval);

        while (true)
        {
            // Wait until game has started
            if (SwordWaveManager.Instance != null && SwordWaveManager.Instance.IsGameStarted())
            {
                // Check if queue has blocks to spawn
                if (_spawnQueue.Count > 0)
                {
                    BlockData blockToSpawn = _spawnQueue.Dequeue();
                    SpawnBlock(blockToSpawn.word, blockToSpawn.label, blockToSpawn.isCorrect);

                    Debug.Log($"[WordSpawner] Spawned from queue. Remaining: {_spawnQueue.Count}");
                }
                else
                {
                    // Queue is empty - check if all blocks are cleared
                    if (AreAllBlocksGone())
                    {
                        Debug.Log("[WordSpawner] Queue empty and all blocks cleared. Notifying manager...");
                        SwordWaveManager.Instance?.OnAllBlocksCleared();

                        // Wait for next sentence
                        yield return new WaitUntil(() => _spawnQueue.Count > 0);
                    }
                }
            }

            yield return wait;
        }
    }

    private bool AreAllBlocksGone()
    {
        // Check if any WordBlocks still exist in the scene
        WordBlock[] remainingBlocks = FindObjectsOfType<WordBlock>();
        return remainingBlocks.Length == 0;
    }

    public void RequeueBlock(string word, string label, bool isCorrect)
    {
        // Add the incorrectly cut block back to the queue
        _spawnQueue.Enqueue(new BlockData(word, label, isCorrect));
        Debug.Log($"[WordSpawner] ⚠️ Block requeued: {word} ({label}). Queue size: {_spawnQueue.Count}");
    }

    private void SpawnBlock(string word, string label, bool isCorrect)
    {
        if (wordBlockPrefab == null)
        {
            Debug.LogError("[WordSpawner] wordBlockPrefab is not assigned!");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("[WordSpawner] spawnPoint is not assigned!");
            return;
        }

        Vector3 basePos = spawnPoint.position;
        _lastSpawnY += verticalSpacing;

        var pos = new Vector3(
            basePos.x,
            _lastSpawnY,
            0f
        );

        // Reset Y position if it goes too high
        if (_lastSpawnY > basePos.y + 10f)
        {
            _lastSpawnY = basePos.y;
        }

        var wb = Instantiate(wordBlockPrefab, pos, Quaternion.identity);

        var wordBlock = wb.GetComponent<WordBlock>();
        if (wordBlock != null)
        {
            wordBlock.Initialize(word, label, isCorrect);
            Debug.Log($"[WordSpawner] Spawned: {word} ({label}) - Correct: {isCorrect}");
        }
        else
        {
            Debug.LogError("[WordSpawner] wordBlockPrefab is missing WordBlock component!");
            Destroy(wb);
            return;
        }

        var rb = wb.GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.freezeRotation = true;
            rb.angularVelocity = 0f;

            float vx = Random.Range(leftSpeedRange.x, leftSpeedRange.y);
            float vy = Random.Range(upSpeedRange.x, upSpeedRange.y);
            rb.linearVelocity = new Vector2(vx, vy);
        }
        else
        {
            Debug.LogWarning("[WordSpawner] wordBlockPrefab is missing Rigidbody2D component!");
        }
    }

    public int GetQueueCount()
    {
        return _spawnQueue.Count;
    }

    [ContextMenu("Debug: Print Queue")]
    private void DebugPrintQueue()
    {
        Debug.Log($"[WordSpawner] Current queue size: {_spawnQueue.Count}");
        int index = 0;
        foreach (var block in _spawnQueue)
        {
            Debug.Log($"  [{index}] {block.word} ({block.label}) - Correct: {block.isCorrect}");
            index++;
        }
    }
}