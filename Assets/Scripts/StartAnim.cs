using System.Collections;
using TMPro;
using UnityEngine;

public class StartAnim : MonoBehaviour
{
    public TextMeshProUGUI text;

    void Start()
    {
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        text.text = "3";
        yield return new WaitForSeconds(0.5f);

        text.text = "2";
        yield return new WaitForSeconds(0.5f);

        text.text = "1";
        yield return new WaitForSeconds(0.5f);

        text.text = "GO!!!";
        yield return new WaitForSeconds(0.5f);

        text.text = "";
    }
}