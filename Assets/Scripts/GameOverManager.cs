using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    public Image darkFade;

    public GameObject gameOverPanel;

    public TMP_Text scoreText;
    public TMP_Text gameOverText;

    public GameObject retryButton;
    public GameObject menuButton;

    int score;

    public GameObject missVfx;

    private void Start()
    {
        Time.timeScale = 1f;
    }

    public void TriggerGameOver(int finalScore)
    {
        score = finalScore;

        Time.timeScale = 0.3f;

        StartCoroutine("ShowGameOver", 0.3f);
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("EmreScene");
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    IEnumerator ShowGameOver()
    {
        missVfx.SetActive(false);
        // panel aç
        gameOverPanel.SetActive(true);

        // ekran kararma
        darkFade.DOFade(0.5f, 0.15f);

        // score animasyonu
        scoreText.text = "0";
        int displayedScore = 0;

        DOTween.To(() => displayedScore, x =>
        {
            displayedScore = x;
            scoreText.text = "Score: " + displayedScore;
        }, score, 0.5f);


        // game over yazýsý
        gameOverText.transform.localScale = Vector3.zero;

        gameOverText.transform
            .DOScale(1.2f, 0.4f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                gameOverText.transform.DOShakePosition(0.2f, 10f);
            });

        // retry button animasyonu
        retryButton.transform.localScale = Vector3.zero;

        retryButton.transform
            .DOScale(1f, 0.3f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                retryButton.transform
                    .DOScale(1.08f, 0.6f)
                    .SetLoops(-1, LoopType.Yoyo);
            });

        // menu button animasyonu
        menuButton.transform.localScale = Vector3.zero;

        menuButton.transform
            .DOScale(1f, 0.3f)
            .SetEase(Ease.OutBack);

        yield return new WaitForSeconds(0.3f);
        darkFade.DOFade(1f, 0.3f);
        yield return new WaitForSeconds(0.3f);
        Time.timeScale = 0.0f;
    }
}