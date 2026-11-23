using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Aviso na tela quando o Xomi ganha um item: desenho do item e a quantidade ao lado (ex: [lata] 1x).
/// Monta a interface por codigo (nao precisa criar nada na cena) e usa tempo real, entao funciona
/// mesmo com o jogo pausado. Uso: PopupItemRecebido.Mostrar(sprite, 1, canvas, fonte);
/// </summary>
public class PopupItemRecebido : MonoBehaviour
{
    private static PopupItemRecebido atual;

    private CanvasGroup grupo;
    private RectTransform rect;

    public static void Mostrar(Sprite sprite, int quantidade, Canvas canvas, TMP_FontAsset fonte)
    {
        if (sprite == null || canvas == null) return;

        // so um aviso por vez
        if (atual != null) Destroy(atual.gameObject);

        GameObject go = new GameObject("PopupItemRecebido", typeof(RectTransform), typeof(CanvasGroup));
        go.transform.SetParent(canvas.rootCanvas.transform, false);
        go.transform.SetAsLastSibling(); // por cima do resto da interface

        PopupItemRecebido popup = go.AddComponent<PopupItemRecebido>();
        atual = popup;
        popup.Montar(sprite, quantidade, fonte);
        popup.StartCoroutine(popup.Animar());
    }

    private void OnDestroy()
    {
        if (atual == this) atual = null;
    }

    private void Montar(Sprite sprite, int quantidade, TMP_FontAsset fonte)
    {
        grupo = GetComponent<CanvasGroup>();
        grupo.interactable = false;
        grupo.blocksRaycasts = false;
        grupo.alpha = 0f;

        // pixel art nitida (muda so na memoria durante o jogo, nao altera o arquivo da imagem)
        if (sprite.texture != null) sprite.texture.filterMode = FilterMode.Point;

        rect = GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(330f, 150f);
        rect.anchoredPosition = new Vector2(0f, -50f);

        // borda + fundo no estilo roxo escuro do jogo
        Image borda = NovaImagem("Borda", new Color(0.82f, 0.55f, 1f, 1f));
        Esticar(borda.rectTransform, 0f);
        Image fundo = NovaImagem("Fundo", new Color(0.07f, 0.04f, 0.13f, 0.94f));
        Esticar(fundo.rectTransform, 5f);

        // desenho do item
        Image icone = NovaImagem("Icone", Color.white);
        icone.sprite = sprite;
        icone.preserveAspect = true;
        RectTransform ri = icone.rectTransform;
        ri.anchorMin = new Vector2(0f, 0.5f);
        ri.anchorMax = new Vector2(0f, 0.5f);
        ri.pivot = new Vector2(0.5f, 0.5f);
        ri.sizeDelta = new Vector2(100f, 110f);
        ri.anchoredPosition = new Vector2(80f, 0f);

        // quantidade ao lado (ex: 1x)
        GameObject goTexto = new GameObject("Quantidade", typeof(RectTransform));
        goTexto.transform.SetParent(transform, false);
        TextMeshProUGUI texto = goTexto.AddComponent<TextMeshProUGUI>();
        if (fonte != null) texto.font = fonte;
        texto.text = quantidade + "x";
        texto.fontSize = 72f;
        texto.fontStyle = FontStyles.Bold;
        texto.alignment = TextAlignmentOptions.MidlineLeft;
        texto.color = Color.white;
        texto.raycastTarget = false;
        texto.textWrappingMode = TextWrappingModes.NoWrap;
        RectTransform rt = texto.rectTransform;
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(170f, 110f);
        rt.anchoredPosition = new Vector2(150f, 0f);
    }

    private Image NovaImagem(string nome, Color cor)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(transform, false);
        Image img = go.GetComponent<Image>();
        img.color = cor;
        img.raycastTarget = false;
        return img;
    }

    private static void Esticar(RectTransform r, float margem)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = new Vector2(margem, margem);
        r.offsetMax = new Vector2(-margem, -margem);
    }

    private IEnumerator Animar()
    {
        // entra: cresce com um pulinho
        float t = 0f;
        const float entrada = 0.3f;
        while (t < entrada)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / entrada);
            float escala = k < 0.7f
                ? Mathf.Lerp(0.5f, 1.15f, k / 0.7f)
                : Mathf.Lerp(1.15f, 1f, (k - 0.7f) / 0.3f);
            rect.localScale = Vector3.one * escala;
            grupo.alpha = Mathf.Clamp01(k * 2f);
            yield return null;
        }
        rect.localScale = Vector3.one;
        grupo.alpha = 1f;

        // fica um tempo na tela
        yield return new WaitForSecondsRealtime(2.2f);

        // sai: sobe um pouco e some
        t = 0f;
        const float saida = 0.45f;
        Vector2 inicio = rect.anchoredPosition;
        while (t < saida)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / saida);
            rect.anchoredPosition = inicio + new Vector2(0f, 30f * k);
            grupo.alpha = 1f - k;
            yield return null;
        }

        Destroy(gameObject);
    }
}
