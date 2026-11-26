using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Objeto do mapa que o jogador pode ler (ex: um jornal no chao).
/// Perto dele aparece o icone "E". Ao apertar E, a imagem do objeto cresce no meio da tela
/// para dar para ler. Aperte E, Enter, Espaco ou clique para fechar (Esc fica livre para o menu de pausa).
/// A interface de leitura e criada por codigo: nao precisa montar nada na cena.
/// </summary>
public class ObjetoLegivel : MonoBehaviour
{
    [Header("Interacao")]
    [Tooltip("Distancia (em unidades do mapa) em que o jogador consegue ler")]
    public float distanciaInteracao = 2.2f;
    public KeyCode teclaInteracao = KeyCode.E;
    [Tooltip("Icone \"E\" que aparece quando o jogador esta perto (pode ficar vazio)")]
    public GameObject iconeInteracao;

    [Header("Leitura")]
    [Tooltip("Imagem que aparece ampliada. Se ficar vazio, usa o sprite deste objeto")]
    public Sprite imagemAmpliada;
    [Tooltip("Quanto da tela a imagem ocupa (0.9 = 90%)")]
    [Range(0.4f, 1f)] public float tamanhoNaTela = 0.9f;
    public string textoDica = "E para fechar";
    [Tooltip("Trava o movimento do jogador enquanto ele le")]
    public bool travarJogadorLendo = true;

    private bool lendo;
    private bool podeFechar;
    private GameObject painel;
    private RectTransform areaImagem;
    private CanvasGroup grupo;

    private void Update()
    {
        if (lendo)
        {
            if (podeFechar && QuerFechar()) Fechar();
            return;
        }

        Transform xomi = CutsceneUtil.AcharXomi();
        bool perto = xomi != null
            && MovimentoLiberado(xomi)
            && Vector2.Distance(xomi.position, transform.position) <= distanciaInteracao;

        if (iconeInteracao != null && iconeInteracao.activeSelf != perto)
            iconeInteracao.SetActive(perto);

        if (perto && Input.GetKeyDown(teclaInteracao)) Abrir();
    }

    private void OnDisable()
    {
        // se o objeto for desligado com a leitura aberta, nao deixa o jogador preso
        if (lendo) Encerrar();
    }

    private static bool QuerFechar()
    {
        return Input.GetKeyDown(KeyCode.E)
            || Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.KeypadEnter)
            || Input.GetKeyDown(KeyCode.Space)
            || Input.GetMouseButtonDown(0);
    }

    // Jogador "livre" = o script de movimento dele esta ligado (nao esta em dialogo nem cutscene)
    private static bool MovimentoLiberado(Transform xomi)
    {
        bool achou = false;
        foreach (MonoBehaviour mb in xomi.GetComponents<MonoBehaviour>())
        {
            if (mb == null || mb.GetType().Name != "NewBehaviourScript") continue;
            achou = true;
            if (mb.enabled) return true;
        }
        return !achou;
    }

    public void Abrir()
    {
        if (lendo) return;

        Sprite sprite = imagemAmpliada;
        if (sprite == null)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) sprite = sr.sprite;
        }
        if (sprite == null)
        {
            Debug.LogWarning("ObjetoLegivel: sem imagem para mostrar em " + name, this);
            return;
        }

        lendo = true;
        podeFechar = false;
        if (iconeInteracao != null) iconeInteracao.SetActive(false);
        if (travarJogadorLendo) CutsceneUtil.TravarXomi(true);

        CriarPainel(sprite);
        StartCoroutine(Animar(true));
    }

    public void Fechar()
    {
        if (!lendo || !podeFechar) return;
        podeFechar = false;
        StartCoroutine(Animar(false));
    }

    private void Encerrar()
    {
        lendo = false;
        podeFechar = false;
        if (painel != null) Destroy(painel);
        painel = null;
        if (travarJogadorLendo) CutsceneUtil.TravarXomi(false);
    }

    private IEnumerator Animar(bool abrindo)
    {
        float duracao = abrindo ? 0.28f : 0.18f;
        float t = 0f;
        while (t < duracao)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duracao);

            if (abrindo)
            {
                // cresce com um pulinho no fim
                float escala = k < 0.75f
                    ? Mathf.Lerp(0.55f, 1.04f, k / 0.75f)
                    : Mathf.Lerp(1.04f, 1f, (k - 0.75f) / 0.25f);
                areaImagem.localScale = Vector3.one * escala;
                grupo.alpha = Mathf.Clamp01(k * 1.6f);
            }
            else
            {
                areaImagem.localScale = Vector3.one * Mathf.Lerp(1f, 0.75f, k);
                grupo.alpha = 1f - k;
            }
            yield return null;
        }

        if (abrindo)
        {
            areaImagem.localScale = Vector3.one;
            grupo.alpha = 1f;
            yield return new WaitForSecondsRealtime(0.12f); // evita fechar sem querer logo ao abrir
            podeFechar = true;
        }
        else
        {
            Encerrar();
        }
    }

    // ---------- interface criada por codigo ----------

    private void CriarPainel(Sprite sprite)
    {
        painel = new GameObject("Leitura (" + name + ")", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));

        Canvas canvas = painel.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 4000;

        CanvasScaler escalador = painel.GetComponent<CanvasScaler>();
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(1920f, 1080f);
        escalador.matchWidthOrHeight = 0.5f;

        grupo = painel.GetComponent<CanvasGroup>();
        grupo.alpha = 0f;

        // fundo escuro (tambem segura cliques para nao apertar nada atras)
        Image fundo = NovaImagem("Fundo", painel.transform, new Color(0.03f, 0.02f, 0.07f, 0.85f));
        fundo.raycastTarget = true;
        Esticar(fundo.rectTransform, Vector2.zero, Vector2.one);

        // area da imagem: ocupa "tamanhoNaTela" da tela, mantendo a proporcao do desenho
        GameObject area = new GameObject("Imagem", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        area.transform.SetParent(painel.transform, false);
        areaImagem = area.GetComponent<RectTransform>();
        float margem = (1f - tamanhoNaTela) * 0.5f;
        Esticar(areaImagem, new Vector2(margem, margem), new Vector2(1f - margem, 1f - margem));

        Image img = area.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;

        // dica para fechar
        GameObject goTexto = new GameObject("Dica", typeof(RectTransform));
        goTexto.transform.SetParent(painel.transform, false);
        TextMeshProUGUI dica = goTexto.AddComponent<TextMeshProUGUI>();
        dica.text = textoDica;
        dica.fontSize = 34f;
        dica.alignment = TextAlignmentOptions.Center;
        dica.color = new Color(0.82f, 0.62f, 1f, 1f);
        dica.raycastTarget = false;
        RectTransform rt = dica.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(700f, 60f);
        rt.anchoredPosition = new Vector2(0f, 14f);
    }

    private static Image NovaImagem(string nome, Transform pai, Color cor)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(pai, false);
        Image img = go.GetComponent<Image>();
        img.color = cor;
        return img;
    }

    private static void Esticar(RectTransform r, Vector2 anchorMin, Vector2 anchorMax)
    {
        r.anchorMin = anchorMin;
        r.anchorMax = anchorMax;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }
}
