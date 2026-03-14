using UnityEngine;
using UnityEngine.InputSystem;

public class HealthSystem : MonoBehaviour
{
    public int maxHealth = 3;
    public static int health;
    public static bool isGameOver = false;

    public GameObject[] healthImages;

    public MissVFX missVFXScript;

    private void Start()
    {
        isGameOver = false;
        health = maxHealth;
    }

    public GameOverManager gameOverManager;
    public void MissedTarget()
    {
        health--;

        // Miss efekti
        missVFXScript.PlayMiss();

        if (health >= 0 && health < healthImages.Length)
        {
            healthImages[health].SetActive(false);
        }

        if (health <= 0 && isGameOver == false)
        {
            isGameOver = true;
            if (PerfectVFX.score > PlayerPrefs.GetInt("MaxScore"))
            {
                PlayerPrefs.SetInt("MaxScore", PerfectVFX.score);
            }
            gameOverManager.TriggerGameOver(PerfectVFX.score);
        }
    }
}