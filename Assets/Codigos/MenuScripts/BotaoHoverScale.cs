using UnityEngine;
using UnityEngine.EventSystems;

public class BotaoHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuração da animação")]
    public float escalaAumentada = 1.1f; // 1.1 = 10% maior
    public float velocidade = 10f;       // quão rápido a transição acontece
    private const float THRESHOLD = 0.001f; // Limite para considerar como "chegou no alvo"

    private Vector3 escalaOriginal;
    private Vector3 escalaAlvo;
    private bool animando = false;

    void Awake()
    {
        escalaOriginal = transform.localScale;
        escalaAlvo = escalaOriginal;
    }

    void Update()
    {
        if (!animando) return;

        // Suaviza a transição entre o tamanho normal e o aumentado
        transform.localScale = Vector3.Lerp(transform.localScale, escalaAlvo, Time.unscaledDeltaTime * velocidade);

        // Para a animação quando atingir o alvo
        if (Vector3.Distance(transform.localScale, escalaAlvo) < THRESHOLD)
        {
            transform.localScale = escalaAlvo;
            animando = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        escalaAlvo = escalaOriginal * escalaAumentada;
        animando = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        escalaAlvo = escalaOriginal;
        animando = true;
    }
}