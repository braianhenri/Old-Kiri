using UnityEngine;

public class SetaGuia : MonoBehaviour
{
    public Transform alvo;
    public Transform jogador;
    public float distanciaDoJogador = 1.2f;
    public float distanciaParaSumir = 2f;
    public float anguloExtra = 0f;

    public float velocidadePulso = 2f;
    public float intensidadePulso = 0.15f;

    public float velocidadeMovimento = 3f;
    public float intensidadeMovimento = 0.15f;

    SpriteRenderer sr;
    Vector3 escalaOriginal;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        escalaOriginal = transform.localScale;
    }

    void LateUpdate()
    {
        if (alvo == null || jogador == null) return;

        Vector2 dir = alvo.position - jogador.position;
        float dist = dir.magnitude;

        if (sr != null) sr.enabled = dist > distanciaParaSumir;
        if (dist <= distanciaParaSumir) return;

        dir.Normalize();

        Vector2 pos = (Vector2)jogador.position + dir * distanciaDoJogador;

        // Movimento para cima e para baixo
        float movimentoY = Mathf.Sin(Time.time * velocidadeMovimento) * intensidadeMovimento;

        transform.position = new Vector3(
            pos.x,
            pos.y + movimentoY,
            transform.position.z
        );

        float angulo = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angulo + anguloExtra);

        // Pulsação
        float pulso = 1f + Mathf.Sin(Time.time * velocidadePulso) * intensidadePulso;
        transform.localScale = escalaOriginal * pulso;
    }
}