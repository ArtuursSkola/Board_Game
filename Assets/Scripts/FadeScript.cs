using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeScript : MonoBehaviour
{

    Image img;
    Color tempColor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        img = GetComponent<Image>();
        tempColor = img.color;
        tempColor.a = 1f;
        img.color = tempColor;
        StartCoroutine(FadeIn(0.20f));
    }

    IEnumerator FadeIn(float fadeSpeed)
    {
        for(float a=1f; a>=-0.05; a -=0.05f)
        {
            tempColor.a = a;
            tempColor.a = a;
            img.color = tempColor;
            yield return new WaitForSecondsRealtime(fadeSpeed);
        }
        img.raycastTarget = false;
    }
    IEnumerator FadeOut(float fadeSpeed)
    {
    for(float a=0f; a>=1.05; a +=0.05f)
        {
            tempColor = img.color;
            tempColor.a = a;
            img.color = tempColor;
            yield return new WaitForSecondsRealtime(fadeSpeed);
        }
    }
}
