using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Ajudas compartilhadas pelas cenas de historia (Marta, Senhorio, intro da cena02).
public static class CutsceneUtil
{
    public static Transform AcharXomi()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        return p != null ? p.transform : null;
    }

    // Liga/desliga os scripts de movimento do Xomi (os mesmos que os dialogos ja travam)
    public static void TravarXomi(bool travar)
    {
        Transform x = AcharXomi();
        if (x == null) return;

        foreach (MonoBehaviour mb in x.GetComponents<MonoBehaviour>())
            if (mb != null && mb.GetType().Name == "NewBehaviourScript") mb.enabled = !travar;

        if (!travar) return;
        Rigidbody2D rb = x.GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;
        Animator an = x.GetComponent<Animator>();
        if (an != null) { an.SetBool("Andando", false); an.SetBool("Lado", false); }
    }

    public static void XomiOlhar(bool paraCima)
    {
        Transform x = AcharXomi();
        if (x == null) return;
        Animator an = x.GetComponent<Animator>();
        if (an == null) return;
        an.SetBool("Cima", paraCima);
        an.SetBool("Baixo", !paraCima);
        an.SetBool("Lado", false);
    }
}

// Tela preta criada por codigo (nao precisa montar nada na cena). Use Criar(alpha) e depois Animar(destino, tempo).
public class TelaFade
{
    public CanvasGroup grupo;

    public static TelaFade Criar(float alphaInicial)
    {
        GameObject go = new GameObject("TelaFade (automatica)");
        Canvas cv = go.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 5000;

        GameObject preto = new GameObject("Preto");
        preto.transform.SetParent(go.transform, false);
        Image img = preto.AddComponent<Image>();
        img.color = Color.black;
        img.raycastTarget = false;
        RectTransform rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        TelaFade f = new TelaFade();
        f.grupo = go.AddComponent<CanvasGroup>();
        f.grupo.alpha = alphaInicial;
        f.grupo.blocksRaycasts = false;
        return f;
    }

    public IEnumerator Animar(float para, float tempo)
    {
        float de = grupo.alpha;
        float t = 0f;
        while (t < tempo)
        {
            t += Time.unscaledDeltaTime;
            grupo.alpha = Mathf.Lerp(de, para, t / tempo);
            yield return null;
        }
        grupo.alpha = para;
    }
}
