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

    [Header("Falas seguintes (aparecem depois da primeira)")]
    [TextArea(2, 4)]
    public string[] falasSeguintes =
    {
        "Isso aqui não é Mani. Não tem nada daqui que eu reconheça.",
        "Essas ruas... essas paredes... parece um lugar que ninguém cuida.",
        "Preciso achar alguém que me diga como voltar. Qualquer um."
    };

    [Header("Movimento")]
    [Tooltip("O Xomi fica parado ate a caixa das falas iniciais sumir. Evita outra fala aparecer junto.")]
    public bool travarDuranteAsFalas = true;

    [Header("Velocidade do texto")]
    public float velocidadeTexto = 0.05f;

    [Header("Tempo de cada sprite")]
    public float tempoCadaSprite = 0.4f;

    [Header("Repeticoes dos sprites (por fala)")]
    public int repeticoesSprites = 2;

    [Header("Tempo entre as falas")]
    public float tempoEntreFalas = 1.5f;

    [Header("Tempo ate desaparecer (depois da ultima fala)")]
    public float tempoAteSumir = 3f;

    [Header("Tempo antes do dialogo")]
    public float tempoAntesDoDialogo = 3f;

    [Header("Som de digitacao")]
    public AudioSource audioSource;
    public AudioClip somTeclado;

    [Header("Intervalo entre sons")]
    public float intervaloSom = 0.08f;

    NewBehaviourScript movimento;
    Rigidbody2D rbXomi;
    Animator animXomi;

    void Start()
    {
        // Deixa a caixa de dialogo escondida
        caixaDialogo.SetActive(false);

        // O Xomi acorda parado e so anda quando as falas iniciais terminarem
        if (travarDuranteAsFalas) TravarMovimento(true);

        // Espera o fade terminar
        StartCoroutine(IniciarDialogo());
    }

    IEnumerator IniciarDialogo()
    {
        // Espera o tempo do Fade
        yield return new WaitForSeconds(tempoAntesDoDialogo);

        // Mostra a caixa de dialogo
        caixaDialogo.SetActive(true);

        // Primeira fala
        yield return StartCoroutine(DigitarTexto(fala));

        // Falas seguintes, uma depois da outra
        if (falasSeguintes != null)
        {
            foreach (string proxima in falasSeguintes)
            {
                if (string.IsNullOrEmpty(proxima)) continue;

                yield return new WaitForSeconds(tempoEntreFalas);
                yield return StartCoroutine(DigitarTexto(proxima));
            }
        }

        // Volta para o primeiro rosto
        RostoXomiDialogando1.sprite = rosto1;

        // Espera antes de desaparecer
        yield return new WaitForSeconds(tempoAteSumir);

        // Esconde a caixa
        caixaDialogo.SetActive(false);

        // Agora sim libera o Xomi
        if (travarDuranteAsFalas) TravarMovimento(false);
    }

    void TravarMovimento(bool travar)
    {
        if (movimento == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p == null) return;

            movimento = p.GetComponent<NewBehaviourScript>();
            rbXomi = p.GetComponent<Rigidbody2D>();
            animXomi = p.GetComponent<Animator>();
        }

        if (movimento == null) return;

        movimento.enabled = !travar;

        if (travar)
        {
            if (rbXomi != null) rbXomi.velocity = Vector2.zero;
            if (animXomi != null) animXomi.SetBool("Andando", false);
        }
    }

    IEnumerator DigitarTexto(string texto)
    {
        TextoFalasIniciais.text = "";

        Sprite[] sprites = { rosto1, rosto2, rosto3, rosto4 };

        int spriteAtual = 0;
        int trocas = 0;

        // Comeca com o primeiro rosto
        RostoXomiDialogando1.sprite = rosto1;

        float contadorSprite = 0f;
        float contadorSom = 0f;

        // Quantas trocas serao feitas nesta fala
        int totalTrocas = (sprites.Length * repeticoesSprites) - 1;

        foreach (char letra in texto)
        {
            // Adiciona a letra
            TextoFalasIniciais.text += letra;

            // Som da digitacao
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

            // Espera antes da proxima letra
            yield return new WaitForSeconds(velocidadeTexto);
        }

        // Volta para o primeiro rosto ao terminar a fala
        RostoXomiDialogando1.sprite = rosto1;
    }
}
