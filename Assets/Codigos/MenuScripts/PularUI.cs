using UnityEngine;

public class PulsarUI : MonoBehaviour
{
    public float velocidade = 3f;     // maior = pulsa mais rápido
    public float intensidade = 0.1f;  // 0.1 = cresce e diminui 10%

    Vector3 escalaOriginal;

    void Awake()
    {
        escalaOriginal = transform.localScale;
    }

    void OnEnable()
    {
        transform.localScale = escalaOriginal;
    }

    void Update()
    {
        float pulso = 1f + Mathf.Sin(Time.unscaledTime * velocidade) * intensidade;
        transform.localScale = escalaOriginal * pulso;
    }
}