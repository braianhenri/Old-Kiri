using System.Collections;
using UnityEngine;

// Marta aparece no topo da tela, corre ate o Xomi, para e inicia o dialogo (DialogoTulipa).
public class MartaCorrida : MonoBehaviour
{
    public DialogoTulipa dialogo;
    public float atrasoInicio = 1f;
    public float velocidade = 7f;
    [Tooltip("Distancia vertical em que ela para, de frente pro Xomi")]
    public float distanciaParada = 3.8f;
    public string parametroCorrendo = "Correndo";

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;
    Transform xomi;
    bool correndo;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = false;
    }

    IEnumerator Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p == null) yield break;
        xomi = p.transform;

        // Marta nao empurra o Xomi
        Collider2D meu = GetComponent<Collider2D>();
        foreach (Collider2D c in p.GetComponents<Collider2D>())
            Physics2D.IgnoreCollision(meu, c);

        yield return new WaitForSeconds(atrasoInicio);

        // Nasce logo acima da borda de cima da tela e corre pra dentro
        Camera cam = Camera.main;
        float y = cam.transform.position.y + cam.orthographicSize + 2f;
        float x = Mathf.Clamp(xomi.position.x + 3f, -9f, 9f);
        rb.position = new Vector2(x, Mathf.Min(y, 37f));
        sr.enabled = true;
        correndo = true;
        anim.SetBool(parametroCorrendo, true);
    }

    void FixedUpdate()
    {
        if (!correndo) return;

        Vector2 alvo = new Vector2(xomi.position.x, xomi.position.y + distanciaParada);
        Vector2 falta = alvo - rb.position;
        if (falta.magnitude < 0.2f) { Chegou(); return; }
        rb.velocity = falta.normalized * velocidade;
    }

    void Chegou()
    {
        correndo = false;
        rb.velocity = Vector2.zero;
        anim.SetBool(parametroCorrendo, false); // estado parado

        // Xomi olha pra ela
        Animator ax = xomi.GetComponent<Animator>();
        if (ax != null) { ax.SetBool("Cima", true); ax.SetBool("Baixo", false); }

        if (dialogo != null) dialogo.Iniciar();

        // trava o Xomi no lugar (o dialogo ja desligou os scripts de movimento)
        Rigidbody2D rbx = xomi.GetComponent<Rigidbody2D>();
        if (rbx != null) rbx.velocity = Vector2.zero;
        if (ax != null) { ax.SetBool("Andando", false); ax.SetBool("Lado", false); }
    }
}
