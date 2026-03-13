using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class PerfectVFX : MonoBehaviour
{
    public Transform ring;
    public ParticleSystem particles;
    public Transform perfectText;
    public Transform perfectImage;

    public float spotScaleAmount = 1.1f;

    public void PlayPerfect()
    {
        // Ring
        ring.DOKill();
        //ring.localScale = Vector3.zero * 0.8f;

        Sequence ringSeq = DOTween.Sequence();
        ringSeq.Append(ring.DOScale(spotScaleAmount, 0.12f).SetEase(Ease.OutBack));
        ringSeq.Append(ring.DOScale(1f, 0.08f).SetEase(Ease.InBack));

        // Perfect Text
        perfectText.DOKill();
        perfectText.localScale = Vector3.zero;

        Sequence textSeq = DOTween.Sequence();
        textSeq.Append(perfectText.DOScale(1.3f, 0.2f).SetEase(Ease.OutBack));
        textSeq.Append(perfectText.DOScale(0f, 0.15f).SetEase(Ease.InBack));

        // Perfect Image
        perfectImage.DOKill();
        perfectImage.localScale = Vector3.zero;

        Sequence imageSeq = DOTween.Sequence();
        imageSeq.Append(perfectImage.DOScale(1.1f, 0.18f).SetEase(Ease.OutBack));
        imageSeq.Append(perfectImage.DOScale(0f, 0.15f).SetEase(Ease.InBack));

        // Particles
        particles.Play();

        // Camera shake
        Camera.main.transform.DOShakePosition(0.1f, 0.15f);
    }
}