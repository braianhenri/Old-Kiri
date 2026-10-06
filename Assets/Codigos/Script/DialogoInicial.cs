using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogoInicial : MonoBehaviour
{
    [Header("Interface")]
    public GameObject caixaDialogo;
    public TMP_Text TextoFalasIniciais;
    public Image RostoXomiDialogando1;

    [Header("Sprites do rosto")]
    public Sprite rosto1;
    public Sprite rosto2;
    public Sprite rosto3;
    public Sprite rosto4;

    [Header("Fala")]
    [TextArea(3, 5)]
    public string fala = "Onde... onde eu estou?";

    [Header("Velocidade do texto")]
    public float velocidadeTexto = 0.05f;

    [Header("Tempo de cada sprite")]
    public float tempoCadaSprite = 0.4f;

    [Header("Repetições dos sprites")]
    public int repeticoesSprites = 2;

    [Header("Tempo até desaparecer")]
    public float tempoAteSumir = 3f;

    [Header("Tempo antes do diálogo")]
    public float tempoAntesDoDialogo = 3f;

    [Header("Som de digitação")]
    public AudioSource audioSource;
    public AudioClip somTeclado;

    [Header("Intervalo entre sons")]
    public float intervaloSom = 0.08f;

    void Start()
    {
        // Deixa a caixa de diálogo escondida
        caixaDialogo.SetActive(false);

        // Espera o fade terminar
        StartCoroutine(IniciarDialogo());
    }

    IEnumerator IniciarDialogo()
    {
        // Espera o tempo do Fade
        yield return new WaitForSeconds(tempoAntesDoDialogo);

        // Mostra a caixa de diálogo
        caixaDialogo.SetActive(true);

        // Começa a digitar
        StartCoroutine(DigitarTexto());
    }

    IEnumerator DigitarTexto()
    {
        TextoFalasIniciais.text = "";

        Sprite[] sprites =
        {
            rosto1,
            rosto2,
            rosto3,
            rosto4
        };

        int spriteAtual = 0;
        int trocas = 0;

        // Começa com o primeiro rosto
        RostoXomiDialogando1.sprite = rosto1;

        float contadorSprite = 0f;
        float contadorSom = 0f;

        // Quantas trocas serão feitas
        int totalTrocas = (sprites.Length * repeticoesSprites) - 1;

        foreach (char letra in fala)
        {
            // Adiciona a letra
            TextoFalasIniciais.text += letra;

            // Som da digitação
            if (letra != ' ' && audioSource != null && somTeclado != null)
            {
                if (contadorSom <= 0f)
                {
                    audioSource.PlayOneShot(somTeclado);
                    contadorSom = intervaloSom;
                }
            }

            contadorSom -= velocidadeTexto;

            // Contador dos sprites
            contadorSprite += velocidadeTexto;

            if (contadorSprite >= tempoCadaSprite && trocas < totalTrocas)
            {
                contadorSprite = 0f;

                trocas++;
                spriteAtual++;

                if (spriteAtual >= sprites.Length)
                {
                    spriteAtual = 0;
                }

                RostoXomiDialogando1.sprite = sprites[spriteAtual];
            }

            // Espera antes da próxima letra
            yield return new WaitForSeconds(velocidadeTexto);
        }

        // Volta para o primeiro rosto
        RostoXomiDialogando1.sprite = rosto1;

        // Espera antes de desaparecer
        yield return new WaitForSeconds(tempoAteSumir);

        // Esconde a caixa
        caixaDialogo.SetActive(false);
    }
}