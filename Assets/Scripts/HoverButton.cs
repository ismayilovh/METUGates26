using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class HoverButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    public bool isPlayButton;

    Vector3 originalScale;
    Tween pulseTween;

    void Start()
    {
        originalScale = transform.localScale;

        if (isPlayButton)
        {
            StartPulse();
        }
    }

    void StartPulse()
    {
        pulseTween = transform
            .DOScale(originalScale * 1.08f, 0.6f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pulseTween?.Kill();

        transform.DOKill();
        transform.DOScale(originalScale * 1.15f, 0.15f)
            .SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        transform.localScale = originalScale;

        if (isPlayButton)
        {
            StartPulse();
        }
        else
        {
            transform.DOScale(originalScale, 0.15f)
                .SetEase(Ease.OutQuad);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.DOKill();
        transform.localScale = originalScale;

        if (isPlayButton)
        {
            StartPulse();
        }
    }
}