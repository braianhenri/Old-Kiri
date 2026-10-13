using System.Collections;
using UnityEngine;

public class RatoCorrendo : MonoBehaviour
{
    [Header("Rato")]
    public GameObject rato;               // objeto do rato
    public SpriteRenderer rendererRato;   // SpriteRenderer do rato
    public Sprite[] frames;               // sprites dele andando

    [Header("Percurso")]
    public Transform pontoInicial;
    public Transform pontoFinal;
    public float velocidade = 4f;

    [Header("Animação")]
    public float tempoCadaFrame = 0.1f;

    [Header("Direção")]
    public bool spriteOlhaParaEsquerda = false;

    private bool jaPassou = false;

    private void Start()
    {
        rato.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Player") && !jaPassou)
        {
            jaPassou = true;
            StartCoroutine(Correr());
        }
    }

    IEnumerator Correr()
    {
        rato.transform.position = pontoInicial.position;
        rato.SetActive(true);

        // Vira o sprite de acordo com a direção do percurso
        bool indoParaEsquerda = pontoFinal.position.x < pontoInicial.position.x;
        rendererRato.flipX = spriteOlhaParaEsquerda ? !indoParaEsquerda : indoParaEsquerda;

        int frameAtual = 0;
        float contador = 0f;

        if (frames.Length > 0)
            rendererRato.sprite = frames[0];

        while (Vector3.Distance(rato.transform.position, pontoFinal.position) > 0.05f)
        {
            rato.transform.position = Vector3.MoveTowards(
                rato.transform.position,
                pontoFinal.position,
                velocidade * Time.deltaTime);

            contador += Time.deltaTime;
            if (frames.Length > 1 && contador >= tempoCadaFrame)
            {
                contador = 0f;
                frameAtual = (frameAtual + 1) % frames.Length;
                rendererRato.sprite = frames[frameAtual];
            }

            yield return null;
        }

        rato.SetActive(false);
    }
}