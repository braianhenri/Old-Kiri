using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// Este script reutiliza Quem, Fala e OpcaoDialogo do DialogoTulipa.cs.
// No DialogoTulipa.cs, adicione "Taison" no FINAL do enum Quem:
//
//   public enum Quem { Tulipa, Jogador, Taison }
//
// (no final pra nao bagunçar o que voce ja preencheu no Inspector)

public class DialogoTaison : MonoBehaviour
{
    [Header("Interacao (apertar botao pra conversar)")]
    public GameObject iconeInteracao;           // a sprite/imagem do "aperte E"
    public KeyCode teclaInteracao = KeyCode.E;
    public bool podeRepetir = false;            // se true, da pra conversar de novo depois

    [Header("Interface")]
    public GameObject caixaDialogo;
    public TMP_Text textoFala;
    public Image rostoImagem;
    public TMP_Text textoNome; // opcional

    [Header("Rostos do Taison")]
    public Sprite[] rostos;
    public string nomeTaison = "Taison";

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
    [Header("Posicao das opcoes (ao lado do Xomi)")]
    public float larguraOpcao = 800f;
    public float alturaOpcao = 80f;
    public float espacoEntreOpcoes = 15f;
    public float margemDoXomi = 40f;
    public int tamanhoFonteOpcao = 28;

    [Header("Batalha (transicao com fade)")]
    [Tooltip("Nome da cena de batalha (precisa estar no Build Settings)")]
    public string cenaBatalha = "ModoBatalha";
    public float tempoFadeBatalha = 1f;
    private bool batalhaIniciada;

    [System.Serializable]
    public class EscolhaSeguinte
    {
        public int aposOpcao;           // indice da opcao (0 = primeira) depois da qual essa escolha aparece
        public OpcaoDialogo[] opcoes;   // novas opcoes, cada uma com sua resposta
    }

    [Header("Escolhas seguintes (depois da resposta de uma opcao)")]
    public EscolhaSeguinte[] escolhasSeguintes;

    private OpcaoDialogo[] opcoesAtuais;
    private bool mostrandoOpcoes = false;
    private Transform xomi;
    private Canvas canvasOpcoes;

    private bool jaFalou = false;
    private bool jogadorPerto = false;
    private bool dialogoAtivo = false;
    private int escolha = -1;

    void Start()
    {
        caixaDialogo.SetActive(false);
        if (painelOpcoes != null) painelOpcoes.SetActive(false);
        if (iconeInteracao != null) iconeInteracao.SetActive(false);
    }

    bool PodeInteragir()
    {
        return jogadorPerto && !dialogoAtivo && (!jaFalou || podeRepetir);
    }

    void AtualizarIcone()
    {
        if (iconeInteracao != null) iconeInteracao.SetActive(PodeInteragir());
    }

    void OnTriggerEnter2D(Collider2D outro)
    {
        if (!outro.CompareTag("Player")) return;
        jogadorPerto = true;
        AtualizarIcone();
    }

    void OnTriggerExit2D(Collider2D outro)
    {
        if (!outro.CompareTag("Player")) return;
        jogadorPerto = false;
        AtualizarIcone();
    }

    void Update()
    {
        if (PodeInteragir() && Input.GetKeyDown(teclaInteracao))
        {
            jaFalou = true;
            StartCoroutine(Dialogo());
        }
    }

    IEnumerator Dialogo()
    {
        dialogoAtivo = true;
        AtualizarIcone();
        if (scriptMovimento != null) scriptMovimento.enabled = false;
        caixaDialogo.SetActive(true);

        // Conversa antes da escolha
        foreach (Fala fala in falasIniciais)
        {
            yield return StartCoroutine(DigitarTexto(fala));
            if (fala.iniciaBatalhaDepois)
            {
                yield return StartCoroutine(IniciarBatalha());
                yield break;
            }
        }

        yield return StartCoroutine(Escolher(opcoes));
        if (batalhaIniciada) yield break; // a cena de batalha ja esta carregando

        caixaDialogo.SetActive(false);
        if (scriptMovimento != null) scriptMovimento.enabled = true;

        dialogoAtivo = false;
        AtualizarIcone();
    }

