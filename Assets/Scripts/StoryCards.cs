using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class StoryCards : MonoBehaviour
{
    // Scene Pop in
    public Transform[] objectsToAnimate;
    public float duration = 0.4f;
    public float delayBetween = 0.08f;

    void Start()
    {
        for (int i = 0; i < objectsToAnimate.Length; i++)
        {
            Transform obj = objectsToAnimate[i];

            obj.localScale = Vector3.zero;

            obj.DOScale(1f, duration)
               .SetDelay(i * delayBetween)
               .SetEase(Ease.OutBack);
        }

        // text baþta kapalý
        storyText.transform.localScale = Vector3.zero;
    }

    // Card Animations
    [System.Serializable]
    public class CardStep
    {
        public Transform card;
        public Transform target;
        public float duration = 0.6f;
    }

    public CardStep[] steps;

    // STORY TEXT
    public TMP_Text storyText;
    public string[] texts;

    int currentStep = 0;
    bool animating = false;

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            PlayNext();
        }
    }

    void PlayNext()
    {
        if (animating) return;

        if (currentStep >= steps.Length)
        {
            SceneManager.LoadScene("EmreScene4");
            return;
        }

        CardStep step = steps[currentStep];
        animating = true;

        Sequence seq = DOTween.Sequence();

        // CARD MOVE
        seq.Join(step.card.DOMove(step.target.position, step.duration).SetEase(Ease.OutBack));
        seq.Join(step.card.DORotate(step.target.rotation.eulerAngles, step.duration));
        seq.Join(step.card.DOScale(step.target.localScale, step.duration));

        // TEXT CHANGE
        if (currentStep < texts.Length)
        {
            seq.AppendCallback(() =>
            {
                if(currentStep >= 1) storyText.text = texts[currentStep - 1];

                storyText.transform.DOKill();
                storyText.transform.localScale = Vector3.zero;

                storyText.transform
                    .DOScale(1f, 0.35f)
                    .SetEase(Ease.OutBack);
            });
        }

        seq.OnComplete(() =>
        {
            animating = false;
            currentStep++;

            // ilk animasyondan sonra ikinci otomatik
            if (currentStep == 1)
            {
                PlayNext();
            }
        });
    }
}