using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BeatFeedback
{
    Perfect,
    Great,
    Good,
    Miss
}

public class RhythmManager : MonoBehaviour
{
    [SerializeField] private GameObject mainPoint;
    [SerializeField] private GameObject beatPointPrefab;
    [SerializeField] private GameObject maliciousBeatPrefab;
    [SerializeField] private Transform poolStartPosition;
    [SerializeField] private Transform poolEndPosition;
    [SerializeField] private Transform missPosition;
    [SerializeField] private RhythmConfig config;
    [SerializeField] private InputActionAsset inputActions;

    [Tooltip("Chance (0-1) of spawning a malicious beat between regular beats")] [SerializeField]
    private float maliciousBeatChance = 0.1f;

    [Tooltip("How long the object stays centered on the main point before returning")] [SerializeField]
    private float holdTime = 0.05f;

    [Tooltip("Duration of movement from pool start to main point")] [SerializeField]
    private float moveToMainDuration = 0.5f;

    [Tooltip("Duration of movement from main point to pool end position")] [SerializeField]
    private float moveToEndDuration = 0.4f;

    [Tooltip("Beat input tolerance as a percentage (20 = 20% early or late)")] [SerializeField]
    private float inputTolerancePercent = 20f;

    [Header("Feedback Tolerance Thresholds")]
    [Tooltip("Tolerance threshold for Perfect feedback (in percentage)")] [SerializeField]
    private float perfectThreshold = 2.5f;

    [Tooltip("Tolerance threshold for Great feedback (in percentage)")] [SerializeField]
    private float greatThreshold = 5f;

    [Tooltip("Tolerance threshold for Good feedback (in percentage)")] [SerializeField]
    private float goodThreshold = 10f;

    // internal
    private Queue<GameObject> availablePool = new Queue<GameObject>();
    private Queue<GameObject> maliciousPool = new Queue<GameObject>();
    private Dictionary<int, (float beatTime, bool inputHandled, bool isMalicious)> activeBeatData = new Dictionary<int, (float, bool, bool)>();
    private Dictionary<int, GameObject> activeBeatObjects = new Dictionary<int, GameObject>();
    private float startTime;
    private int beatIndex = 0;
    private int successCount = 0;
    private int failureCount = 0;
    private InputAction pressAction;
    private float moveStartThreshold = 2f; // Beats start moving 2 seconds before their beat time

    private void Start()
    {
        if (mainPoint == null)
        {
            Debug.LogError("RhythmManager: mainPoint is not assigned.");
            enabled = false;
            return;
        }

        if (poolStartPosition == null)
        {
            Debug.LogError("RhythmManager: poolStartPosition is not assigned.");
            enabled = false;
            return;
        }

        if (poolEndPosition == null)
        {
            Debug.LogError("RhythmManager: poolEndPosition is not assigned.");
            enabled = false;
            return;
        }

        if (beatPointPrefab == null)
        {
            Debug.LogError("RhythmManager: beatPointPrefab is not assigned.");
            enabled = false;
            return;
        }

        if (maliciousBeatPrefab == null)
        {
            Debug.LogError("RhythmManager: maliciousBeatPrefab is not assigned.");
            enabled = false;
            return;
        }

        if (config == null || config.beats == null || config.beats.Count == 0)
        {
            Debug.LogWarning("RhythmManager: no beats configured.");
            enabled = false;
            return;
        }

        if (inputActions == null)
        {
            Debug.LogError("RhythmManager: inputActions is not assigned.");
            enabled = false;
            return;
        }

        pressAction = inputActions.FindAction("Press");
        if (pressAction == null)
        {
            Debug.LogError("RhythmManager: 'Press' action not found in InputActionAsset.");
            enabled = false;
            return;
        }

        pressAction.Enable();

        startTime = Time.time;
        activeBeatData.Clear();
        activeBeatObjects.Clear();
        StartCoroutine(ProcessBeats());
        StartCoroutine(MonitorInputs());
        StartCoroutine(UpdateBeatPositions());
    }

