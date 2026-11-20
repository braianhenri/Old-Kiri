using UnityEngine;
using UnityEngine.EventSystems;

public class BotaoHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuração da animação")]
    public float escalaAumentada = 1.1f; // 1.1 = 10% maior
    public float velocidade = 10f;       // quão rápido a transição acontece

    private Vector3 escalaOriginal;
    private Vector3 escalaAlvo;

    void Awake()
    {
        escalaOriginal = transform.localScale;
        escalaAlvo = escalaOriginal;
    }

    void Update()
    {
        // Suaviza a transição entre o tamanho normal e o aumentado
        transform.localScale = Vector3.Lerp(transform.localScale, escalaAlvo, Time.unscaledDeltaTime * velocidade);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        escalaAlvo = escalaOriginal * escalaAumentada;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        escalaAlvo = escalaOriginal;
    }
}