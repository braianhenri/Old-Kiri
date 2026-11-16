using UnityEngine;

public class SetaGuia : MonoBehaviour
{
    [Header("Alvos")]
    public Transform alvo;
    public Transform jogador;

    [Header("Posicao")]
    public float distanciaDoJogador = 2.5f;
    public float distanciaParaSumir = 3.5f;
    public float anguloExtra = 0f;

    [Header("Animacao")]
    public float velocidadePulso = 3f;
    public float intensidadePulso = 0.1f;
    public float velocidadeMovimento = 4f;
    public float intensidadeMovimento = 0.2f;

    [Header("Suavizacao")]
    public float suavidadeRotacao = 12f;
    public float velocidadeSumir = 6f;

    SpriteRenderer sr;
    Vector3 escalaOriginal;
    float anguloAtual;
    float alfa = 1f;
    bool precisaAlinhar = true;

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
        if (dist < 0.01f) return;
        dir /= dist;

        // aparece e some suavemente (nao "pisca")
        float alfaDesejado = dist > distanciaParaSumir ? 1f : 0f;
        alfa = Mathf.MoveTowards(alfa, alfaDesejado, velocidadeSumir * Time.deltaTime);

        if (sr != null)
        {
            Color c = sr.color;
            c.a = alfa;
            sr.color = c;
            sr.enabled = alfa > 0.01f;
        }

        if (alfa <= 0.01f)
        {
            precisaAlinhar = true;
            return;
        }

        // a seta nunca passa do destino
        float distBase = Mathf.Max(0f, Mathf.Min(distanciaDoJogador, dist - 0.5f));

        // vai e volta na direcao que a seta aponta
        float balanco = Mathf.Sin(Time.time * velocidadeMovimento) * intensidadeMovimento;

        Vector2 pos = (Vector2)jogador.position + dir * (distBase + balanco);
        transform.position = new Vector3(pos.x, pos.y, transform.position.z);

        // rotacao suave
        float anguloDesejado = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + anguloExtra;
        if (precisaAlinhar)
        {
            anguloAtual = anguloDesejado;
            precisaAlinhar = false;
        }
        anguloAtual = Mathf.LerpAngle(anguloAtual, anguloDesejado, suavidadeRotacao * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, anguloAtual);

        // pulso de tamanho
        float pulso = 1f + Mathf.Sin(Time.time * velocidadePulso) * intensidadePulso;
        transform.localScale = escalaOriginal * pulso;
    }
}