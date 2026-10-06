
using System.Collections;
using UnityEngine;

public class FadeIn : MonoBehaviour
{
    [Header("Configuração")]
    public CanvasGroup painelFade;
    public float duracao = 3f;

    void Start()
    {
        // Congela o jogo
        Time.timeScale = 0f;

        // Ativa o painel
        painelFade.gameObject.SetActive(true);

        // Começa totalmente preto
        painelFade.alpha = 1f;

        StartCoroutine(Clarear());
    }

    IEnumerator Clarear()
    {
        float tempoDecorrido = 0f;

        while (tempoDecorrido < duracao)
        {
            // Usa unscaledDeltaTime porque o jogo está pausado
            tempoDecorrido += Time.unscaledDeltaTime;

            painelFade.alpha = Mathf.Lerp(
                1f,
                0f,
                tempoDecorrido / duracao
            );

            yield return null;
        }

        painelFade.alpha = 0f;

        // Desativa o painel
        painelFade.gameObject.SetActive(false);

        // Libera o jogo
        Time.timeScale = 1f;
    }
}