using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public Image darkFade;

    public GameObject gameOverPanel;

    public TMP_Text scoreText;
    public TMP_Text highscoreText;
    public TMP_Text gameOverText;

    public GameObject retryButton;
    public GameObject menuButton;

    public GameObject missVfx;

    int score;
    int highscore;
    bool newHighscore;

    void Start()
    {
        Time.timeScale = 1f;
    }

    public void TriggerGameOver(int finalScore)
    {
        score = finalScore;

        int savedHighscore = PlayerPrefs.GetInt("MaxScore", 0);

        newHighscore = score > savedHighscore;

        if (newHighscore)
        {
            PlayerPrefs.SetInt("MaxScore", score);
            highscore = score;
        }
        else
        {
            highscore = savedHighscore;
        }

        Time.timeScale = 0.3f;

        StartCoroutine(ShowGameOver());
    }

    public void RetryGame()
    {
        ButtonClick.PlayButtonSound();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMenu()
    {
        ButtonClick.PlayButtonSound();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    IEnumerator ShowGameOver()
    {
        missVfx.SetActive(false);

        gameOverPanel.SetActive(true);

        darkFade.DOFade(0.5f, 0.15f);

        yield return new WaitForSeconds(0.1f);

        // SCORE SAYMA
        scoreText.text = "Score: 0";

        float displayedScore = 0;

        Tween scoreTween = DOTween.To(() => displayedScore, x =>
        {
            displayedScore = x;
            scoreText.text = "Score: " + Mathf.RoundToInt(displayedScore);
        }, score, 0.6f);

        yield return scoreTween.WaitForCompletion();

        // garanti olsun diye gerçek deðeri yaz
        scoreText.text = "Score: " + score;

        // GAME OVER TEXT
        gameOverText.transform.localScale = Vector3.zero;

        gameOverText.transform
            .DOScale(1.2f, 0.4f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                gameOverText.transform.DOShakePosition(0.2f, 10f);
            });

        // RETRY BUTTON
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

        // MENU BUTTON
        menuButton.transform.localScale = Vector3.zero;

        menuButton.transform
            .DOScale(1f, 0.3f)
            .SetEase(Ease.OutBack);

        yield return new WaitForSeconds(0.2f);

        // HIGHSCORE SAYMA
        highscoreText.text = "Highscore: 0";

        float displayedHighscore = 0;

        Tween highTween = DOTween.To(() => displayedHighscore, x =>
        {
            displayedHighscore = x;
            highscoreText.text = "Highscore: " + Mathf.RoundToInt(displayedHighscore);
        }, highscore, 0.6f);

        yield return highTween.WaitForCompletion();

        highscoreText.text = "Highscore: " + highscore;

        // NEW HIGHSCORE EFFECT
        if (newHighscore)
        {
            highscoreText.color = Color.yellow;

            highscoreText.transform
                .DOScale(1.3f, 0.3f)
                .SetEase(Ease.OutBack);
        }

        yield return new WaitForSeconds(0.3f);

        darkFade.DOFade(1f, 0.3f);

        yield return new WaitForSeconds(0.3f);

        Time.timeScale = 0f;
    }
}