using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BeatFeedback
{
    Perfect,
    Great,
    Good,
    Early,
    Miss
}

public class RhythmManager : MonoBehaviour
{
    [SerializeField] private GameObject mainPoint;
    [SerializeField] private GameObject beatPointPrefab;
    [SerializeField] private Transform poolStartPosition;
    [SerializeField] private Transform poolEndPosition;
    [SerializeField] private RhythmConfig config;
    [SerializeField] private InputActionAsset inputActions;

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
    private Dictionary<int, (float beatTime, bool inputHandled)> activeBeatData = new Dictionary<int, (float, bool)>();
    private float startTime;
    private int beatIndex = 0;
    private int successCount = 0;
    private int failureCount = 0;
    private InputAction pressAction;

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
        StartCoroutine(ProcessBeats());
        StartCoroutine(MonitorInputs());
    }

    private IEnumerator ProcessBeats()
    {
        while (beatIndex < config.beats.Count)
        {
            float targetBeat = config.beats[beatIndex];
            float elapsed = Time.time - startTime;
            float timeUntilBeat = targetBeat - elapsed;

            // get an object from the pool
            GameObject obj = GetPooledObject();

            if (obj == null)
            {
                Debug.LogWarning("RhythmManager: failed to get pooled object.");
                beatIndex++;
                yield return null;
                continue;
            }

            int currentBeatIndex = beatIndex;
            activeBeatData[currentBeatIndex] = (targetBeat, false);

            // start coroutine to move it so that it arrives exactly at targetBeat
            StartCoroutine(MoveToMainAndReturn(obj, Mathf.Max(0f, timeUntilBeat), currentBeatIndex));

            beatIndex++;
            yield return null;
        }
    }

    private IEnumerator MoveToMainAndReturn(GameObject obj, float timeToArrive, int beatIndex)
    {
        Vector3 startPos = poolStartPosition.position;
        Vector3 mainPos = mainPoint.transform.position;
        Vector3 endPos = poolEndPosition.position;

        // Move from pool start to main point
        if (timeToArrive <= 0f)
        {
            obj.transform.position = mainPos;
        }
        else
        {
            float t = 0f;
            while (t < timeToArrive)
            {
                if (obj == null) yield break;
                t += Time.deltaTime;
                float frac = Mathf.Clamp01(t / timeToArrive);
                obj.transform.position = Vector3.Lerp(startPos, mainPos, frac);
                yield return null;
            }
            obj.transform.position = mainPos;
        }

        // hold briefly on the main point
        float hold = 0f;
        while (hold < holdTime)
        {
            if (obj == null) yield break;
            hold += Time.deltaTime;
            yield return null;
        }

        // move from main point to end position
        float mt = 0f;
        while (mt < moveToEndDuration)
        {
            if (obj == null) yield break;
            mt += Time.deltaTime;
            float frac = Mathf.Clamp01(mt / moveToEndDuration);
            obj.transform.position = Vector3.Lerp(mainPos, endPos, frac);
            yield return null;
        }

        obj.transform.position = endPos;

        // Deactivate, teleport to start, and return to pool
        if (obj != null)
        {
            obj.transform.position = startPos;
            obj.SetActive(false);
            availablePool.Enqueue(obj);
        }

        // Clean up beat data
        if (activeBeatData.ContainsKey(beatIndex))
        {
            var beatData = activeBeatData[beatIndex];
            if (!beatData.inputHandled)
            {
                failureCount++;

                Debug.Log($"Beat {beatIndex} MISS - No input");

                healthSystem.MissedTarget();
            }
            activeBeatData.Remove(beatIndex);
        }
    }

    private GameObject GetPooledObject()
    {
        GameObject obj = null;

        if (availablePool.Count > 0)
        {
            obj = availablePool.Dequeue();
        }
        else
        {
            obj = Instantiate(beatPointPrefab, poolStartPosition.position, Quaternion.identity);
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
            // Valid input within tolerance and early
            successCount++;
            var beatData = activeBeatData[closestBeatIndex];
            activeBeatData[closestBeatIndex] = (beatData.beatTime, true);

            float beatTime = beatData.beatTime;
            float errorPercentage = (closestDistance / beatTime) * 100f;

            BeatFeedback feedback = GetFeedback(errorPercentage, true);

            Debug.Log($"Beat {closestBeatIndex} SUCCESS - {feedback} - Input detected with {closestDistance:F3}s error ({errorPercentage:F2}%). Success: {successCount}, Failed: {failureCount}");
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
                activeBeatData[closestMissBeatIndex] = (beatData.beatTime, true);

                bool isLate = inputTime >= beatData.beatTime;
                string missReason = isLate ? "Late press" : "Input too far from beat";

                Debug.Log($"Beat {closestMissBeatIndex} MISS - {missReason}");

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
            Debug.Log("MISS - Late press");
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
            return BeatFeedback.Great;
        }
        else if (errorPercentage <= goodThreshold)
        {
            return BeatFeedback.Good;
        }
        else if (errorPercentage <= inputTolerancePercent)
        {
            return BeatFeedback.Early;
        }

        Debug.Log("MISS - Too inaccurate");
        healthSystem.MissedTarget();
        return BeatFeedback.Miss;
    }
}
