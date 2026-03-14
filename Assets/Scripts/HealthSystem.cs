using UnityEngine;
using UnityEngine.InputSystem;

public class HealthSystem : MonoBehaviour
{
    public int maxHealth = 3;
    int health;

    public GameObject[] healthImages;

    public MissVFX missVFXScript;

    private void Start()
    {
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

        if (health <= 0)
        {
            gameOverManager.TriggerGameOver(1000);
        }
    }
}