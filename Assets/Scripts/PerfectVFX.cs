using UnityEngine;
using DG.Tweening;

public class PerfectVFX : MonoBehaviour
{
    public Transform ring;
    public ParticleSystem particles;

    public Transform perfectText;
    public Transform perfectImage;

    public Transform greatText;
    public Transform greatImage;

    public Transform goodText;
    public Transform goodImage;

    public Transform earlyText;
    public Transform earlyImage;

    public float spotScaleAmount = 1.1f;

    void PlayEffect(Transform text, Transform image, float multiplier, float duration, bool playParticles)
    {
        // Ring
        ring.DOKill();

        Sequence ringSeq = DOTween.Sequence();
        ringSeq.Append(ring.DOScale(spotScaleAmount * multiplier, duration).SetEase(Ease.OutBack));
        ringSeq.Append(ring.DOScale(1f, duration * 0.7f).SetEase(Ease.InBack));

        // Text
        text.DOKill();
        text.localScale = Vector3.zero;

        Sequence textSeq = DOTween.Sequence();
        textSeq.Append(text.DOScale(1.3f * multiplier, duration).SetEase(Ease.OutBack));
        textSeq.Append(text.DOScale(0f, duration * 0.8f).SetEase(Ease.InBack));

        // Image
        image.DOKill();
        image.localScale = Vector3.zero;

        Sequence imageSeq = DOTween.Sequence();
        imageSeq.Append(image.DOScale(1.1f * multiplier, duration).SetEase(Ease.OutBack));
        imageSeq.Append(image.DOScale(0f, duration * 0.8f).SetEase(Ease.InBack));

        if (playParticles)
            particles.Play();

        Camera.main.transform.DOShakePosition(0.1f * multiplier, 0.12f);
    }

    public void PlayPerfect()
    {
        PlayEffect(perfectText, perfectImage, 1f, 0.20f, true);
    }

    public void PlayGreat()
    {
        PlayEffect(greatText, greatImage, 0.85f, 0.18f, false);
    }

    public void PlayGood()
    {
        PlayEffect(goodText, goodImage, 0.7f, 0.16f, false);
    }
}