    private IEnumerator ProcessBeats()
    {
        while (beatIndex < config.beats.Count)
        {
            float targetBeat = config.beats[beatIndex];
            float elapsed = Time.time - startTime;
            float timeUntilBeat = targetBeat - elapsed;

            // Spawn beat when it's close enough to appear on screen (moveStartThreshold + some buffer)
            if (timeUntilBeat <= moveStartThreshold + 0.5f)
            {
                // get an object from the pool
                GameObject obj = GetPooledObject(false);

                if (obj == null)
                {
                    Debug.LogWarning("RhythmManager: failed to get pooled object.");
                    beatIndex++;
                    yield return null;
                    continue;
                }

                int currentBeatIndex = beatIndex;
                obj.transform.position = poolStartPosition.position;
                activeBeatData[currentBeatIndex] = (targetBeat, false, false);
                activeBeatObjects[currentBeatIndex] = obj;

                // Randomly spawn a malicious beat after this beat
                if (Random.value < maliciousBeatChance && beatIndex + 1 < config.beats.Count)
                {
                    float nextBeat = config.beats[beatIndex + 1];
                    float maliciousBeatTime = (targetBeat + nextBeat) / 2f;

                    GameObject maliciousObj = GetPooledObject(true);
                    if (maliciousObj != null)
                    {
                        maliciousObj.transform.position = poolStartPosition.position;
                        int maliciousBeatIndex = beatIndex + 10000;
                        activeBeatData[maliciousBeatIndex] = (maliciousBeatTime, false, true);
                        activeBeatObjects[maliciousBeatIndex] = maliciousObj;
                    }
                }

                beatIndex++;
            }

            yield return null;
        }
    }

    private IEnumerator UpdateBeatPositions()
    {
        while (true)
        {
            float currentTime = Time.time - startTime;
            Vector3 startPos = poolStartPosition.position;
            Vector3 mainPos = mainPoint.transform.position;
            Vector3 endPos = poolEndPosition.position;

            List<int> beatsToRemove = new List<int>();

            foreach (var kvp in activeBeatObjects)
            {
                int beatIdx = kvp.Key;
                GameObject obj = kvp.Value;

                if (obj == null || !activeBeatData.ContainsKey(beatIdx))
                {
                    beatsToRemove.Add(beatIdx);
                    continue;
                }

                float beatTime = activeBeatData[beatIdx].beatTime;
                float timeRemaining = beatTime - currentTime;

                // Phase 1: Stay at start if more than moveStartThreshold away
                if (timeRemaining > moveStartThreshold)
                {
                    obj.transform.position = startPos;
                }
                // Phase 2: Move from start to main point (smooth continuous movement)
                else if (timeRemaining > 0)
                {
                    float moveProgress = 1f - (timeRemaining / moveStartThreshold);
                    obj.transform.position = Vector3.Lerp(startPos, mainPos, moveProgress);
                }
                // Phase 3: Hold at main point
                else if (timeRemaining > -holdTime)
                {
                    obj.transform.position = mainPos;
                }
                // Phase 4: Move from main to end (smooth continuous movement)
                else if (timeRemaining > -holdTime - moveToEndDuration)
                {
                    float timeIntoExit = -holdTime - timeRemaining;
                    float moveProgress = timeIntoExit / moveToEndDuration;
                    obj.transform.position = Vector3.Lerp(mainPos, endPos, moveProgress);
                }
                // Phase 5: Beat has passed - mark for cleanup
                else
                {
                    var beatData = activeBeatData[beatIdx];
                    if (!beatData.inputHandled)
                    {
                        failureCount++;

                        string beatType = beatData.isMalicious ? "Malicious" : "Regular";
                        Debug.Log($"Beat {beatIdx} MISS - No input ({beatType})");

                        if (!beatData.isMalicious)
                            healthSystem.MissedTarget();
                    }

                    // Return to pool
                    obj.transform.position = startPos;
                    obj.SetActive(false);

                    if (beatData.isMalicious)
                        maliciousPool.Enqueue(obj);
                    else
                        availablePool.Enqueue(obj);

                    beatsToRemove.Add(beatIdx);
                    activeBeatData.Remove(beatIdx);
                }
            }

            // Clean up removed beats
            foreach (int beatIdx in beatsToRemove)
            {
                activeBeatObjects.Remove(beatIdx);
            }

            yield return null;
        }
    }

    private GameObject GetPooledObject(bool isMalicious)
    {
        GameObject obj = null;
        Queue<GameObject> pool = isMalicious ? maliciousPool : availablePool;
        GameObject prefab = isMalicious ? maliciousBeatPrefab : beatPointPrefab;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = Instantiate(prefab, poolStartPosition.position, Quaternion.identity);
        }

        if (obj != null)
        {
            obj.SetActive(true);
        }

