using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

/// <summary>
/// Inimigo do modo de batalha (NPC "Suspeito"). Monta o corpo a partir das fatias de
/// combate_suspeito, reconhece a regiao atingida, aplica dano por regiao, solta as pecas
/// perdidas e executa as animacoes/efeitos (tudo em pixel art, sem efeitos graficos).
/// Nao conhece o mapa nem a historia: o ControladorCombate decide quando usar cada coisa.
/// </summary>
public class InimigoBatalha : MonoBehaviour
{
    public enum EstadoInimigo { Parado, Atacando, Atingido, Atordoado, Derrotado }

    public struct ResultadoDano
    {
        public TipoRegiao regiao;
        public int danoAplicado;
        public bool partePerdida;
        public bool derrotou;
        public bool atordoou;
    }

    [Header("Identidade")]
    [SerializeField] private string nomeInimigo = "Taison";

    [Header("Sprites (fatias de combate_suspeito)")]
    [SerializeField] private Sprite corpoCompleto;
    [SerializeField] private Sprite troncoSemCabeca;
    [SerializeField] private RectInt posicaoTroncoSemCabeca = new RectInt(10, 29, 57, 86);
    [SerializeField] private float escala = 3.4f;
    [SerializeField] private int pixelsPorUnidade = 100;
    [SerializeField] private int ordemDeDesenho = 5;

    [Header("Forca (define a vida e a chance de acertar)")]
    [Tooltip("Quanto MAIOR a forca, MAIOR a vida e MENOR a chance de o inimigo errar o ataque. Quanto menor, o contrario.")]
    [Range(1, 20)]
    [SerializeField] private int forca = 10;
    [Tooltip("Forca de referencia: com a forca igual a este valor, a vida e a precisao ficam exatamente nos valores base.")]
    [SerializeField] private int forcaReferencia = 10;

    [Header("Vida e regioes")]
    [Tooltip("VIDA do inimigo (pode mexer aqui, inclusive durante o Play). Vida maxima real = vida x forca / forca de referencia.")]
    [FormerlySerializedAs("vidaTotalMaxima")]
    [SerializeField] private int vidaBase = 90;
    [SerializeField] private List<RegiaoCorpo> regioes = new List<RegiaoCorpo>();

    [Header("Ataque do inimigo")]
    [SerializeField] private int danoFaca = 14;
    [SerializeField] private int danoInvestida = 7;
    [Tooltip("Chance de ACERTAR o ataque quando a forca e igual a forca de referencia.")]
    [Range(0f, 1f)]
    [SerializeField] private float precisaoNaForcaReferencia = 0.9f;
    [Tooltip("Quanto a chance de acertar sobe (ou desce) a cada ponto de forca acima (ou abaixo) da referencia.")]
    [SerializeField] private float precisaoPorPontoDeForca = 0.02f;
    [Range(0f, 1f)] [SerializeField] private float precisaoMinima = 0.35f;
    [Range(0f, 1f)] [SerializeField] private float precisaoMaxima = 0.99f;
    [SerializeField] private float penalidadePorPerna = 0.2f;
    [Tooltip("Ponto do ombro do braco armado no sprite do corpo (pixels).")]
    [SerializeField] private Vector2Int ombroBracoArmado = new Vector2Int(13, 85);

    [Header("Efeitos")]
    [SerializeField] private Color corRealce = new Color(0.82f, 0.55f, 1f, 1f);
    [SerializeField] private Color corPoeira = new Color(0.6f, 0.5f, 0.78f, 1f);
    [SerializeField] private TMP_FontAsset fontePopup;
    [SerializeField] private float intensidadeTremor = 0.12f;

    public event Action<int, int> OnVidaMudou;
    public event Action<RegiaoCorpo> OnRegiaoPerdida;
    public event Action OnDerrotado;

    // ---- estado ----
    private int vidaAtual;
    private int vidaMaximaConhecida;
    private bool derrotado;
    private bool atordoado;
    private EstadoInimigo estado = EstadoInimigo.Parado;
    private int animacoesAtivas;

    // ---- visual ----
    private Transform visual;
    private Transform pivoBraco;
    private SpriteRenderer corpoRenderer;
    private SpriteRenderer bracoRenderer;
    private SpriteRenderer golpeRenderer;
    private SpriteRenderer realceRenderer;
    private Material materialSprite;

    private int larg;
    private int alt;
    private Color32[] pixelsOriginais;
    private readonly Dictionary<TipoRegiao, Color32[]> pixelsPecas = new Dictionary<TipoRegiao, Color32[]>();
    private Color32[] pixelsTronco;
    private int troncoLarg;
    private int troncoAlt;

    private readonly List<UnityEngine.Object> descartaveisCorpo = new List<UnityEngine.Object>();
    private readonly List<UnityEngine.Object> descartaveisFixos = new List<UnityEngine.Object>();
    private readonly Dictionary<TipoRegiao, Sprite> spritesRealce = new Dictionary<TipoRegiao, Sprite>();
    private Sprite spriteCorpo;
    private Sprite spriteCorpoBranco;
    private Sprite spriteSemBraco;
    private Sprite spriteQuadrado;

    // ---- animacao ----
    private Vector3 offsetAnim = Vector3.zero;
    private float rotAnim;
    private Vector3 escalaAnim = Vector3.one;
    private float tremor;
    private float alturaAtualPx;
    private float alturaAlvoPx;
    private float rotPosturaAtual;
    private float rotPosturaAlvo;
    private bool realceAtivo;
    private float proximaEstrela;

    private class Particula
    {
        public Transform t;
        public SpriteRenderer sr;
        public Vector3 vel;
        public float gravidade;
        public float vida;
        public float tempo;
        public Color cor;
    }

