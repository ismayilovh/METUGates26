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
    public float beatOffset = 0;

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
        winController.gameObject.SetActive(false); 
        isWinChecked = false;
        ObjectsMove.objectCount = config.beats.Count;
        pressAction = inputActions.FindAction("Press");
        pressAction.Enable();

        StartCoroutine(InitializeGame());
    }

    IEnumerator InitializeGame()
    {
        yield return new WaitForSeconds(3f);

        AudioManager.Instance.StartRhythm();

        // Wait until the scheduled DSP start time before spawning beats
        yield return new WaitUntil(() => AudioSettings.dspTime >= AudioManager.Instance.GetDSPStartTime());

        StartCoroutine(SpawnBeats());
        StartCoroutine(UpdateBeats());
        StartCoroutine(CheckInputs());
    }

    IEnumerator SpawnBeats()
    {
        while (beatIndex < config.beats.Count)
        {
            float beatTime = config.beats[beatIndex] + beatOffset;
            float elapsed = AudioManager.Instance.GetTime();
            float remaining = beatTime - elapsed;

            if (remaining <= moveStartTime + 0.5f)
            {
                SpawnBeat(beatIndex, beatTime, false);

                if (Random.value < maliciousChance && beatIndex + 1 < config.beats.Count)
                {
                    float nextBeat = config.beats[beatIndex + 1] + beatOffset;
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
            float time = AudioManager.Instance.GetTime();

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
                float time = AudioManager.Instance.GetTime();

                HandleInput(time);
            }

            yield return null;
        }
    }

    void HandleInput(float inputTime)
    {
        int closest = -1;
        float closestDist = float.MaxValue;

        int nearestUpcoming = -1;
        float nearestUpcomingDist = float.MaxValue;

        foreach (var kvp in activeBeats)
        {
            BeatData data = kvp.Value;

            if (data.handled) continue;

            float dist = Mathf.Abs(inputTime - data.beatTime);
            bool early = inputTime < data.beatTime;

            float tolerance = moveStartTime * (inputTolerancePercent / 100f);

            bool withinWindow = (early && dist <= tolerance) || (!early && dist <= holdTime);

            if (withinWindow && dist < closestDist)
            {
                closest = kvp.Key;
                closestDist = dist;
            }

            if (early && dist < nearestUpcomingDist)
            {
                nearestUpcoming = kvp.Key;
                nearestUpcomingDist = dist;
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

            float errorPercent = (closestDist / moveStartTime) * 100f;
            BeatFeedback feedback = GetFeedback(errorPercent);
            Debug.Log("Beat " + closest + " " + feedback);
        }
        else
        {
            if (nearestUpcoming >= 0)
                activeBeats[nearestUpcoming].handled = true;

            failureCount++;
            GetFeedback(float.MaxValue);
            Debug.Log("Miss — pressed too early");
        }
    }

    BeatFeedback GetFeedback(float error)
    {
        if (error <= perfectThreshold)
        {
            objectsMove.MoveTrail(2f, 0.2f);
            perfectVfx.PlayPerfect();
            return BeatFeedback.Perfect;
        }
        if (error <= greatThreshold)
        {
            objectsMove.MoveTrail(2f, 0.2f);
            perfectVfx.PlayGreat();
            return BeatFeedback.Great;
        }
        if (error <= goodThreshold)
        {
            objectsMove.MoveTrail(2f, 0.2f);
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
            winController.gameObject.SetActive(true);
            winController.ShowWin(PerfectVFX.score);
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