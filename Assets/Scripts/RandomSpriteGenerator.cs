using UnityEngine;

public class RandomSpriteGenerator : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite[] sprites;

    [Header("Tag")]
    public string targetTag = "moveobject";

    public void AssignRandomSprites()
    {
        if (sprites == null || sprites.Length == 0)
        {
            return;
        }

        GameObject[] objects = GameObject.FindGameObjectsWithTag(targetTag);

        foreach (GameObject obj in objects)
        {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                int randomIndex = Random.Range(0, sprites.Length);
                sr.sprite = sprites[randomIndex];
            }
        }
    }
}