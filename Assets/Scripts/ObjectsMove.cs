using UnityEngine;
using DG.Tweening;

public class ObjectsMove : MonoBehaviour
{
    GameObject[] objects;
    Vector3[] startingPositions;

    public float moveDistance = 2f;
    public float moveDuration = 0.2f;

    void Start()
    {
        objects = GameObject.FindGameObjectsWithTag("moveobject");

        startingPositions = new Vector3[objects.Length];

        for (int i = 0; i < objects.Length; i++)
        {
            startingPositions[i] = objects[i].transform.position;
        }
        MoveTrail();
    }

    public void MoveTrail()
    {
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < objects.Length; i++)
        {
            seq.Join(
                objects[i].transform.DOMoveX(
                    objects[i].transform.position.x + moveDistance,
                    moveDuration
                ).SetEase(Ease.Linear)
            );
        }

        seq.OnComplete(() =>
        {
            for (int i = 0; i < objects.Length; i++)
            {
                objects[i].transform.position = startingPositions[i];
            }
        });
    }
}