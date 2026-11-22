using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ResultadoTiming { Erro, Fraco, Bom, Perfeito }

/// <summary>
/// Barra de timing do ataque do jogador. Um marcador vai e volta pela barra; o jogador aperta
/// Enter/Espaco/Z (ou clica) para para-lo. Quanto mais perto do centro da zona, melhor o golpe.
/// A interface e criada em tempo de execucao dentro do canvas da batalha (nada para montar na cena).
/// Uso: yield return timing.Executar(precisaoDaRegiao, bonusDeLargura); depois ler timing.Resultado.
/// </summary>
public class TimingAtaque : MonoBehaviour
{
    [Header("Aparencia")]
    [SerializeField] private Vector2 tamanhoBarra = new Vector2(720f, 54f);
    [SerializeField] private Vector2 posicaoBarra = new Vector2(0f, -150f);

    [Header("Dificuldade")]
    [Tooltip("Largura da zona boa (0 a 1 da barra) quando a precisao da regiao e 0.")]
    [SerializeField] private float larguraMinima = 0.06f;
    [Tooltip("Largura extra da zona boa conforme a precisao da regiao (0 a 1).")]
    [SerializeField] private float larguraPorPrecisao = 0.16f;
    [Tooltip("Barras por segundo do marcador.")]
    [SerializeField] private float velocidadeBase = 1.0f;
    [Tooltip("Velocidade extra para regioes dificeis (precisao baixa).")]
    [SerializeField] private float velocidadeExtraPorDificuldade = 0.8f;
    [Tooltip("Quantas barras o marcador percorre antes de contar como erro (demorou demais).")]
    [SerializeField] private float maxVarreduras = 3f;
    [Tooltip("Ignora entrada logo no inicio para nao aproveitar o Enter que confirmou o ataque.")]
    [SerializeField] private float tempoSemEntrada = 0.2f;

    public ResultadoTiming Resultado { get; private set; }

    private TMP_Text fonteReferencia;
    private RectTransform painel;
    private RectTransform zonaFraca;
    private RectTransform zonaBoa;
    private RectTransform zonaPerfeita;
    private RectTransform marcador;
    private Image imagemMarcador;
    private TMP_Text rotulo;

    public void Configurar(TMP_Text referencia)
    {
        fonteReferencia = referencia;
    }

    public void Esconder()
    {
        if (painel != null) painel.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        Esconder();
    }

    public IEnumerator Executar(float precisaoRegiao, float bonusLargura)
    {
        Construir();
        if (painel == null)
        {
            // sem canvas: nao trava a batalha, conta como golpe normal
            Resultado = ResultadoTiming.Bom;
            yield break;
        }

        float precisao = Mathf.Clamp01(precisaoRegiao);
        float dificuldade = 1f - precisao;
        float largura = Mathf.Clamp(larguraMinima + larguraPorPrecisao * precisao + bonusLargura, 0.06f, 0.6f);
        float larguraPerfeita = largura * 0.3f;
        float larguraFraca = Mathf.Min(1f, largura * 1.7f);
        float centro = Random.Range(0.28f, 0.72f);

        DefinirZona(zonaFraca, centro, larguraFraca);
        DefinirZona(zonaBoa, centro, largura);
        DefinirZona(zonaPerfeita, centro, larguraPerfeita);

        imagemMarcador.color = Color.white;
        rotulo.text = "ATAQUE!  Aperte Enter, Espaço ou clique no centro da zona";
        painel.gameObject.SetActive(true);

        float velocidade = velocidadeBase + dificuldade * velocidadeExtraPorDificuldade;
        float pos = 0f;
        float direcao = 1f;
        float percorrido = 0f;
        float tempo = 0f;
        bool parou = false;

        DefinirMarcador(pos);
        while (percorrido < maxVarreduras)
        {
            float passo = velocidade * Time.deltaTime;
            pos += direcao * passo;
            percorrido += passo;
            if (pos >= 1f)
            {
                pos = 1f - (pos - 1f);
                direcao = -1f;
            }
            else if (pos <= 0f)
            {
                pos = -pos;
                direcao = 1f;
            }
            pos = Mathf.Clamp01(pos);
            DefinirMarcador(pos);

            tempo += Time.deltaTime;
            if (tempo > tempoSemEntrada && EntradaConfirmar())
            {
                parou = true;
                break;
            }
            yield return null;
        }

        string texto;
        Color cor;
        if (!parou)
        {
            Resultado = ResultadoTiming.Erro;
            texto = "Demorou demais!";
            cor = new Color(0.9f, 0.25f, 0.3f);
        }
        else
        {
            float distancia = Mathf.Abs(pos - centro);
            if (distancia <= larguraPerfeita * 0.5f)
            {
                Resultado = ResultadoTiming.Perfeito;
                texto = "PERFEITO!";
                cor = new Color(1f, 0.95f, 0.4f);
            }
            else if (distancia <= largura * 0.5f)
            {
                Resultado = ResultadoTiming.Bom;
                texto = "Bom!";
                cor = new Color(0.4f, 0.9f, 0.5f);
            }
            else if (distancia <= larguraFraca * 0.5f)
            {
                Resultado = ResultadoTiming.Fraco;
                texto = "Fraco...";
                cor = new Color(0.95f, 0.7f, 0.25f);
            }
            else
            {
                Resultado = ResultadoTiming.Erro;
                texto = "Errou o tempo!";
                cor = new Color(0.9f, 0.25f, 0.3f);
            }
        }

        imagemMarcador.color = cor;
        rotulo.text = texto;
        yield return new WaitForSeconds(0.5f);
        painel.gameObject.SetActive(false);
    }

