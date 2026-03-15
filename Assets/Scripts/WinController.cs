using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class WinController : MonoBehaviour
{
    public Transform panel;
    public TMP_Text titleText;

    public Transform retryButton;
    public Transform menuButton;
    public Transform manifestButton; // yeni button

    public TMP_Text scoreText;
    public TMP_Text HighscoreText;

    int score;
    int highscore;

    public Animator cashierAnimator;

    public float congratsDelay = 1.5f;

    void Start()
    {
        panel.localScale = Vector3.zero;
        titleText.transform.localScale = Vector3.zero;

        retryButton.localScale = Vector3.zero;
        menuButton.localScale = Vector3.zero;
        manifestButton.localScale = Vector3.zero; // yeni button
    }

    public void ShowWin(int finalScore)
    {
        score = finalScore;

        int savedHighscore = PlayerPrefs.GetInt("MaxScore", 0);

        if (score > savedHighscore)
        {
            savedHighscore = score;
            PlayerPrefs.SetInt("MaxScore", savedHighscore);
        }

        highscore = savedHighscore;

        if (cashierAnimator != null)
        {
            cashierAnimator.SetTrigger("Play");
        }

        StartCoroutine(ShowWinAfterDelay());
    }

    IEnumerator ShowWinAfterDelay()
    {
        yield return new WaitForSeconds(congratsDelay);
        ShowWinUI();
    }

    void ShowWinUI()
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(panel.DOScale(1f, 0.4f).SetEase(Ease.OutBack));

        seq.Append(titleText.transform.DOScale(1.2f, 0.25f).SetEase(Ease.OutBack));

        seq.AppendCallback(() =>
        {
            scoreText.text = "Score: 0";

            int displayedScore = 0;

            DOTween.To(() => displayedScore, x =>
            {
                displayedScore = x;
                scoreText.text = "Score: " + displayedScore;
            }, score, 0.6f);
        });

        seq.AppendInterval(0.6f);

        seq.AppendCallback(() =>
        {
            HighscoreText.text = "Highscore: 0";

            int displayedHighscore = 0;

            DOTween.To(() => displayedHighscore, x =>
            {
                displayedHighscore = x;
                HighscoreText.text = "Highscore: " + displayedHighscore;
            }, highscore, 0.6f);
        });

        seq.AppendInterval(0.6f);

        seq.Append(retryButton.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
        seq.Join(menuButton.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
        seq.Join(manifestButton.DOScale(1f, 0.3f).SetEase(Ease.OutBack)); // yeni button animasyonu
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ManifestScene()
    {
        SceneManager.LoadScene("ManifestScene2");
    }
}