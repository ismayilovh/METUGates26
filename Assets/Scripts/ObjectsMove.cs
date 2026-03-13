using UnityEngine;
using DG.Tweening;

public class ObjectsMove : MonoBehaviour
{
    GameObject[] objects;
    Vector3[] startingPositions;

    void Start()
    {
        objects = GameObject.FindGameObjectsWithTag("moveobject");

        startingPositions = new Vector3[objects.Length];

        for (int i = 0; i < objects.Length; i++)
        {
            startingPositions[i] = objects[i].transform.position;
        }
        
    }

    public void MoveTrail(float moveDistance, float moveDuration)
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