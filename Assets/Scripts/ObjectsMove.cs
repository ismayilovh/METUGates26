using UnityEngine;
using DG.Tweening;
using System.Collections;

public class ObjectsMove : MonoBehaviour
{
    GameObject[] objects;

    [Header("Spawn Settings")]
    public GameObject prefab;
    public static int objectCount = 25;
    public float offset = 2f;

    [Header("Positions")]
    public Transform endingPos;
    public Transform lastEndPos;

    public RandomSpriteGenerator spriteGenerator;

    void Start()
    {
        SpawnObjects();
    }

    void SpawnObjects()
    {
        objects = new GameObject[objectCount];

        for (int i = 0; i < objectCount; i++)
        {
            Vector3 pos = endingPos.position;
            pos.x -= i * offset;

            GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
            obj.tag = "moveobject";

            objects[i] = obj;
        }

        spriteGenerator.AssignRandomSprites();
    }

    public void MoveTrail(float moveDistance, float moveDuration)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null) continue;

            GameObject obj = objects[i];

            obj.transform.DOMoveX(
                obj.transform.position.x + moveDistance,
                moveDuration
            )
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                if (obj == null) return;

                if (obj.transform.position.x >= endingPos.position.x + 1)
                {
                    obj.transform.DOKill();

                    StartCoroutine(SendToLastEnd(obj));
                }
            });
        }
    }

    IEnumerator SendToLastEnd(GameObject obj)
    {
        yield return new WaitForSeconds(0.2f);

        if (obj == null) yield break;

        obj.transform.DOMove(
            lastEndPos.position,
            0.4f
        )
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            if (obj != null)
                Destroy(obj);
        });
    }
}