using DG.Tweening;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MissVFX : MonoBehaviour
{
    public Transform missText;
    public Transform missImage;
    public ParticleSystem missParticles;
    public Transform ring;
    public float spotScaleAmount = 1.1f;
    public Image loseBG;

    public void PlayMiss()
    {
        PerfectVFX.score -= 20;
        // Ring
        ring.DOKill();
        //ring.localScale = Vector3.zero;

        Sequence ringSeq = DOTween.Sequence();
        ringSeq.Append(ring.DOScale(spotScaleAmount, 0.12f).SetEase(Ease.OutBack));
        ringSeq.Append(ring.DOScale(1, 0.08f).SetEase(Ease.InBack));


        // Miss Text
        missText.DOKill();
        missText.localScale = Vector3.zero;

        Sequence textSeq = DOTween.Sequence();
        textSeq.Append(missText.DOScale(0.9f, 0.12f).SetEase(Ease.OutQuad));
        textSeq.Append(missText.DOScale(0.6f, 0.15f).SetEase(Ease.InQuad));
        textSeq.Append(missText.DOScale(0f, 0.1f));

        // Miss Image
        missImage.DOKill();
        missImage.localScale = Vector3.zero;

        Sequence imgSeq = DOTween.Sequence();
        imgSeq.Append(missImage.DOScale(0.8f, 0.12f).SetEase(Ease.OutQuad));
        imgSeq.Append(missImage.DOScale(0f, 0.18f).SetEase(Ease.InQuad));

        // Particle
         missParticles.Play();

        // küçük kamera shake
        Camera.main.transform.DOShakePosition(0.08f, 0.08f);
        // baþlangýç alpha
        Color c = loseBG.color;
        c.a = 0;
        loseBG.color = c;

        Sequence seq = DOTween.Sequence();

        seq.Append(loseBG.DOFade(0.06f, 0.08f)); // çok hafif ve hýzlý
        seq.Append(loseBG.DOFade(0f, 0.12f));    // hemen geri

    }

}