        return obj;
    }

    private IEnumerator MonitorInputs()
    {
        while (beatIndex < config.beats.Count || activeBeatData.Count > 0)
        {
            if (pressAction.WasPressedThisFrame())
            {
                float currentTime = Time.time - startTime;
                CheckBeatInput(currentTime);
            }

            yield return null;
        }
    }

    private void OnDisable()
    {
        if (pressAction != null)
        {
            pressAction.Disable();
        }
    }

    private void CheckBeatInput(float inputTime)
    {
        // Find the closest active beat that is early and within the tolerance window
        int closestBeatIndex = -1;
        float closestDistance = float.MaxValue;

        foreach (var kvp in activeBeatData)
        {
            int beatIdx = kvp.Key;
            float beatTime = kvp.Value.beatTime;
            bool alreadyHandled = kvp.Value.inputHandled;

            if (alreadyHandled)
                continue;

            float distance = Mathf.Abs(inputTime - beatTime);
            bool isEarly = inputTime < beatTime;

            // Calculate tolerance as a percentage of the beat time (or use a minimum window)
            float toleranceWindow = beatTime * (inputTolerancePercent / 100f);
            toleranceWindow = Mathf.Max(toleranceWindow, 0.1f); // Minimum 0.1 second window

            // Only consider early presses within tolerance
            if (isEarly && distance <= toleranceWindow && distance < closestDistance)
            {
                closestDistance = distance;
                closestBeatIndex = beatIdx;
            }
        }

        if (closestBeatIndex >= 0)
        {
            var beatData = activeBeatData[closestBeatIndex];
            bool isMalicious = beatData.isMalicious;

            // Check if player hit a malicious beat
            if (isMalicious)
            {
                failureCount++;
                activeBeatData[closestBeatIndex] = (beatData.beatTime, true, true);
                Debug.Log($"Beat {closestBeatIndex} MISS - Hit malicious beat!");
                healthSystem.MissedTarget();
            }
            else
            {
                // Valid input on regular beat
                successCount++;
                activeBeatData[closestBeatIndex] = (beatData.beatTime, true, false);

                float beatTime = beatData.beatTime;
                float errorPercentage = (closestDistance / beatTime) * 100f;

                BeatFeedback feedback = GetFeedback(errorPercentage, true);

                Debug.Log($"Beat {closestBeatIndex} SUCCESS - {feedback} - Input detected with {closestDistance:F3}s error ({errorPercentage:F2}%). Success: {successCount}, Failed: {failureCount}");
            }
        }
        else
        {
            // No beat within early tolerance - find closest beat and mark as Miss
            int closestMissBeatIndex = -1;
            float closestMissDistance = float.MaxValue;

            foreach (var kvp in activeBeatData)
            {
                int beatIdx = kvp.Key;
                float beatTime = kvp.Value.beatTime;
                bool alreadyHandled = kvp.Value.inputHandled;

                if (alreadyHandled)
                    continue;

                float distance = Mathf.Abs(inputTime - beatTime);

                if (distance < closestMissDistance)
                {
                    closestMissDistance = distance;
                    closestMissBeatIndex = beatIdx;
                }
            }

            if (closestMissBeatIndex >= 0)
            {
                failureCount++;

                var beatData = activeBeatData[closestMissBeatIndex];
                activeBeatData[closestMissBeatIndex] = (beatData.beatTime, true, beatData.isMalicious);

                bool isLate = inputTime >= beatData.beatTime;
                string missReason = isLate ? "Late press" : "Input too far from beat";

                Debug.Log($"Beat {closestMissBeatIndex} MISS - {missReason}");

                if (!beatData.isMalicious)
                    healthSystem.MissedTarget();
            }
        }
    }
    public ObjectsMove objectsMove;
    public HealthSystem healthSystem;
    public PerfectVFX perfectVfx;
    private BeatFeedback GetFeedback(float errorPercentage, bool isEarly)
    {
        // Late presses are always miss
        if (!isEarly)
        {
            healthSystem.MissedTarget();
            return BeatFeedback.Miss;
        }
        objectsMove.MoveTrail(2f, 0.2f);
        if (errorPercentage <= perfectThreshold)
        {
            perfectVfx.PlayPerfect();
            return BeatFeedback.Perfect;
        }
        else if (errorPercentage <= greatThreshold)
        {
            perfectVfx.PlayGreat();
            return BeatFeedback.Great;
        }
        else if (errorPercentage <= goodThreshold)
        {
            perfectVfx.PlayGood();
            return BeatFeedback.Good;
        }

        healthSystem.MissedTarget();
        return BeatFeedback.Miss;
    }
}
