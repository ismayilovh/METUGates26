using UnityEngine;

public class ButtonClick : MonoBehaviour
{
    public static ButtonClick Instance;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClick;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void PlayButtonSound()
    {
        if (Instance == null) return;

        Instance.audioSource.PlayOneShot(Instance.buttonClick);
    }
}