using System.Collections;
using UnityEngine;

// Marta aparece no topo da tela correndo, para perto do Xomi so pra avisar (2 falas, sem escolhas),
// sai correndo pra baixo e libera o senhorio (SenhorioAbordagem). O Xomi nao interage com ela.
public class MartaCorrida : MonoBehaviour
{
    public DialogoTulipa dialogo;
    [Tooltip("O senhorio so aparece depois que a Marta termina e sai")]
    public SenhorioAbordagem senhorio;
    public float atrasoInicio = 1f;
    public float velocidade = 7f;
    [Tooltip("Distancia vertical em que ela para, de frente pro Xomi")]
    public float distanciaParada = 4.4f;
    public string parametroCorrendo = "Correndo";
    [Tooltip("Altura (Y) abaixo da tela onde ela some ao sair")]
    public float yDeSaida = -44f;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;
    Transform xomi;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = false;
    }

    IEnumerator Start()
    {
        xomi = CutsceneUtil.AcharXomi();
        if (xomi == null) yield break;

        // Marta e so um "efeito": sem colisao, nao empurra nem e empurrada
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        rb.gravityScale = 0f;

        yield return new WaitForSeconds(atrasoInicio);

        // Nasce logo acima da borda de cima da tela e corre pra dentro
        Camera cam = Camera.main;
        float y = Mathf.Min(cam.transform.position.y + cam.orthographicSize + 2f, 37f);
        float x = Mathf.Clamp(xomi.position.x + 3f, -9f, 9f);
        rb.position = new Vector2(x, y);
        transform.position = new Vector3(x, y, transform.position.z);
        sr.enabled = true;
        anim.SetBool(parametroCorrendo, true);

        yield return StartCoroutine(IrPara(() => new Vector2(xomi.position.x, xomi.position.y + distanciaParada), 0.2f));

        // chegou: para, Xomi olha pra ela e ela diz as duas falas
        anim.SetBool(parametroCorrendo, false);
        CutsceneUtil.XomiOlhar(true);
        if (dialogo != null)
        {
            CutsceneUtil.TravarXomi(true);
            dialogo.Iniciar();
            yield return null;
            while (dialogo.caixaDialogo != null && dialogo.caixaDialogo.activeSelf) yield return null;
            CutsceneUtil.TravarXomi(false);
        }

        // sai correndo ("ali embaixo"), passando ao lado do Xomi
        anim.SetBool(parametroCorrendo, true);
        float lado = rb.position.x >= xomi.position.x ? 1f : -1f;
        Vector2 saida = new Vector2(Mathf.Clamp(xomi.position.x + 6f * lado, -9f, 9f), yDeSaida);
        yield return StartCoroutine(IrPara(() => saida, 0.5f));

        anim.SetBool(parametroCorrendo, false);
        sr.enabled = false;
        if (senhorio != null) senhorio.Armar();
        gameObject.SetActive(false);
    }

    IEnumerator IrPara(System.Func<Vector2> alvo, float tolerancia)
    {
        while (true)
        {
            Vector2 falta = alvo() - rb.position;
            if (falta.magnitude < tolerancia) break;
            rb.velocity = falta.normalized * velocidade;
            yield return new WaitForFixedUpdate();
        }
        rb.velocity = Vector2.zero;
    }
}
