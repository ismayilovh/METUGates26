using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;
using System.Collections;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Scene gameScene;
    public GameObject mainMenuUI;
    public GameObject settingsUI;
    public GameObject playButton;

    public float scaleAmount = 1.15f;
    public float duration = 0.5f;
    public Slider volumeSlider;


    void Start()
    {
        volumeSlider.onValueChanged.AddListener(SetVolume);

        playButton.transform.localScale = Vector3.one * 0.3f;

        Sequence seq = DOTween.Sequence();

        seq.Append(playButton.transform.DOScale(0.25f, 0.07f).SetEase(Ease.InQuad));     // anticipation
        seq.Append(playButton.transform.DOScale(1.45f, 0.12f).SetEase(Ease.OutQuad));    // pop
        seq.Append(playButton.transform.DOScale(1.2f, 0.12f).SetEase(Ease.InOutSine));   // bounce
        seq.Append(playButton.transform.DOScale(1.3f, 0.18f).SetEase(Ease.OutBack));     // smooth settle

        seq.AppendInterval(0.4f);

        seq.SetLoops(-1);

    }

    void SetVolume(float value)
    {
        AudioListener.volume = value;
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void Settings()
    {
        settingsUI.SetActive(true);
        settingsUI.transform.localScale = Vector3.zero;
        settingsUI.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack);
        mainMenuUI.SetActive(false);
    }

    public void BackToMainMenu()
    {
        StartCoroutine(ClosePanelRoutine());
    }

    IEnumerator ClosePanelRoutine()
    {
        settingsUI.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack);

        yield return new WaitForSeconds(0.2f);

        settingsUI.SetActive(false);
        mainMenuUI.SetActive(true);
    }


    public void QuitGame()
    {
        Application.Quit();
    }
}
