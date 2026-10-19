using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogoTulipa : MonoBehaviour
{
    [Header("Interface")]
    public GameObject caixaDialogo;
    public TMP_Text textoFala;
    public Image rostoImagem;

    [Header("Sprites do rosto da Tulipa")]
    public Sprite[] rostos;

    [Header("Falas da Tulipa (antes da escolha)")]
    [TextArea(3, 5)] public string[] falasIniciais;

    [Header("Escolhas")]
    public GameObject painelOpcoes;
    public Button botaoOpcao1;
    public Button botaoOpcao2;
    public TMP_Text textoOpcao1;
    public TMP_Text textoOpcao2;
    public string opcao1 = "Opção 1";
    public string opcao2 = "Opção 2";

    [Header("Respostas da Tulipa")]
    [TextArea(3, 5)] public string[] respostaOpcao1;
    [TextArea(3, 5)] public string[] respostaOpcao2;

    [Header("Velocidade do texto")]
    public float velocidadeTexto = 0.05f;

    [Header("Tempo de cada sprite")]
    public float tempoCadaSprite = 0.25f;

    [Header("Tempo depois de cada fala")]
    public float tempoEntreFalas = 1.5f;

    [Header("Som de digitação")]
    public AudioSource audioSource;
    public AudioClip somTeclado;
    public float intervaloSom = 0.08f;

    [Header("Travar o jogador")]
    public MonoBehaviour scriptMovimento;

    private bool jaFalou = false;
    private int escolha = -1;

    void Start()
    {
        caixaDialogo.SetActive(false);
        painelOpcoes.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Player") && !jaFalou)
        {
            jaFalou = true;
            StartCoroutine(Dialogo());
        }
    }

    IEnumerator Dialogo()
    {
        if (scriptMovimento != null) scriptMovimento.enabled = false;
        caixaDialogo.SetActive(true);

        // Falas antes da escolha
        foreach (string fala in falasIniciais)
            yield return StartCoroutine(DigitarTexto(fala));

        // Escolha
        textoFala.text = "";
        textoOpcao1.text = opcao1;
        textoOpcao2.text = opcao2;
        escolha = -1;

        botaoOpcao1.onClick.RemoveAllListeners();
        botaoOpcao2.onClick.RemoveAllListeners();
        botaoOpcao1.onClick.AddListener(() => escolha = 0);
        botaoOpcao2.onClick.AddListener(() => escolha = 1);

        painelOpcoes.SetActive(true);
        yield return new WaitUntil(() => escolha != -1);
        painelOpcoes.SetActive(false);

        // Resposta
        string[] resposta = (escolha == 0) ? respostaOpcao1 : respostaOpcao2;
        foreach (string fala in resposta)
            yield return StartCoroutine(DigitarTexto(fala));

        caixaDialogo.SetActive(false);
        if (scriptMovimento != null) scriptMovimento.enabled = true;
    }

    IEnumerator DigitarTexto(string fala)
    {
        textoFala.text = "";

        int spriteAtual = 0;
        float contadorSprite = 0f;
        float contadorSom = 0f;

        if (rostos.Length > 0) rostoImagem.sprite = rostos[0];

        foreach (char letra in fala)
        {
            textoFala.text += letra;

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

            // Troca o rosto
            contadorSprite += velocidadeTexto;
            if (contadorSprite >= tempoCadaSprite && rostos.Length > 1)
            {
                contadorSprite = 0f;
                spriteAtual = (spriteAtual + 1) % rostos.Length;
                rostoImagem.sprite = rostos[spriteAtual];
            }

            yield return new WaitForSeconds(velocidadeTexto);
        }

        // Volta pro primeiro rosto
        if (rostos.Length > 0) rostoImagem.sprite = rostos[0];

        yield return new WaitForSeconds(tempoEntreFalas);
    }
}