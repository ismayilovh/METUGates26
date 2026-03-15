using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    public Scene gameScene;
    public GameObject mainMenuUI;
    public GameObject settingsUI;
    public GameObject playButton;

    public Slider volumeSlider;
    public Slider sfxVolumeSlider;

    void Start()
    {
        PlayerPrefs.SetFloat("SFXVolume", 1f);

        volumeSlider.onValueChanged.AddListener(SetVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    void SetVolume(float value)
    {
        AudioListener.volume = value;
    }

    void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void PlayGame()
    {
        ButtonClick.PlayButtonSound();
        SceneManager.LoadScene("StoryScene");
    }

    public void Settings()
    {
        ButtonClick.PlayButtonSound();
        settingsUI.SetActive(true);
        settingsUI.transform.localScale = Vector3.zero;
        settingsUI.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack);

        mainMenuUI.SetActive(false);
    }

    public void BackToMainMenu()
    {
        ButtonClick.PlayButtonSound();
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
        ButtonClick.PlayButtonSound();
        Application.Quit();
    }

}