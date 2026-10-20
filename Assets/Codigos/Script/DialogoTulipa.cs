using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public enum Quem
{
    Tulipa,
    Jogador
}

[System.Serializable]
public class Fala
{
    public Quem quem;
    [TextArea(3, 5)] public string texto;
}

[System.Serializable]
public class OpcaoDialogo
{
    [Tooltip("O que aparece no botao (e o que o jogador fala)")]
    public string textoOpcao;

    [Tooltip("O que acontece na conversa depois dessa escolha")]
    public Fala[] resposta;

    [Tooltip("Opcional: ligue aqui o que deve acontecer ao escolher (dar item, etc)")]
    public UnityEvent aoEscolher;
}

public class DialogoTulipa : MonoBehaviour
{
    [Header("Interface")]
    public GameObject caixaDialogo;
    public TMP_Text textoFala;
    public Image rostoImagem;
    public TMP_Text textoNome; // opcional

    [Header("Rostos da Tulipa")]
    public Sprite[] rostos;
    public string nomeTulipa = "Tulipa";

    [Header("Rostos do Jogador")]
    public Sprite[] rostosJogador;
    public string nomeJogador = "Xomi";

    [Header("Conversa antes da escolha")]
    public Fala[] falasIniciais;

    [Header("Escolhas")]
    public GameObject painelOpcoes;
    public Button[] botoesOpcao;     // arraste os botoes (ex: 3)
    public TMP_Text[] textosOpcao;   // o texto de cada botao, na MESMA ordem
    public OpcaoDialogo[] opcoes;    // cada opcao com sua resposta
    public bool jogadorFalaAOpcao = true; // o jogador "fala" a opcao escolhida

    [Header("Velocidade do texto")]
    public float velocidadeTexto = 0.05f;

    [Header("Tempo de cada sprite")]
    public float tempoCadaSprite = 0.25f;

    [Header("Tempo depois de cada fala")]
    public float tempoEntreFalas = 1.5f;

    [Header("Som de digitacao")]
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
        if (painelOpcoes != null) painelOpcoes.SetActive(false);
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

        // Conversa antes da escolha
        foreach (Fala fala in falasIniciais)
            yield return StartCoroutine(DigitarTexto(fala));

        // Escolha (so acontece se tiver opcoes cadastradas)
        if (opcoes.Length > 0 && painelOpcoes != null)
        {
            textoFala.text = "";
            if (textoNome != null) textoNome.text = "";
            escolha = -1;

            if (opcoes.Length > botoesOpcao.Length)
                Debug.LogWarning("Tem mais opcoes do que botoes! Adicione mais botoes na lista.");

            for (int i = 0; i < botoesOpcao.Length; i++)
            {
                bool existe = i < opcoes.Length && i < textosOpcao.Length;
                botoesOpcao[i].gameObject.SetActive(existe);
                if (!existe) continue;

                textosOpcao[i].text = opcoes[i].textoOpcao;

                int indice = i; // copia necessaria pro clique pegar o indice certo
                botoesOpcao[i].onClick.RemoveAllListeners();
                botoesOpcao[i].onClick.AddListener(() => escolha = indice);
            }

            painelOpcoes.SetActive(true);
            yield return new WaitUntil(() => escolha != -1);
            painelOpcoes.SetActive(false);

            OpcaoDialogo escolhida = opcoes[escolha];

            // O jogador fala a opcao que escolheu
            if (jogadorFalaAOpcao)
            {
                Fala falaDoJogador = new Fala { quem = Quem.Jogador, texto = escolhida.textoOpcao };
                yield return StartCoroutine(DigitarTexto(falaDoJogador));
            }

            // Dispara o evento da opcao (dar item, etc)
            escolhida.aoEscolher?.Invoke();

            // Resposta especifica dessa escolha
            foreach (Fala fala in escolhida.resposta)
                yield return StartCoroutine(DigitarTexto(fala));
        }

        caixaDialogo.SetActive(false);
        if (scriptMovimento != null) scriptMovimento.enabled = true;
    }

    IEnumerator DigitarTexto(Fala fala)
    {
        textoFala.text = "";

        Sprite[] faces = (fala.quem == Quem.Tulipa) ? rostos : rostosJogador;
        if (textoNome != null)
            textoNome.text = (fala.quem == Quem.Tulipa) ? nomeTulipa : nomeJogador;

        if (rostoImagem != null) rostoImagem.enabled = faces.Length > 0;

        int spriteAtual = 0;
        float contadorSprite = 0f;
        float contadorSom = 0f;

        if (faces.Length > 0 && rostoImagem != null) rostoImagem.sprite = faces[0];

        foreach (char letra in fala.texto)
        {
            textoFala.text += letra;

            if (letra != ' ' && audioSource != null && somTeclado != null)
            {
                if (contadorSom <= 0f)
                {
                    audioSource.PlayOneShot(somTeclado);
                    contadorSom = intervaloSom;
                }
            }
            contadorSom -= velocidadeTexto;

            contadorSprite += velocidadeTexto;
            if (contadorSprite >= tempoCadaSprite && faces.Length > 1)
            {
                contadorSprite = 0f;
                spriteAtual = (spriteAtual + 1) % faces.Length;
                if (rostoImagem != null) rostoImagem.sprite = faces[spriteAtual];
            }

            yield return new WaitForSeconds(velocidadeTexto);
        }

        if (faces.Length > 0 && rostoImagem != null) rostoImagem.sprite = faces[0];

        yield return new WaitForSeconds(tempoEntreFalas);
    }
}