    private static bool EntradaConfirmar()
    {
        return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)
            || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z)
            || Input.GetMouseButtonDown(0);
    }

    // ================= interface criada por codigo =================

    private void Construir()
    {
        if (painel != null) return;

        Canvas canvas = null;
        if (fonteReferencia != null && fonteReferencia.canvas != null) canvas = fonteReferencia.canvas.rootCanvas;
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("TimingAtaque: nenhum Canvas encontrado, o timing sera ignorado.", this);
            return;
        }

        GameObject raiz = new GameObject("TimingAtaque_UI", typeof(RectTransform));
        painel = raiz.GetComponent<RectTransform>();
        painel.SetParent(canvas.transform, false);
        painel.anchorMin = painel.anchorMax = new Vector2(0.5f, 0.5f);
        painel.pivot = new Vector2(0.5f, 0.5f);
        painel.sizeDelta = tamanhoBarra;
        painel.anchoredPosition = posicaoBarra;

        Image fundo = raiz.AddComponent<Image>();
        fundo.color = new Color(0.05f, 0.05f, 0.09f, 0.88f);
        fundo.raycastTarget = false;

        zonaFraca = CriarZona("ZonaFraca", new Color(0.85f, 0.62f, 0.2f, 0.55f));
        zonaBoa = CriarZona("ZonaBoa", new Color(0.3f, 0.8f, 0.4f, 0.8f));
        zonaPerfeita = CriarZona("ZonaPerfeita", new Color(1f, 0.95f, 0.45f, 0.95f));

        GameObject m = new GameObject("Marcador", typeof(RectTransform));
        marcador = m.GetComponent<RectTransform>();
        marcador.SetParent(painel, false);
        marcador.pivot = new Vector2(0.5f, 0.5f);
        marcador.sizeDelta = new Vector2(8f, 10f);
        imagemMarcador = m.AddComponent<Image>();
        imagemMarcador.color = Color.white;
        imagemMarcador.raycastTarget = false;

        GameObject t = new GameObject("Rotulo", typeof(RectTransform));
        RectTransform rt = t.GetComponent<RectTransform>();
        rt.SetParent(painel, false);
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(0f, 40f);
        rt.anchoredPosition = new Vector2(0f, 6f);
        TextMeshProUGUI tmp = t.AddComponent<TextMeshProUGUI>();
        if (fonteReferencia != null) tmp.font = fonteReferencia.font;
        tmp.fontSize = 26f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        rotulo = tmp;

        raiz.SetActive(false);
    }

    private RectTransform CriarZona(string nome, Color cor)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(painel, false);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        Image img = go.AddComponent<Image>();
        img.color = cor;
        img.raycastTarget = false;
        return rt;
    }

    private static void DefinirZona(RectTransform zona, float centro, float largura)
    {
        float minX = Mathf.Clamp01(centro - largura * 0.5f);
        float maxX = Mathf.Clamp01(centro + largura * 0.5f);
        zona.anchorMin = new Vector2(minX, 0.12f);
        zona.anchorMax = new Vector2(maxX, 0.88f);
        zona.offsetMin = zona.offsetMax = Vector2.zero;
    }

    private void DefinirMarcador(float p)
    {
        marcador.anchorMin = new Vector2(p, 0f);
        marcador.anchorMax = new Vector2(p, 1f);
        marcador.sizeDelta = new Vector2(8f, 10f);
        marcador.anchoredPosition = Vector2.zero;
    }
}