    // ================= propriedades publicas =================

    // vida maxima real: vida base escalada pela forca
    private int vidaTotalMaxima { get { return CalcularVidaMaxima(); } }

    public string Nome { get { return nomeInimigo; } }

    public int Forca
    {
        get { return forca; }
        set
        {
            forca = Mathf.Max(1, value);
            if (Application.isPlaying && materialSprite != null) AplicarNovaVidaMaxima();
        }
    }

    public int VidaBase
    {
        get { return vidaBase; }
        set
        {
            vidaBase = Mathf.Max(1, value);
            if (Application.isPlaying && materialSprite != null) AplicarNovaVidaMaxima();
        }
    }

    public int CalcularVidaMaxima()
    {
        float fator = forcaReferencia > 0 ? (float)forca / forcaReferencia : 1f;
        return Mathf.Max(1, Mathf.RoundToInt(vidaBase * fator));
    }

    /// <summary>Chance de acertar que vem so da forca (sem penalidades de pernas).</summary>
    public float PrecisaoDaForca
    {
        get
        {
            float p = precisaoNaForcaReferencia + (forca - forcaReferencia) * precisaoPorPontoDeForca;
            return Mathf.Clamp(p, precisaoMinima, precisaoMaxima);
        }
    }
    public int VidaAtual { get { return vidaAtual; } }
    public int VidaMaxima { get { return vidaTotalMaxima; } }
    public bool Derrotado { get { return derrotado; } }
    public bool Ocupado { get { return animacoesAtivas > 0; } }
    public EstadoInimigo Estado { get { return estado; } }
    public IReadOnlyList<RegiaoCorpo> Regioes { get { return regioes; } }
    public bool PodeAtacarComFaca { get { return !EstaPerdida(TipoRegiao.BracoArmado); } }
    public bool PodeAtacar { get { return !derrotado && !(EstaPerdida(TipoRegiao.PernaEsquerda) && EstaPerdida(TipoRegiao.PernaDireita)); } }
    public int DanoFaca { get { return danoFaca; } }
    public int DanoInvestida { get { return Mathf.RoundToInt(danoInvestida * (EstaPerdida(TipoRegiao.BracoLivre) ? 0.8f : 1f)); } }

    public float PrecisaoAtaque
    {
        get
        {
            int pernas = (EstaPerdida(TipoRegiao.PernaEsquerda) ? 1 : 0) + (EstaPerdida(TipoRegiao.PernaDireita) ? 1 : 0);
            return Mathf.Clamp01(PrecisaoDaForca - pernas * penalidadePorPerna);
        }
    }

    // ================= ciclo de vida =================

    private void Awake()
    {
        if (corpoCompleto == null || regioes.Count == 0)
        {
            Debug.LogError("InimigoBatalha: faltam as fatias do corpo ou as regioes. Use 'Configurar com fatias' no componente.", this);
            enabled = false;
            return;
        }

        materialSprite = CriarMaterial();
        PrepararPixels();
        ConstruirHierarquia();
        CriarSpritesAuxiliares();
        ZerarEstado();
        ReconstruirCorpo();
    }

    private void OnDestroy()
    {
        foreach (UnityEngine.Object o in descartaveisCorpo) { if (o != null) Destroy(o); }
        foreach (UnityEngine.Object o in descartaveisFixos) { if (o != null) Destroy(o); }
        foreach (KeyValuePair<TipoRegiao, Sprite> par in spritesRealce)
        {
            if (par.Value != null)
            {
                if (par.Value.texture != null) Destroy(par.Value.texture);
                Destroy(par.Value);
            }
        }
        if (materialSprite != null) Destroy(materialSprite);
    }

    private void LateUpdate()
    {
        if (visual == null) return;

        alturaAtualPx = Mathf.MoveTowards(alturaAtualPx, alturaAlvoPx, 80f * Time.deltaTime);
        rotPosturaAtual = Mathf.MoveTowards(rotPosturaAtual, rotPosturaAlvo, 30f * Time.deltaTime);

        float respiracao = derrotado ? 0f : Mathf.Sin(Time.time * 2.3f) * 0.01f;
        Vector3 jitter = Vector3.zero;
        if (tremor > 0.001f)
        {
            jitter = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f) * tremor * intensidadeTremor;
            tremor = Mathf.MoveTowards(tremor, 0f, Time.deltaTime * 3f);
        }

        float balanco = atordoado ? Mathf.Sin(Time.time * 9f) * 3f : 0f;

        visual.localPosition = offsetAnim + jitter + new Vector3(0f, respiracao - (alturaAtualPx / pixelsPorUnidade) * escala, 0f);
        visual.localRotation = Quaternion.Euler(0f, 0f, rotAnim + rotPosturaAtual + balanco);
        visual.localScale = new Vector3(escala * escalaAnim.x, escala * escalaAnim.y, escala * escalaAnim.z);

