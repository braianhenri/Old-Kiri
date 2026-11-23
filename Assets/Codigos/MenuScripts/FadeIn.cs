using System.Collections;
using UnityEngine;

public class FadeIn : MonoBehaviour
{
    [Header("Configuracao")]
    public CanvasGroup painelFade;
    public float duracao = 3f; // usado quando o efeito de acordar esta desligado

    [Header("Efeito de acordar")]
    public bool efeitoAcordar = true;
    public float tempoNoEscuro = 1f; // quanto tempo fica preto antes de abrir os olhos

    void Start()
    {
        // Congela o jogo
        Time.timeScale = 0f;

        // Ativa o painel e comeca totalmente preto
        painelFade.gameObject.SetActive(true);
        painelFade.alpha = 1f;

        if (efeitoAcordar)
            StartCoroutine(Acordar());
        else
            StartCoroutine(Clarear());
    }

    // Efeito de acordar: abre os olhos aos poucos, piscando
    IEnumerator Acordar()
    {
        yield return Esperar(tempoNoEscuro);

        yield return IrPara(0.55f, 1.0f);   // abre um pouco
        yield return IrPara(0.95f, 0.45f);  // fecha de novo
        yield return Esperar(0.3f);
        yield return IrPara(0.25f, 1.0f);   // abre mais
        yield return IrPara(0.70f, 0.35f);  // pisca
        yield return IrPara(0f, 1.4f);      // abre de vez

        Finalizar();
    }

    // Fade simples (efeito antigo)
    IEnumerator Clarear()
    {
        float tempoDecorrido = 0f;

        while (tempoDecorrido < duracao)
        {
            // Usa unscaledDeltaTime porque o jogo esta pausado
            tempoDecorrido += Time.unscaledDeltaTime;

            painelFade.alpha = Mathf.Lerp(1f, 0f, tempoDecorrido / duracao);

            yield return null;
        }

        Finalizar();
    }

    IEnumerator Esperar(float tempo)
    {
        float t = 0f;
        while (t < tempo)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    // Vai do alpha atual ate o alvo, de forma suave
    IEnumerator IrPara(float alvo, float tempo)
    {
        float inicio = painelFade.alpha;
        float t = 0f;

        while (t < tempo)
        {
            t += Time.unscaledDeltaTime;
            painelFade.alpha = Mathf.Lerp(inicio, alvo, Mathf.SmoothStep(0f, 1f, t / tempo));
            yield return null;
        }

        painelFade.alpha = alvo;
    }

    void Finalizar()
    {
        painelFade.alpha = 0f;

        // Desativa o painel
        painelFade.gameObject.SetActive(false);

        // Libera o jogo
        Time.timeScale = 1f;
    }
}
