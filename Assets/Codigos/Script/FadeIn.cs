using System.Collections;
using UnityEngine;

public class FadeIn : MonoBehaviour
{
    [Header("Configuração")]
    public CanvasGroup painelFade; // arraste o PainelFade aqui
    public float duracao = 1.5f;   // quantos segundos até clarear totalmente

    void Start()
    {
        painelFade.alpha = 1f; // começa 100% preto
        StartCoroutine(Clarear());
    }

    IEnumerator Clarear()
    {
        float tempoDecorrido = 0f;

        while (tempoDecorrido < duracao)
        {
            tempoDecorrido += Time.deltaTime;
            painelFade.alpha = Mathf.Lerp(1f, 0f, tempoDecorrido / duracao);
            yield return null;
        }

        painelFade.alpha = 0f;
        painelFade.gameObject.SetActive(false); // desativa pra não bloquear cliques na tela
    }
}