    // Mostra uma lista de opcoes, espera a escolha, roda a resposta (e a escolha seguinte, se houver)
    IEnumerator Escolher(OpcaoDialogo[] lista)
    {
        if (lista == null || lista.Length == 0 || painelOpcoes == null) yield break;
        opcoesAtuais = lista;
        {
            textoFala.text = "";
            if (textoNome != null) textoNome.text = "";

            // Esconde a caixa do Taison enquanto ninguem esta falando (so as opcoes aparecem)
            caixaDialogo.SetActive(false);
            escolha = -1;

            if (lista.Length > botoesOpcao.Length)
                Debug.LogWarning("Tem mais opcoes do que botoes! Adicione mais botoes na lista.");

            // Mostra o painel perto do Xomi (opcoes escondidas e travadas ate serem digitadas)
            PrepararLayoutOpcoes();
            painelOpcoes.SetActive(true);
            mostrandoOpcoes = true;
            SeguirXomi();

            for (int i = 0; i < botoesOpcao.Length; i++)
            {
                bool existe = OpcaoExiste(i);
                botoesOpcao[i].gameObject.SetActive(existe);
                Transform painelOp = PainelDaOpcao(i);
                if (painelOp != null) painelOp.gameObject.SetActive(false);
                if (!existe) continue;

                textosOpcao[i].text = "";
                botoesOpcao[i].interactable = false;

                int indice = i; // copia necessaria pro clique pegar o indice certo
                botoesOpcao[i].onClick.RemoveAllListeners();
                botoesOpcao[i].onClick.AddListener(() => escolha = indice);
            }

            // Digita uma opcao de cada vez: 1, depois 2, depois 3
            for (int i = 0; i < botoesOpcao.Length; i++)
            {
                if (!OpcaoExiste(i)) continue;
                Transform painelOp = PainelDaOpcao(i);
                if (painelOp != null) painelOp.gameObject.SetActive(true);
                yield return StartCoroutine(DigitarOpcao(textosOpcao[i], lista[i].textoOpcao));
            }

            // So libera o clique depois que as tres terminaram de ser digitadas
            for (int i = 0; i < botoesOpcao.Length; i++)
                if (OpcaoExiste(i)) botoesOpcao[i].interactable = true;

            yield return new WaitUntil(() => escolha != -1);
            mostrandoOpcoes = false;
            painelOpcoes.SetActive(false);

            // Alguem vai voltar a falar: mostra a caixa de novo
            caixaDialogo.SetActive(true);

            int idx = escolha;
            OpcaoDialogo escolhida = lista[idx];

            // O jogador fala a opcao que escolheu
            if (jogadorFalaAOpcao)
            {
                Fala falaDoJogador = new Fala { quem = Quem.Jogador, texto = escolhida.textoOpcao };
                yield return StartCoroutine(DigitarTexto(falaDoJogador));
            }

            // Dispara o evento da opcao (dar item, etc)
            escolhida.aoEscolher?.Invoke();

            // Opcao que leva direto para a batalha (ex: "E te interessa?")
            if (escolhida.iniciaBatalhaAoEscolher)
            {
                yield return StartCoroutine(IniciarBatalha());
                yield break;
            }

            // Resposta especifica dessa escolha
            foreach (Fala fala in escolhida.resposta)
            {
                yield return StartCoroutine(DigitarTexto(fala));
                if (fala.iniciaBatalhaDepois)
                {
                    yield return StartCoroutine(IniciarBatalha());
                    yield break;
                }
            }

            // Escolha seguinte: depois dessa resposta, o jogador escolhe de novo
            if (lista == opcoes)
                foreach (EscolhaSeguinte e in escolhasSeguintes)
                    if (e.aposOpcao == idx && e.opcoes.Length > 0)
                    {
                        yield return StartCoroutine(Escolher(e.opcoes));
                        break;
                    }
        }
    }

    // Fade leve para preto e troca para a cena de batalha. Roda uma unica vez.
    IEnumerator IniciarBatalha()
    {
        if (batalhaIniciada) yield break;
        batalhaIniciada = true;

        CutsceneUtil.TravarXomi(true);
        TelaFade fade = TelaFade.Criar(0f);
        yield return StartCoroutine(fade.Animar(1f, tempoFadeBatalha));

        Time.timeScale = 1f;
        SceneManager.LoadScene(cenaBatalha);
    }

    void LateUpdate()
    {
        if (mostrandoOpcoes) SeguirXomi();
    }

    // Painel ("Opcoes dialogo") que contem o botao da opcao i
    Transform PainelDaOpcao(int i)
    {
        Transform pai = botoesOpcao[i].transform.parent;
        return (pai != null && pai != painelOpcoes.transform) ? pai : null;
    }

