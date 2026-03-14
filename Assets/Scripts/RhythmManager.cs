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
    [Header("References")]
    public GameObject mainPoint;
    public GameObject beatPrefab;
    public GameObject maliciousBeatPrefab;

    public Transform poolStart;
    public Transform poolEnd;

    public RhythmConfig config;
    public InputActionAsset inputActions;

    public ObjectsMove objectsMove;
    public HealthSystem healthSystem;
    public PerfectVFX perfectVfx;
    public WinController winController;

    [Header("Timing")]
    public float moveStartTime = 2f;
    public float holdTime = 0.05f;
    public float exitDuration = 0.4f;

    [Header("Input Tolerance")]
    public float inputTolerancePercent = 20f;
    public float perfectThreshold = 2.5f;
    public float greatThreshold = 5f;
    public float goodThreshold = 10f;

    [Header("Malicious")]
    public float maliciousChance = 0.1f;

    Queue<GameObject> beatPool = new Queue<GameObject>();
    Queue<GameObject> maliciousPool = new Queue<GameObject>();

    Dictionary<int, BeatData> activeBeats = new Dictionary<int, BeatData>();

    InputAction pressAction;

    float startTime;

    int beatIndex;

    int successCount;
    int failureCount;

    class BeatData
    {
        public float beatTime;
        public bool handled;
        public bool malicious;
        public GameObject obj;
    }

    void Start()
    {
        isWinChecked = false;
        pressAction = inputActions.FindAction("Press");
        pressAction.Enable();

        startTime = Time.time;

        StartCoroutine(SpawnBeats());
        StartCoroutine(UpdateBeats());
        StartCoroutine(CheckInputs());
    }

    IEnumerator SpawnBeats()
    {
        while (beatIndex < config.beats.Count)
        {
            float beatTime = config.beats[beatIndex];
            float elapsed = Time.time - startTime;
            float remaining = beatTime - elapsed;

            if (remaining <= moveStartTime + 0.5f)
            {
                SpawnBeat(beatIndex, beatTime, false);

                if (Random.value < maliciousChance && beatIndex + 1 < config.beats.Count)
                {
                    float nextBeat = config.beats[beatIndex + 1];
                    float maliciousTime = (beatTime + nextBeat) * 0.5f;

                    SpawnBeat(beatIndex + 10000, maliciousTime, true);
                }

                beatIndex++;
            }

            yield return null;
        }
    }

    void SpawnBeat(int index, float beatTime, bool malicious)
    {
        GameObject obj = GetObject(malicious);

        obj.transform.position = poolStart.position;

        BeatData data = new BeatData();
        data.beatTime = beatTime;
        data.handled = false;
        data.malicious = malicious;
        data.obj = obj;

        activeBeats[index] = data;
    }

    IEnumerator UpdateBeats()
    {
        while (true)
        {
            float time = Time.time - startTime;

            List<int> removeList = new List<int>();

            foreach (var kvp in activeBeats)
            {
                int index = kvp.Key;
                BeatData data = kvp.Value;

                float remaining = data.beatTime - time;

                Vector3 start = poolStart.position;
                Vector3 main = mainPoint.transform.position;
                Vector3 end = poolEnd.position;

                if (remaining > moveStartTime)
                {
                    data.obj.transform.position = start;
                }
                else if (remaining > 0)
                {
                    float t = 1f - (remaining / moveStartTime);
                    data.obj.transform.position = Vector3.Lerp(start, main, t);
                }
                else if (remaining > -holdTime)
                {
                    data.obj.transform.position = main;
                }
                else if (remaining > -holdTime - exitDuration)
                {
                    float t = (-holdTime - remaining) / exitDuration;
                    data.obj.transform.position = Vector3.Lerp(main, end, t);
                }
                else
                {
                    if (!data.handled && !data.malicious)
                    {
                        failureCount++;
                        healthSystem.MissedTarget();
                    }

                    ReturnObject(data.obj, data.malicious);

                    removeList.Add(index);
                }
            }

            foreach (int i in removeList)
            {
                activeBeats.Remove(i);
            }

            CheckForWin();

            yield return null;
        }
    }

    IEnumerator CheckInputs()
    {
        while (true)
        {
            if (pressAction.WasPressedThisFrame())
            {
                float time = Time.time - startTime;

                HandleInput(time);
            }

            yield return null;
        }
    }

    void HandleInput(float inputTime)
    {
        int closest = -1;
        float closestDist = float.MaxValue;

        foreach (var kvp in activeBeats)
        {
            BeatData data = kvp.Value;

            if (data.handled)
                continue;

            float dist = Mathf.Abs(inputTime - data.beatTime);
            bool early = inputTime < data.beatTime;

            float tolerance = Mathf.Max(data.beatTime * (inputTolerancePercent / 100f), 0.1f);

            if (early && dist <= tolerance && dist < closestDist)
            {
                closest = kvp.Key;
                closestDist = dist;
            }
        }

        if (closest >= 0)
        {
            BeatData data = activeBeats[closest];

            data.handled = true;

            if (data.malicious)
            {
                failureCount++;
                healthSystem.MissedTarget();
                return;
            }

            successCount++;

            float errorPercent = (closestDist / data.beatTime) * 100f;

            BeatFeedback feedback = GetFeedback(errorPercent);

            Debug.Log("Beat " + closest + " " + feedback);
        }
    }

    BeatFeedback GetFeedback(float error)
    {
        objectsMove.MoveTrail(2f, 0.2f);

        if (error <= perfectThreshold)
        {
            perfectVfx.PlayPerfect();
            return BeatFeedback.Perfect;
        }

        if (error <= greatThreshold)
        {
            perfectVfx.PlayGreat();
            return BeatFeedback.Great;
        }

        if (error <= goodThreshold)
        {
            perfectVfx.PlayGood();
            return BeatFeedback.Good;
        }

        healthSystem.MissedTarget();
        return BeatFeedback.Miss;
    }

    bool isWinChecked = false;

    void CheckForWin()
    {
        if (!isWinChecked &&
            successCount + failureCount >= config.beats.Count &&
            HealthSystem.health > 0)
        {
            isWinChecked = true;
            winController.ShowWin(successCount);
        }
    }

    GameObject GetObject(bool malicious)
    {
        Queue<GameObject> pool = malicious ? maliciousPool : beatPool;
        GameObject prefab = malicious ? maliciousBeatPrefab : beatPrefab;

        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        return Instantiate(prefab, poolStart.position, Quaternion.identity);
    }

    void ReturnObject(GameObject obj, bool malicious)
    {
        obj.SetActive(false);

        if (malicious)
            maliciousPool.Enqueue(obj);
        else
            beatPool.Enqueue(obj);
    }

    void OnDisable()
    {
        pressAction.Disable();
    }
}