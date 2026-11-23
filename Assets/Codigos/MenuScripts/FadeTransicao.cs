using System.Collections;
using UnityEngine;

public class FadeTransicao : MonoBehaviour
{
    public CanvasGroup painelFade;

    void Start()
    {
        painelFade.alpha = 0f;
        painelFade.gameObject.SetActive(false);
    }

    public IEnumerator Escurecer(float tempo = 1f)
    {
        Time.timeScale = 0f;
        painelFade.gameObject.SetActive(true);
        yield return Animar(0f, 1f, tempo);
    }

    public IEnumerator Clarear(float tempo = 1f)
    {
        painelFade.gameObject.SetActive(true);
        yield return Animar(1f, 0f, tempo);
        painelFade.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    IEnumerator Animar(float de, float para, float tempo)
    {
        float t = 0f;
        painelFade.alpha = de;
        while (t < tempo)
        {
            t += Time.unscaledDeltaTime;
            painelFade.alpha = Mathf.Lerp(de, para, t / tempo);
            yield return null;
        }
        painelFade.alpha = para;
    }
}