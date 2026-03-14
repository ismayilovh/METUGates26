using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private const double scheduleBuffer = 1.0;
    [SerializeField] private AudioSource audioSource;
    private double dspStartTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void StartRhythm()
    {
        dspStartTime = AudioSettings.dspTime + scheduleBuffer;
        audioSource.PlayScheduled(dspStartTime);
    }

    public float GetTime()
    {
        return (float)(AudioSettings.dspTime - dspStartTime);
    }

    public double GetDSPStartTime()
    {
        return (float)dspStartTime;
    }
}
