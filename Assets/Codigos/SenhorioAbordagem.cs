using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class FalaCena
{
    [Tooltip("false = fala o senhorio, true = fala o Xomi")]
    public bool jogador;
    [TextArea(2, 4)] public string texto;
}

// Cena03: depois que a Marta sai, o senhorio aparece mais a frente, aborda o Xomi,
// corta as escolhas no meio, obriga ele a trabalhar e leva ele pela mao ate o topo do mapa (cena02).
public class SenhorioAbordagem : MonoBehaviour
{
    [Header("Quando aparece")]
    [Tooltip("A Marta arma isso quando sai. Marque na mao se quiser testar sem a Marta.")]
    public bool armado = false;
    [Tooltip("Quando o Xomi passar dessa altura (Y) o senhorio aparece")]
    public float yGatilho = -27f;
    public float distanciaSpawn = 11f;
    public float velocidadeChegada = 6f;
    public float distanciaConversa = 3.6f;

    [Header("Sprites do senhorio (sheet senhoriofinal2_spritesheet)")]
    public Sprite parado;
    public Sprite[] andandoBaixo;
    public Sprite[] andandoCima;
    public float tempoFrame = 0.12f;

    [Header("Interface (mesma caixa das outras falas)")]
    public GameObject caixaDialogo;
    public TMP_Text textoFala;
    public Image rostoImagem;
    public Sprite[] rostosSenhorio;   // senhorio Falando
    public Sprite[] rostosJogador;

    [Header("Falas")]
    public FalaCena[] falasChegada;
    public string[] textosOpcoes;
    [Tooltip("Em qual opcao (0 = primeira) o senhorio corta no meio")]
    public int interromperNaOpcao = 2;
    public int letrasAntesDeInterromper = 14;
    public FalaCena[] falasInterrupcao;

    [Header("Opcoes na tela")]
    public GameObject painelOpcoes;
    public Button[] botoesOpcao;
    public TMP_Text[] textosOpcao;
    public float larguraOpcao = 800f;
    public float alturaOpcao = 80f;
    public float espacoEntreOpcoes = 15f;
    public float margemDoXomi = 40f;
    public int tamanhoFonteOpcao = 28;

    [Header("Texto e som")]
    public float velocidadeTexto = 0.05f;
    public float tempoCadaSprite = 0.25f;
    public float tempoEntreFalas = 1.5f;
    public AudioSource audioSource;
    public AudioClip somTeclado;
    public float intervaloSom = 0.08f;

    [Header("Levar o Xomi pela mao")]
    public float velocidadeLevar = 7f;
    public float yTopo = 34f;
    [Tooltip("Onde o Xomi fica em relacao ao senhorio enquanto e levado")]
    public Vector2 posicaoXomiAoLado = new Vector2(-1.9f, -0.2f);
    public string cenaDestino = "cena02";
    public float tempoFade = 1f;

    SpriteRenderer sr;
    Transform xomi;
    Canvas canvasOpcoes;
    bool iniciou;
    bool mostrandoOpcoes;
    int totalOpcoes;

