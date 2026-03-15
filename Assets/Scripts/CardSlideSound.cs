using UnityEngine;

public class CardSlideSound : MonoBehaviour
{
    public Rigidbody rb;
    public AudioSource slideAudio;

    public float minSlideSpeed = 0.2f;

    void Update()
    {
        float speed = rb.linearVelocity.magnitude;

        if (speed > minSlideSpeed)
        {
            if (!slideAudio.isPlaying)
                slideAudio.Play();
        }
        else
        {
            if (slideAudio.isPlaying)
                slideAudio.Stop();
        }
    }
}