    bool OpcaoExiste(int i)
    {
        return i < opcoesAtuais.Length && i < textosOpcao.Length && i < botoesOpcao.Length;
    }

    // Ajusta tamanho/alinhamento das opcoes pra ficarem como texto flutuante a esquerda do Xomi
    void PrepararLayoutOpcoes()
    {
        for (int i = 0; i < botoesOpcao.Length && i < textosOpcao.Length; i++)
        {
            RectTransform painel = PainelDaOpcao(i) as RectTransform;
            if (painel == null) continue;

            painel.pivot = new Vector2(1f, 0.5f);
            painel.sizeDelta = new Vector2(larguraOpcao, alturaOpcao);

            // Botao ocupa o painel inteiro (a area clicavel e a linha toda)
            RectTransform botao = botoesOpcao[i].transform as RectTransform;
            botao.anchorMin = Vector2.zero;
            botao.anchorMax = Vector2.one;
            botao.offsetMin = Vector2.zero;
            botao.offsetMax = Vector2.zero;
            botao.SetAsFirstSibling();

            // Texto por cima do botao, alinhado a esquerda
            RectTransform texto = textosOpcao[i].rectTransform;
            texto.SetParent(painel, false);
            texto.anchorMin = Vector2.zero;
            texto.anchorMax = Vector2.one;
            texto.offsetMin = new Vector2(20f, 5f);
            texto.offsetMax = new Vector2(-20f, -5f);
            texto.SetAsLastSibling();
            textosOpcao[i].alignment = TextAlignmentOptions.MidlineLeft;
            textosOpcao[i].fontSize = tamanhoFonteOpcao;
            textosOpcao[i].raycastTarget = false; // o clique vai pro botao
        }
    }

    // Posiciona as opcoes empilhadas, a esquerda do Xomi, acompanhando ele na tela
    void SeguirXomi()
    {
        if (xomi == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p == null) return;
            xomi = p.transform;
        }
        if (canvasOpcoes == null) canvasOpcoes = painelOpcoes.GetComponentInParent<Canvas>();
        Camera cam = Camera.main;
        if (cam == null) return;

        float escala = canvasOpcoes != null ? canvasOpcoes.scaleFactor : 1f;

        Vector3 ponto = xomi.position;
        Renderer r = xomi.GetComponent<Renderer>();
        if (r != null) ponto = new Vector3(r.bounds.min.x, r.bounds.center.y, xomi.position.z);

        Vector3 tela = cam.WorldToScreenPoint(ponto);
        float x = Mathf.Max(tela.x - margemDoXomi * escala, (larguraOpcao + 10f) * escala);

        int total = 0;
        for (int i = 0; i < botoesOpcao.Length; i++) if (OpcaoExiste(i)) total++;

        float passo = (alturaOpcao + espacoEntreOpcoes) * escala;
        float topo = tela.y + (total - 1) * 0.5f * passo;

        int posicao = 0;
        for (int i = 0; i < botoesOpcao.Length; i++)
        {
            if (!OpcaoExiste(i)) continue;
            Transform painel = PainelDaOpcao(i);
            if (painel != null)
                painel.position = new Vector3(x, topo - posicao * passo, 0f);
            posicao++;
        }
    }

    // Digita a opcao letra por letra, igual as falas
    IEnumerator DigitarOpcao(TMP_Text alvo, string texto)
    {
        alvo.text = "";
        float contadorSom = 0f;

        foreach (char letra in texto)
        {
            alvo.text += letra;

            if (letra != ' ' && audioSource != null && somTeclado != null)
            {
                if (contadorSom <= 0f)
                {
                    audioSource.PlayOneShot(somTeclado);
                    contadorSom = intervaloSom;
                }
            }
            contadorSom -= velocidadeTexto;

            yield return new WaitForSeconds(velocidadeTexto);
        }

        yield return new WaitForSeconds(0.2f);
    }

    IEnumerator DigitarTexto(Fala fala)
    {
        textoFala.text = "";

        // Qualquer fala que nao seja do Jogador e do Taison (NPC deste script)
        bool ehJogador = fala.quem == Quem.Jogador;
        Sprite[] faces = ehJogador ? rostosJogador : rostos;
        if (textoNome != null)
            textoNome.text = ehJogador ? nomeJogador : nomeTaison;

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
