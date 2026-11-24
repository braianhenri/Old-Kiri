using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Cena02: ao chegar, so aparece a caixa de dialogo do senhorio mandando o Xomi cobrar o Taison.
// (o senhorio em si nao aparece na cena). Usa a mesma caixa de dialogo do Taison.
public class IntroCena02 : MonoBehaviour
{
    [Header("Interface (mesma caixa do Taison)")]
    public GameObject caixaDialogo;
    public TMP_Text textoFala;
    public Image rostoImagem;
    public Sprite[] rostosSenhorio;   // senhorio Falando

    [Header("Falas do senhorio")]
    [TextArea(2, 4)] public string[] falas;

    [Header("Texto e som")]
    public float velocidadeTexto = 0.05f;
    public float tempoCadaSprite = 0.25f;
    public float tempoEntreFalas = 1.5f;
    public AudioSource audioSource;
    public AudioClip somTeclado;
    public float intervaloSom = 0.08f;

    [Header("Fade de entrada")]
    public float tempoFade = 1f;
    public float esperaAposFade = 0.4f;

    IEnumerator Start()
    {
        // comeca preto (a cena03 terminou escura) e clareia
        TelaFade fade = TelaFade.Criar(1f);
        CutsceneUtil.TravarXomi(true);
        yield return null;
        yield return StartCoroutine(fade.Animar(0f, tempoFade));
        yield return new WaitForSeconds(esperaAposFade);

        if (caixaDialogo != null && textoFala != null && falas != null && falas.Length > 0)
        {
            caixaDialogo.SetActive(true);
            foreach (string f in falas) yield return StartCoroutine(Falar(f));
            caixaDialogo.SetActive(false);
        }

        CutsceneUtil.TravarXomi(false);
    }

    IEnumerator Falar(string texto)
    {
        textoFala.text = "";
        bool temFace = rostosSenhorio != null && rostosSenhorio.Length > 0;
        if (rostoImagem != null)
        {
            rostoImagem.enabled = temFace;
            if (temFace) rostoImagem.sprite = rostosSenhorio[0];
        }

        int atual = 0;
        float cSprite = 0f;
        float cSom = 0f;
        foreach (char letra in texto)
        {
            textoFala.text += letra;

            if (letra != ' ' && audioSource != null && somTeclado != null && cSom <= 0f)
            {
                audioSource.PlayOneShot(somTeclado);
                cSom = intervaloSom;
            }
            cSom -= velocidadeTexto;

            cSprite += velocidadeTexto;
            if (cSprite >= tempoCadaSprite && rostosSenhorio != null && rostosSenhorio.Length > 1)
            {
                cSprite = 0f;
                atual = (atual + 1) % rostosSenhorio.Length;
                if (rostoImagem != null) rostoImagem.sprite = rostosSenhorio[atual];
            }
            yield return new WaitForSeconds(velocidadeTexto);
        }

        if (temFace && rostoImagem != null) rostoImagem.sprite = rostosSenhorio[0];
        yield return new WaitForSeconds(tempoEntreFalas);
    }
}