        if (realceAtivo && realceRenderer != null)
        {
            Color c = corRealce;
            c.a = 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.time * 6f));
            realceRenderer.color = c;
        }

        if (atordoado && !derrotado && Time.time >= proximaEstrela)
        {
            proximaEstrela = Time.time + 0.28f;
            EmitirParticulas(PontoRegiaoMundo(TipoRegiao.Cabeca) + Vector3.up * 0.9f, 2, new Color(1f, 0.92f, 0.5f, 1f), 0.4f, 1.2f, -1f, 0.07f, 0.12f, 0.7f);
        }
    }

    private void OnValidate()
    {
        forca = Mathf.Max(1, forca);
        forcaReferencia = Mathf.Max(1, forcaReferencia);
        vidaBase = Mathf.Max(1, vidaBase);
        if (!Application.isPlaying || materialSprite == null) return;
        AplicarNovaVidaMaxima();
    }

    // reaplica a vida maxima (forca ou vida base mudaram), mantendo a proporcao da vida atual
    private void AplicarNovaVidaMaxima()
    {
        int nova = vidaTotalMaxima;
        if (nova == vidaMaximaConhecida) return;
        if (!derrotado)
        {
            float proporcao = vidaMaximaConhecida > 0 ? (float)vidaAtual / vidaMaximaConhecida : 1f;
            vidaAtual = Mathf.Clamp(Mathf.RoundToInt(nova * proporcao), 1, nova);
        }
        vidaMaximaConhecida = nova;
        if (OnVidaMudou != null) OnVidaMudou.Invoke(vidaAtual, nova);
    }

    // ================= configuracao =================

    /// <summary>Preenche as regioes padrao a partir das 7 fatias de combate_suspeito (0 a 6).</summary>
    public void ConfigurarComFatias(Sprite[] f)
    {
        corpoCompleto = f[6];
        troncoSemCabeca = f[4];
        regioes = new List<RegiaoCorpo>
        {
            NovaRegiao(TipoRegiao.Cabeca, "Cabeça", 40, 2f, 0.70f, new RectInt(11, 106, 42, 39), f[3], new RectInt(11, 106, 42, 39), 1),
            NovaRegiao(TipoRegiao.BracoArmado, "Braço da faca", 24, 1.5f, 0.75f, new RectInt(0, 35, 27, 53), f[0], new RectInt(0, 35, 27, 53), 0),
            NovaRegiao(TipoRegiao.Tronco, "Tronco", 45, 1f, 0.95f, new RectInt(10, 29, 57, 86), null, new RectInt(10, 29, 57, 86), 0),
            NovaRegiao(TipoRegiao.BracoLivre, "Braço livre", 24, 1.5f, 0.75f, new RectInt(49, 64, 22, 28), f[1], new RectInt(49, 64, 22, 28), 0),
            NovaRegiao(TipoRegiao.PernaEsquerda, "Perna esquerda", 24, 1.25f, 0.85f, new RectInt(15, 0, 27, 40), f[2], new RectInt(15, 0, 27, 40), 0),
            NovaRegiao(TipoRegiao.PernaDireita, "Perna direita", 24, 1.25f, 0.85f, new RectInt(54, 0, 27, 37), f[5], new RectInt(54, 0, 27, 37), 0)
        };
    }

    private static RegiaoCorpo NovaRegiao(TipoRegiao tipo, string nome, int vida, float mult, float prec, RectInt area, Sprite peca, RectInt posPeca, int expandir)
    {
        RegiaoCorpo r = new RegiaoCorpo();
        r.tipo = tipo;
        r.nome = nome;
        r.vidaMaxima = vida;
        r.multiplicadorDano = mult;
        r.precisao = prec;
        r.areaPixels = area;
        r.pecaSolta = peca;
        r.posicaoPeca = posPeca;
        r.destacavel = peca != null;
        r.expandirRecorte = expandir;
        return r;
    }

    // ================= consultas =================

    public RegiaoCorpo Obter(TipoRegiao tipo)
    {
        for (int i = 0; i < regioes.Count; i++)
        {
            if (regioes[i].tipo == tipo) return regioes[i];
        }
        return null;
    }

    public bool EstaPerdida(TipoRegiao tipo)
    {
        RegiaoCorpo r = Obter(tipo);
        return r != null && r.perdida;
    }

    /// <summary>Centro da regiao em coordenadas de mundo.</summary>
    public Vector3 PontoRegiaoMundo(TipoRegiao tipo)
    {
        RegiaoCorpo r = Obter(tipo);
        if (r == null || visual == null) return transform.position;
        Vector3 local = new Vector3(
            (r.areaPixels.x + r.areaPixels.width * 0.5f - larg * 0.5f) / pixelsPorUnidade,
            (r.areaPixels.y + r.areaPixels.height * 0.5f) / pixelsPorUnidade,
            0f);
        return visual.TransformPoint(local);
    }

    /// <summary>Reconhece qual regiao esta sob um ponto do mundo (mouse, por exemplo).</summary>
    public bool TentarObterRegiaoNoPonto(Vector3 mundo, out TipoRegiao tipo)
    {
        tipo = TipoRegiao.Tronco;
        if (visual == null) return false;

        mundo.z = visual.position.z;
        Vector3 local = visual.InverseTransformPoint(mundo);
        int px = Mathf.FloorToInt(local.x * pixelsPorUnidade + larg * 0.5f);
        int py = Mathf.FloorToInt(local.y * pixelsPorUnidade);

        TipoRegiao[] prioridade =
        {
            TipoRegiao.Cabeca, TipoRegiao.BracoArmado, TipoRegiao.BracoLivre,
            TipoRegiao.PernaEsquerda, TipoRegiao.PernaDireita, TipoRegiao.Tronco
        };

        for (int i = 0; i < prioridade.Length; i++)
        {
            RegiaoCorpo r = Obter(prioridade[i]);
            if (r == null || r.perdida) continue;
            if (px >= r.areaPixels.xMin && px < r.areaPixels.xMax && py >= r.areaPixels.yMin && py < r.areaPixels.yMax)
            {
                tipo = prioridade[i];
                return true;
            }
        }
        return false;
    }

    public bool ConsumirAtordoamento()
    {
        if (!atordoado) return false;
        atordoado = false;
        if (!derrotado) estado = EstadoInimigo.Parado;
        return true;
    }

    public IEnumerator AguardarLivre()
    {
        while (Ocupado) yield return null;
    }

    // ================= dano =================

    /// <summary>Aplica dano numa regiao e dispara as reacoes visuais.</summary>
    public ResultadoDano ReceberDano(TipoRegiao tipo, int dano)
    {
        ResultadoDano res = new ResultadoDano();
        res.regiao = tipo;

        RegiaoCorpo r = Obter(tipo);
        if (r == null || r.perdida || derrotado) return res;

        dano = Mathf.Max(1, dano);
        res.danoAplicado = dano;

        vidaAtual = Mathf.Max(0, vidaAtual - dano);
        r.vidaAtual = Mathf.Max(0, r.vidaAtual - dano);
        if (OnVidaMudou != null) OnVidaMudou.Invoke(vidaAtual, vidaTotalMaxima);

        bool perdeu = r.vidaAtual <= 0;
        if (perdeu)
        {
            r.perdida = true;
            if (OnRegiaoPerdida != null) OnRegiaoPerdida.Invoke(r);
        }

        bool morreu = vidaAtual <= 0 || (perdeu && (tipo == TipoRegiao.Tronco || tipo == TipoRegiao.Cabeca));
        if (tipo == TipoRegiao.Cabeca && !perdeu && dano >= r.vidaMaxima * 0.4f)
        {
            atordoado = true;
            res.atordoou = true;
        }

        res.partePerdida = perdeu;
        res.derrotou = morreu;
        if (morreu)
        {
            derrotado = true;
            if (vidaAtual > 0)
            {
                // cabeca ou tronco destruidos: a barra de vida acaba junto
                vidaAtual = 0;
                if (OnVidaMudou != null) OnVidaMudou.Invoke(vidaAtual, vidaTotalMaxima);
            }
        }

        Color corNumero = perdeu ? new Color(1f, 0.55f, 0.55f, 1f) : Color.white;
        MostrarTextoFlutuante(dano.ToString(), PontoRegiaoMundo(tipo) + new Vector3(0.4f, 0.3f, 0f), corNumero);

        StartCoroutine(Contado(ReagirAoGolpe(r, dano, perdeu, morreu)));
        return res;
    }

    // ================= animacoes publicas =================

    /// <summary>Ataque do inimigo. aoImpactar e chamado no momento do golpe.</summary>
    public IEnumerator AnimarAtaque(bool comFaca, Action aoIniciarTelegrafo, Action aoImpactar)
    {
        animacoesAtivas++;
        estado = EstadoInimigo.Atacando;
        bool usarBraco = comFaca && PodeAtacarComFaca && bracoRenderer != null;

        if (usarBraco)
        {
            corpoRenderer.sprite = spriteSemBraco;
            bracoRenderer.enabled = true;
            pivoBraco.localRotation = Quaternion.identity;
        }

        if (aoIniciarTelegrafo != null) aoIniciarTelegrafo.Invoke();
        MostrarTextoFlutuante("!", PontoRegiaoMundo(TipoRegiao.Cabeca) + new Vector3(0f, 1.1f, 0f), new Color(1f, 0.85f, 0.4f, 1f));

        // preparacao
        yield return StartCoroutine(Interpolar(0.6f, t =>
        {
            float k = Mathf.SmoothStep(0f, 1f, t);
            escalaAnim = Vector3.one * (1f - 0.04f * k);
            offsetAnim = new Vector3(0f, 0.05f * k, 0f);
            if (usarBraco) pivoBraco.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, -140f, k));
        }));

        // golpe
        bool impactou = false;
        yield return StartCoroutine(Interpolar(0.14f, t =>
        {
            escalaAnim = Vector3.one * Mathf.Lerp(0.96f, 1.3f, t);
            offsetAnim = new Vector3(0f, Mathf.Lerp(0.05f, -0.2f, t), 0f);
            if (usarBraco) pivoBraco.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-140f, 70f, t));
            if (!impactou && t >= 0.55f)
            {
                impactou = true;
                if (usarBraco) MostrarGolpe();
                if (aoImpactar != null) aoImpactar.Invoke();
            }
        }));
        if (!impactou && aoImpactar != null) aoImpactar.Invoke();

        yield return new WaitForSeconds(0.18f);

        // recuo
        yield return StartCoroutine(Interpolar(0.3f, t =>
        {
            float k = Mathf.SmoothStep(0f, 1f, t);
            escalaAnim = Vector3.one * Mathf.Lerp(1.3f, 1f, k);
            offsetAnim = new Vector3(0f, Mathf.Lerp(-0.2f, 0f, k), 0f);
            if (usarBraco) pivoBraco.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(70f, 0f, k));
        }));

        escalaAnim = Vector3.one;
        offsetAnim = Vector3.zero;
        if (golpeRenderer != null) golpeRenderer.enabled = false;
        if (usarBraco)
        {
            bracoRenderer.enabled = false;
            corpoRenderer.sprite = spriteCorpo;
        }
        if (!derrotado) estado = atordoado ? EstadoInimigo.Atordoado : EstadoInimigo.Parado;
        animacoesAtivas--;
    }

    /// <summary>Inimigo desvia quando o jogador erra.</summary>
    public IEnumerator AnimarEsquiva()
    {
        animacoesAtivas++;
        float lado = Random.value < 0.5f ? -1f : 1f;
        yield return StartCoroutine(Interpolar(0.35f, t =>
        {
            float k = Mathf.Sin(t * Mathf.PI);
            offsetAnim = new Vector3(0.55f * lado * k, 0f, 0f);
            rotAnim = -5f * lado * k;
        }));
        offsetAnim = Vector3.zero;
        rotAnim = 0f;
        animacoesAtivas--;
    }

    // ================= realce da regiao escolhida =================

    public void MostrarRealce(TipoRegiao? tipo)
    {
        if (realceRenderer == null) return;
        if (!tipo.HasValue)
        {
            realceAtivo = false;
            realceRenderer.enabled = false;
            return;
        }

        RegiaoCorpo r = Obter(tipo.Value);
        if (r == null) return;

        Sprite s;
        if (!spritesRealce.TryGetValue(tipo.Value, out s))
        {
            s = CriarMoldura(r.areaPixels.width + 4, r.areaPixels.height + 4);
            spritesRealce[tipo.Value] = s;
        }

        realceRenderer.sprite = s;
        realceRenderer.transform.localPosition = new Vector3(
            (r.areaPixels.x + r.areaPixels.width * 0.5f - larg * 0.5f) / pixelsPorUnidade,
            (r.areaPixels.y + r.areaPixels.height * 0.5f) / pixelsPorUnidade,
            0f);
        realceRenderer.enabled = true;
        realceAtivo = true;
    }

    // ================= reinicio =================

    public void Resetar()
    {
        StopAllCoroutines();
        animacoesAtivas = 0;
        ZerarEstado();
        ReconstruirCorpo();
        if (corpoRenderer != null) corpoRenderer.color = Color.white;
        if (bracoRenderer != null) bracoRenderer.enabled = false;
        if (golpeRenderer != null) golpeRenderer.enabled = false;
        MostrarRealce(null);
        if (OnVidaMudou != null) OnVidaMudou.Invoke(vidaAtual, vidaTotalMaxima);
    }

    private void ZerarEstado()
    {
        vidaAtual = vidaTotalMaxima;
        vidaMaximaConhecida = vidaTotalMaxima;
        derrotado = false;
        atordoado = false;
        estado = EstadoInimigo.Parado;
        offsetAnim = Vector3.zero;
        rotAnim = 0f;
        escalaAnim = Vector3.one;
        tremor = 0f;
        alturaAtualPx = 0f;
        alturaAlvoPx = 0f;
        rotPosturaAtual = 0f;
        rotPosturaAlvo = 0f;
        for (int i = 0; i < regioes.Count; i++)
        {
            regioes[i].vidaAtual = regioes[i].vidaMaxima;
            regioes[i].perdida = false;
        }
    }

    // ================= reacoes ao golpe =================

    private IEnumerator Contado(IEnumerator rotina)
    {
        animacoesAtivas++;
        yield return StartCoroutine(rotina);
        animacoesAtivas--;
    }

    private IEnumerator ReagirAoGolpe(RegiaoCorpo r, int dano, bool perdeu, bool morreu)
    {
        estado = EstadoInimigo.Atingido;
        float forca = Mathf.Clamp01(dano / 25f);
        tremor = 0.6f + forca;

        // piscada branca (pixel art)
        corpoRenderer.sprite = spriteCorpoBranco;
        EmitirParticulas(PontoRegiaoMundo(r.tipo), 6 + Mathf.RoundToInt(forca * 6f), corPoeira, 1.5f, 4f, 6f, 0.05f, 0.13f, 0.5f);
        yield return new WaitForSeconds(0.07f);
        corpoRenderer.sprite = spriteCorpo;

        // reacao especifica por regiao
        TipoRegiao tipo = r.tipo;
        yield return StartCoroutine(Interpolar(0.3f, t =>
        {
            float k = Mathf.Sin(t * Mathf.PI);
            switch (tipo)
            {
                case TipoRegiao.Cabeca:
                    rotAnim = 9f * k;
                    offsetAnim = new Vector3(0.06f * k, -0.12f * k, 0f);
                    break;
                case TipoRegiao.Tronco:
                    escalaAnim = new Vector3(1f + 0.05f * k, 1f - 0.07f * k, 1f);
                    offsetAnim = new Vector3(0f, -0.05f * k, 0f);
                    break;
                case TipoRegiao.BracoArmado:
                    offsetAnim = new Vector3(0.25f * k, 0f, 0f);
                    rotAnim = -4f * k;
                    break;
                case TipoRegiao.BracoLivre:
                    offsetAnim = new Vector3(-0.25f * k, 0f, 0f);
                    rotAnim = 4f * k;
                    break;
                case TipoRegiao.PernaEsquerda:
                    rotAnim = 6f * k;
                    offsetAnim = new Vector3(0f, -0.12f * k, 0f);
                    break;
                case TipoRegiao.PernaDireita:
                    rotAnim = -6f * k;
                    offsetAnim = new Vector3(0f, -0.12f * k, 0f);
                    break;
            }
        }));
        offsetAnim = Vector3.zero;
        rotAnim = 0f;
        escalaAnim = Vector3.one;

        if (perdeu)
        {
            DestacarPeca(r);
            ReconstruirCorpo();
            RecalcularPostura();
            tremor = 1.4f;
            yield return new WaitForSeconds(0.25f);
        }

        if (morreu)
        {
            yield return StartCoroutine(AnimarDerrota());
        }
        else
        {
            estado = atordoado ? EstadoInimigo.Atordoado : EstadoInimigo.Parado;
        }
    }

    private IEnumerator AnimarDerrota()
    {
        estado = EstadoInimigo.Derrotado;
        atordoado = false;
        float lado = Random.value < 0.5f ? -1f : 1f;

        yield return StartCoroutine(Interpolar(0.7f, t =>
        {
            float k = t * t;
            rotAnim = 85f * lado * k;
            offsetAnim = new Vector3(0.9f * -lado * k, -0.25f * k, 0f);
        }));

        EmitirParticulas(transform.position + new Vector3(0f, 0.4f, 0f), 16, corPoeira, 1.5f, 5f, 5f, 0.06f, 0.16f, 0.7f);
        yield return new WaitForSeconds(0.25f);

        yield return StartCoroutine(Interpolar(0.6f, t =>
        {
            corpoRenderer.color = new Color(1f, 1f, 1f, 1f - t);
        }));

        if (OnDerrotado != null) OnDerrotado.Invoke();
    }

    private void RecalcularPostura()
    {
        bool esq = EstaPerdida(TipoRegiao.PernaEsquerda);
        bool dir = EstaPerdida(TipoRegiao.PernaDireita);
        alturaAlvoPx = ((esq ? 1 : 0) + (dir ? 1 : 0)) * 14f;
        rotPosturaAlvo = (esq && !dir) ? 6f : ((dir && !esq) ? -6f : 0f);
    }

    // ================= pecas soltas e particulas =================

    private void DestacarPeca(RegiaoCorpo r)
    {
        Color32[] px;
        if (!r.destacavel || !pixelsPecas.TryGetValue(r.tipo, out px)) return;

        int pw = r.posicaoPeca.width;
        int ph = r.posicaoPeca.height;
        Texture2D tex = new Texture2D(pw, ph, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels32(px);
        tex.Apply();
        Sprite sp = Sprite.Create(tex, new Rect(0, 0, pw, ph), new Vector2(0.5f, 0.5f), pixelsPorUnidade, 0, SpriteMeshType.FullRect);

        Vector3 centroLocal = new Vector3(
            (r.posicaoPeca.x + pw * 0.5f - larg * 0.5f) / pixelsPorUnidade,
            (r.posicaoPeca.y + ph * 0.5f) / pixelsPorUnidade,
            0f);
        Vector3 pos = visual.TransformPoint(centroLocal);

        GameObject go = new GameObject("Peca_" + r.tipo);
        go.transform.position = pos;
        go.transform.rotation = visual.rotation;
        go.transform.localScale = visual.lossyScale;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sp;
        sr.sharedMaterial = materialSprite;
        sr.sortingOrder = ordemDeDesenho + 4;

        Vector3 vel;
        switch (r.tipo)
        {
            case TipoRegiao.Cabeca: vel = new Vector3(Random.Range(-1.5f, 1.5f), 7f, 0f); break;
            case TipoRegiao.BracoArmado: vel = new Vector3(-3.5f, 5f, 0f); break;
            case TipoRegiao.BracoLivre: vel = new Vector3(3.5f, 5f, 0f); break;
            case TipoRegiao.PernaEsquerda: vel = new Vector3(-3f, 3.5f, 0f); break;
            default: vel = new Vector3(3f, 3.5f, 0f); break;
        }

        EmitirParticulas(pos, 14, corPoeira, 1.5f, 5f, 5f, 0.06f, 0.16f, 0.6f);
        StartCoroutine(AnimarPeca(go, sr, tex, sp, vel, Random.Range(240f, 420f) * Mathf.Sign(vel.x == 0f ? 1f : vel.x)));
    }

    private IEnumerator AnimarPeca(GameObject go, SpriteRenderer sr, Texture2D tex, Sprite sp, Vector3 vel, float giro)
    {
        float tempo = 0f;
        const float duracao = 1.2f;
        while (tempo < duracao)
        {
            float dt = Time.deltaTime;
            tempo += dt;
            vel.y -= 18f * dt;
            go.transform.position += vel * dt;
            go.transform.Rotate(0f, 0f, giro * dt);
            float restante = duracao - tempo;
            if (restante < 0.4f) sr.color = new Color(1f, 1f, 1f, Mathf.Clamp01(restante / 0.4f));
            yield return null;
        }
        Destroy(go);
        Destroy(sp);
        Destroy(tex);
    }

    private void EmitirParticulas(Vector3 pos, int quantidade, Color cor, float velMin, float velMax, float gravidade, float tamMin, float tamMax, float vidaMax)
    {
        if (spriteQuadrado == null) return;
        List<Particula> lista = new List<Particula>();
        for (int i = 0; i < quantidade; i++)
        {
            GameObject go = new GameObject("Particula");
            go.transform.position = pos + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0f);
            go.transform.localScale = Vector3.one * Random.Range(tamMin, tamMax);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = spriteQuadrado;
            sr.sharedMaterial = materialSprite;
            sr.sortingOrder = ordemDeDesenho + 6;
            sr.color = cor;

            float ang = Random.Range(0f, Mathf.PI * 2f);
            float v = Random.Range(velMin, velMax);

            Particula p = new Particula();
            p.t = go.transform;
            p.sr = sr;
            p.vel = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang) * 0.8f + 0.4f, 0f) * v;
            p.gravidade = gravidade;
            p.vida = Random.Range(vidaMax * 0.5f, vidaMax);
            p.cor = cor;
            lista.Add(p);
        }
        StartCoroutine(AnimarParticulas(lista));
    }

    private IEnumerator AnimarParticulas(List<Particula> lista)
    {
        while (lista.Count > 0)
        {
            float dt = Time.deltaTime;
            for (int i = lista.Count - 1; i >= 0; i--)
            {
                Particula p = lista[i];
                p.tempo += dt;
                if (p.tempo >= p.vida)
                {
                    if (p.t != null) Destroy(p.t.gameObject);
                    lista.RemoveAt(i);
                    continue;
                }
                p.vel.y -= p.gravidade * dt;
                p.t.position += p.vel * dt;
                Color c = p.cor;
                c.a = 1f - (p.tempo / p.vida);
                p.sr.color = c;
            }
            yield return null;
        }
    }

    public void MostrarTextoFlutuante(string texto, Vector3 pos, Color cor)
    {
        GameObject go = new GameObject("TextoFlutuante");
        go.transform.position = pos;
        TextMeshPro tmp = go.AddComponent<TextMeshPro>();
        if (fontePopup != null) tmp.font = fontePopup;
        tmp.text = texto;
        tmp.fontSize = 10f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = cor;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.rectTransform.sizeDelta = new Vector2(8f, 2f);
        tmp.sortingOrder = ordemDeDesenho + 10;
        StartCoroutine(AnimarTextoFlutuante(tmp));
    }

    private IEnumerator AnimarTextoFlutuante(TextMeshPro tmp)
    {
        float tempo = 0f;
        const float duracao = 1.1f;
        Vector3 inicio = tmp.transform.position;
        Color cor = tmp.color;
        while (tempo < duracao && tmp != null)
        {
            tempo += Time.deltaTime;
            float k = tempo / duracao;
            tmp.transform.position = inicio + new Vector3(0f, 1.2f * k, 0f);
            cor.a = k < 0.6f ? 1f : 1f - ((k - 0.6f) / 0.4f);
            tmp.color = cor;
            yield return null;
        }
        if (tmp != null) Destroy(tmp.gameObject);
    }

    private void MostrarGolpe()
    {
        if (golpeRenderer == null) return;
        StartCoroutine(AnimarGolpe());
    }

    private IEnumerator AnimarGolpe()
    {
        golpeRenderer.enabled = true;
        Transform t = golpeRenderer.transform;
        yield return StartCoroutine(Interpolar(0.2f, k =>
        {
            t.localScale = Vector3.one * Mathf.Lerp(1.6f, 2.6f, k);
            golpeRenderer.color = new Color(1f, 1f, 1f, 1f - k);
        }));
        golpeRenderer.enabled = false;
    }

    private IEnumerator Interpolar(float duracao, Action<float> passo)
    {
        float t = 0f;
        while (t < duracao)
        {
            t += Time.deltaTime;
            passo(Mathf.Clamp01(t / duracao));
            yield return null;
        }
        passo(1f);
    }

    // ================= montagem do corpo =================

    private static Material CriarMaterial()
    {
        Shader s = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (s == null) s = Shader.Find("Sprites/Default");
        return new Material(s);
    }

    private static Color32[] LerPixels(Sprite s, out int w, out int h)
    {
        Rect r = s.rect;
        w = (int)r.width;
        h = (int)r.height;
        Texture2D fonte = s.texture;

        RenderTexture rt = RenderTexture.GetTemporary(fonte.width, fonte.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(fonte, rt);
        RenderTexture anterior = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D tmp = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tmp.ReadPixels(new Rect(r.x, r.y, w, h), 0, 0);
        tmp.Apply();

        RenderTexture.active = anterior;
        RenderTexture.ReleaseTemporary(rt);

        Color32[] px = tmp.GetPixels32();
        Destroy(tmp);
        return px;
    }

    private void PrepararPixels()
    {
        int w;
        int h;
        pixelsOriginais = LerPixels(corpoCompleto, out w, out h);
        larg = w;
        alt = h;

        pixelsPecas.Clear();
        for (int i = 0; i < regioes.Count; i++)
        {
            RegiaoCorpo r = regioes[i];
            if (r.pecaSolta == null || !r.destacavel) continue;
            int pw;
            int ph;
            pixelsPecas[r.tipo] = LerPixels(r.pecaSolta, out pw, out ph);
            r.posicaoPeca = new RectInt(r.posicaoPeca.x, r.posicaoPeca.y, pw, ph);
        }

        if (troncoSemCabeca != null)
            pixelsTronco = LerPixels(troncoSemCabeca, out troncoLarg, out troncoAlt);
    }

    private SpriteRenderer NovoRenderer(string nome, Transform pai, int ordem)
    {
        GameObject go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sharedMaterial = materialSprite;
        sr.sortingOrder = ordem;
        return sr;
    }

    private void ConstruirHierarquia()
    {
        visual = new GameObject("Visual").transform;
        visual.SetParent(transform, false);

        corpoRenderer = NovoRenderer("Corpo", visual, ordemDeDesenho);

        pivoBraco = new GameObject("PivoBraco").transform;
        pivoBraco.SetParent(visual, false);
        pivoBraco.localPosition = new Vector3((ombroBracoArmado.x - larg * 0.5f) / pixelsPorUnidade, ombroBracoArmado.y / (float)pixelsPorUnidade, 0f);
        bracoRenderer = NovoRenderer("BracoAtaque", pivoBraco, ordemDeDesenho + 1);
        bracoRenderer.enabled = false;

        golpeRenderer = NovoRenderer("Golpe", visual, ordemDeDesenho + 2);
        golpeRenderer.transform.localPosition = new Vector3(0.02f, 0.72f, 0f);
        golpeRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, -25f);
        golpeRenderer.enabled = false;

        realceRenderer = NovoRenderer("Realce", visual, ordemDeDesenho + 3);
        realceRenderer.enabled = false;
    }

    private void CriarSpritesAuxiliares()
    {
        // quadrado 1x1 (particulas)
        Texture2D tq = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tq.filterMode = FilterMode.Point;
        tq.SetPixel(0, 0, Color.white);
        tq.Apply();
        spriteQuadrado = Sprite.Create(tq, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        descartaveisFixos.Add(tq);
        descartaveisFixos.Add(spriteQuadrado);

        // braco armado separado (para a animacao do ataque)
        RegiaoCorpo ra = Obter(TipoRegiao.BracoArmado);
        Color32[] pxBraco;
        if (ra != null && pixelsPecas.TryGetValue(TipoRegiao.BracoArmado, out pxBraco))
        {
            Vector2 pivo = new Vector2(
                (ombroBracoArmado.x - ra.posicaoPeca.x) / (float)ra.posicaoPeca.width,
                (ombroBracoArmado.y - ra.posicaoPeca.y) / (float)ra.posicaoPeca.height);
            bracoRenderer.sprite = CriarSprite(pxBraco, ra.posicaoPeca.width, ra.posicaoPeca.height, pivo, descartaveisFixos);
        }

        // arco do golpe da faca
        int s = 48;
        Color32[] arco = new Color32[s * s];
        Vector2 c = new Vector2(s * 0.5f, s * 0.5f);
        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                Vector2 d = new Vector2(x + 0.5f, y + 0.5f) - c;
                float rad = d.magnitude;
                float a = Mathf.Abs(Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
                if (a < 125f) continue;
                float espessura = 6f * (1f - (180f - a) / 55f);
                if (rad <= 22f && rad >= 22f - espessura)
                    arco[y * s + x] = new Color32(235, 225, 255, 255);
            }
        }
        golpeRenderer.sprite = CriarSprite(arco, s, s, new Vector2(0.5f, 0.5f), descartaveisFixos);
    }

    private Sprite CriarSprite(Color32[] px, int w, int h, Vector2 pivo, List<UnityEngine.Object> lista)
    {
        Texture2D t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.filterMode = FilterMode.Point;
        t.wrapMode = TextureWrapMode.Clamp;
        t.SetPixels32(px);
        t.Apply();
        Sprite s = Sprite.Create(t, new Rect(0, 0, w, h), pivo, pixelsPorUnidade, 0, SpriteMeshType.FullRect);
        if (lista != null)
        {
            lista.Add(t);
            lista.Add(s);
        }
        return s;
    }

    private Sprite CriarMoldura(int w, int h)
    {
        Color32[] px = new Color32[w * h];
        const int espessura = 2;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool borda = x < espessura || y < espessura || x >= w - espessura || y >= h - espessura;
                if (borda) px[y * w + x] = new Color32(255, 255, 255, 255);
            }
        }
        return CriarSprite(px, w, h, new Vector2(0.5f, 0.5f), null);
    }

    private void ReconstruirCorpo()
    {
        for (int i = 0; i < descartaveisCorpo.Count; i++)
        {
            if (descartaveisCorpo[i] != null) Destroy(descartaveisCorpo[i]);
        }
        descartaveisCorpo.Clear();

        Vector2 pivo = new Vector2(0.5f, 0f);
        Color32[] px = Compor(false);
        spriteCorpo = CriarSprite(px, larg, alt, pivo, descartaveisCorpo);
        spriteCorpoBranco = CriarSprite(Branco(px), larg, alt, pivo, descartaveisCorpo);
        spriteSemBraco = CriarSprite(Compor(true), larg, alt, pivo, descartaveisCorpo);

        if (corpoRenderer != null) corpoRenderer.sprite = spriteCorpo;
    }

    private Color32[] Compor(bool forcarSemBracoArmado)
    {
        Color32[] px = (Color32[])pixelsOriginais.Clone();
        for (int i = 0; i < regioes.Count; i++)
        {
            RegiaoCorpo r = regioes[i];
            bool apagar = r.perdida && r.destacavel;
            if (forcarSemBracoArmado && r.tipo == TipoRegiao.BracoArmado) apagar = true;
            if (apagar) ApagarPeca(px, r);
        }
        if (EstaPerdida(TipoRegiao.Cabeca) && Obter(TipoRegiao.Cabeca).destacavel && pixelsTronco != null)
            RestaurarPescoco(px);
        return px;
    }

    private void ApagarPeca(Color32[] px, RegiaoCorpo r)
    {
        Color32[] peca;
        if (!pixelsPecas.TryGetValue(r.tipo, out peca)) return;

        int pw = r.posicaoPeca.width;
        int ph = r.posicaoPeca.height;
        int e = r.expandirRecorte;
        for (int y = 0; y < ph; y++)
        {
            for (int x = 0; x < pw; x++)
            {
                if (peca[y * pw + x].a < 40) continue;
                for (int dy = -e; dy <= e; dy++)
                {
                    for (int dx = -e; dx <= e; dx++)
                    {
                        int bx = r.posicaoPeca.x + x + dx;
                        int by = r.posicaoPeca.y + y + dy;
                        if (bx < 0 || by < 0 || bx >= larg || by >= alt) continue;
                        px[by * larg + bx] = new Color32(0, 0, 0, 0);
                    }
                }
            }
        }
    }

    private void RestaurarPescoco(Color32[] px)
    {
        int ox = posicaoTroncoSemCabeca.x;
        int oy = posicaoTroncoSemCabeca.y;
        for (int y = Mathf.Max(0, troncoAlt - 14); y < troncoAlt; y++)
        {
            for (int x = 0; x < troncoLarg; x++)
            {
                Color32 t = pixelsTronco[y * troncoLarg + x];
                if (t.a < 128) continue;
                int bx = ox + x;
                int by = oy + y;
                if (bx < 0 || by < 0 || bx >= larg || by >= alt) continue;
                if (px[by * larg + bx].a < 10) px[by * larg + bx] = t;
            }
        }
    }

    private static Color32[] Branco(Color32[] origem)
    {
        Color32[] px = new Color32[origem.Length];
        for (int i = 0; i < origem.Length; i++)
        {
            if (origem[i].a > 0) px[i] = new Color32(255, 255, 255, origem[i].a);
        }
        return px;
    }
}