    Sprite[] animAtual;
    bool animando;
    int frame;
    float tFrame;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;
    }

    void Start()
    {
        if (caixaDialogo != null) caixaDialogo.SetActive(false);
        if (painelOpcoes != null) painelOpcoes.SetActive(false);
    }

    public void Armar() { armado = true; }

    void Update()
    {
        if (animando && animAtual != null && animAtual.Length > 0 && sr != null)
        {
            tFrame += Time.deltaTime;
            if (tFrame >= tempoFrame)
            {
                tFrame = 0f;
                frame = (frame + 1) % animAtual.Length;
                sr.sprite = animAtual[frame];
            }
        }

        if (!armado || iniciou) return;
        if (xomi == null) xomi = CutsceneUtil.AcharXomi();
        if (xomi != null && xomi.position.y >= yGatilho)
        {
            iniciou = true;
            StartCoroutine(Cena());
        }
    }

    void LateUpdate()
    {
        if (mostrandoOpcoes) SeguirXomi();
    }

    void Andar(Sprite[] frames)
    {
        animAtual = frames;
        animando = frames != null && frames.Length > 0;
        frame = 0;
        tFrame = 0f;
        if (animando) sr.sprite = frames[0];
    }

    void Parar()
    {
        animando = false;
        if (parado != null) sr.sprite = parado;
    }

    IEnumerator Cena()
    {
        // Xomi para e olha pra cima; o senhorio aparece acima da tela e anda ate ele
        CutsceneUtil.TravarXomi(true);
        CutsceneUtil.XomiOlhar(true);

        transform.position = new Vector3(xomi.position.x, xomi.position.y + distanciaSpawn, 0f);
        sr.enabled = true;
        Andar(andandoBaixo);
        while (true)
        {
            Vector3 alvo = xomi.position + Vector3.up * distanciaConversa;
            Vector3 falta = alvo - transform.position;
            if (falta.magnitude < 0.05f) break;
            transform.position += falta.normalized * Mathf.Min(velocidadeChegada * Time.deltaTime, falta.magnitude);
            yield return null;
        }
        Parar();
        yield return new WaitForSeconds(0.4f);

        // pergunta quem e e ja oferece o emprego
        caixaDialogo.SetActive(true);
        foreach (FalaCena f in falasChegada) yield return StartCoroutine(Falar(f));

        // opcoes aparecem uma a uma... e o senhorio corta no meio
        yield return StartCoroutine(Escolhas());

        // fala final ("me da a mao") e ele leva o Xomi
        caixaDialogo.SetActive(false);
        yield return StartCoroutine(Levar());
    }

    IEnumerator Escolhas()
    {
        textoFala.text = "";
        caixaDialogo.SetActive(false);

        totalOpcoes = Mathf.Min(textosOpcoes.Length, Mathf.Min(botoesOpcao.Length, textosOpcao.Length));
        PrepararLayoutOpcoes();
        painelOpcoes.SetActive(true);
        mostrandoOpcoes = true;
        SeguirXomi();

        for (int i = 0; i < botoesOpcao.Length; i++)
        {
            bool existe = i < totalOpcoes;
            botoesOpcao[i].gameObject.SetActive(existe);
            Transform painel = PainelDaOpcao(i);
            if (painel != null) painel.gameObject.SetActive(false);
            if (!existe) continue;
            textosOpcao[i].text = "";
            botoesOpcao[i].interactable = false; // o Xomi nunca chega a escolher
            botoesOpcao[i].onClick.RemoveAllListeners();
        }

        for (int i = 0; i < totalOpcoes; i++)
        {
            Transform painel = PainelDaOpcao(i);
            if (painel != null) painel.gameObject.SetActive(true);

            if (i == interromperNaOpcao)
            {
                // digita so um pedaco e o senhorio atropela
                string t = textosOpcoes[i];
                int corte = Mathf.Clamp(letrasAntesDeInterromper, 0, t.Length);
                yield return StartCoroutine(DigitarOpcao(textosOpcao[i], t.Substring(0, corte)));
                textosOpcao[i].text += "-";
                yield return new WaitForSeconds(0.5f);
                break;
            }
            yield return StartCoroutine(DigitarOpcao(textosOpcao[i], textosOpcoes[i]));
        }
        yield return new WaitForSeconds(0.3f);

        mostrandoOpcoes = false;
        painelOpcoes.SetActive(false);
        caixaDialogo.SetActive(true);
        foreach (FalaCena f in falasInterrupcao) yield return StartCoroutine(Falar(f));
    }

    IEnumerator Levar()
    {
        // Xomi sem fisica nem colisao enquanto e levado
        Rigidbody2D rbx = xomi.GetComponent<Rigidbody2D>();
        if (rbx != null) { rbx.velocity = Vector2.zero; rbx.bodyType = RigidbodyType2D.Kinematic; }
        foreach (Collider2D c in xomi.GetComponents<Collider2D>()) c.enabled = false;
        CutsceneUtil.XomiOlhar(true);
        Animator ax = xomi.GetComponent<Animator>();

        // senhorio chega ao lado do Xomi
        Andar(andandoBaixo);
        Vector3 destino = xomi.position - (Vector3)posicaoXomiAoLado;
        while ((destino - transform.position).magnitude > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destino, velocidadeChegada * Time.deltaTime);
            yield return null;
        }

        // os dois sobem juntos ate o topo do mapa
        Andar(andandoCima);
        if (ax != null) ax.SetBool("Andando", true);
        while (xomi.position.y < yTopo)
        {
            Vector3 passo = Vector3.up * velocidadeLevar * Time.deltaTime;
            xomi.position += passo;
            transform.position += passo;
            yield return null;
        }
        if (ax != null) ax.SetBool("Andando", false);
        Parar();

        TelaFade fade = TelaFade.Criar(0f);
        yield return StartCoroutine(fade.Animar(1f, tempoFade));
        Time.timeScale = 1f;
        SceneManager.LoadScene(cenaDestino);
    }

    IEnumerator Falar(FalaCena fala)
    {
        textoFala.text = "";
        Sprite[] faces = fala.jogador ? rostosJogador : rostosSenhorio;
        bool temFace = faces != null && faces.Length > 0;
        if (rostoImagem != null)
        {
            rostoImagem.enabled = temFace;
            if (temFace) rostoImagem.sprite = faces[0];
        }

        int atual = 0;
        float cSprite = 0f;
        float cSom = 0f;
        foreach (char letra in fala.texto)
        {
            textoFala.text += letra;

            if (letra != ' ' && audioSource != null && somTeclado != null && cSom <= 0f)
            {
                audioSource.PlayOneShot(somTeclado);
                cSom = intervaloSom;
            }
            cSom -= velocidadeTexto;

            cSprite += velocidadeTexto;
            if (cSprite >= tempoCadaSprite && faces != null && faces.Length > 1)
            {
                cSprite = 0f;
                atual = (atual + 1) % faces.Length;
                if (rostoImagem != null) rostoImagem.sprite = faces[atual];
            }
            yield return new WaitForSeconds(velocidadeTexto);
        }

        if (temFace && rostoImagem != null) rostoImagem.sprite = faces[0];
        yield return new WaitForSeconds(tempoEntreFalas);
    }

    IEnumerator DigitarOpcao(TMP_Text alvo, string texto)
    {
        alvo.text = "";
        float cSom = 0f;
        foreach (char letra in texto)
        {
            alvo.text += letra;
            if (letra != ' ' && audioSource != null && somTeclado != null && cSom <= 0f)
            {
                audioSource.PlayOneShot(somTeclado);
                cSom = intervaloSom;
            }
            cSom -= velocidadeTexto;
            yield return new WaitForSeconds(velocidadeTexto);
        }
        yield return new WaitForSeconds(0.2f);
    }

    // ---- layout das opcoes (igual ao do DialogoTaison) ----

    Transform PainelDaOpcao(int i)
    {
        Transform pai = botoesOpcao[i].transform.parent;
        return (pai != null && pai != painelOpcoes.transform) ? pai : null;
    }

    void PrepararLayoutOpcoes()
    {
        for (int i = 0; i < botoesOpcao.Length && i < textosOpcao.Length; i++)
        {
            RectTransform painel = PainelDaOpcao(i) as RectTransform;
            if (painel == null) continue;

            painel.pivot = new Vector2(1f, 0.5f);
            painel.sizeDelta = new Vector2(larguraOpcao, alturaOpcao);

            RectTransform botao = botoesOpcao[i].transform as RectTransform;
            botao.anchorMin = Vector2.zero;
            botao.anchorMax = Vector2.one;
            botao.offsetMin = Vector2.zero;
            botao.offsetMax = Vector2.zero;
            botao.SetAsFirstSibling();

            RectTransform texto = textosOpcao[i].rectTransform;
            texto.SetParent(painel, false);
            texto.anchorMin = Vector2.zero;
            texto.anchorMax = Vector2.one;
            texto.offsetMin = new Vector2(20f, 5f);
            texto.offsetMax = new Vector2(-20f, -5f);
            texto.SetAsLastSibling();
            textosOpcao[i].alignment = TextAlignmentOptions.MidlineLeft;
            textosOpcao[i].fontSize = tamanhoFonteOpcao;
            textosOpcao[i].raycastTarget = false;
        }
    }

    void SeguirXomi()
    {
        if (xomi == null) xomi = CutsceneUtil.AcharXomi();
        if (xomi == null || painelOpcoes == null) return;
        if (canvasOpcoes == null) canvasOpcoes = painelOpcoes.GetComponentInParent<Canvas>();
        Camera cam = Camera.main;
        if (cam == null) return;

        float escala = canvasOpcoes != null ? canvasOpcoes.scaleFactor : 1f;

        Vector3 ponto = xomi.position;
        Renderer r = xomi.GetComponent<Renderer>();
        if (r != null) ponto = new Vector3(r.bounds.min.x, r.bounds.center.y, xomi.position.z);

        Vector3 tela = cam.WorldToScreenPoint(ponto);
        float x = Mathf.Max(tela.x - margemDoXomi * escala, (larguraOpcao + 10f) * escala);

        float passo = (alturaOpcao + espacoEntreOpcoes) * escala;
        float topo = tela.y + (totalOpcoes - 1) * 0.5f * passo;

        for (int i = 0; i < totalOpcoes; i++)
        {
            Transform painel = PainelDaOpcao(i);
            if (painel != null) painel.position = new Vector3(x, topo - i * passo, 0f);
        }
    }
}
