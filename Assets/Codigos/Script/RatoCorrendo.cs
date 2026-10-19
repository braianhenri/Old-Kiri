using System.Collections;
using UnityEngine;

public class RatoCorrendo : MonoBehaviour
{
    [Header("Rato")]
    public GameObject rato;               // objeto do rato
    public SpriteRenderer rendererRato;   // SpriteRenderer do rato
    public Sprite[] frames;               // sprites dele andando

    [Header("Som")]
    public AudioSource somRato;           // Audio Source do rato

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

        // Toca o som depois de ativar o rato
        if (somRato != null) somRato.Play();

        // Vira o sprite de acordo com a direção do percurso
        bool indoParaEsquerda = pontoFinal.position.x < pontoInicial.position.x;
        rendererRato.flipX = (indoParaEsquerda != spriteOlhaParaEsquerda);

        int frameAtual = 0;
        float timer = 0f;

        while (Vector3.Distance(rato.transform.position, pontoFinal.position) > 0.05f)
        {
            // Movimento
            rato.transform.position = Vector3.MoveTowards(
                rato.transform.position,
                pontoFinal.position,
                velocidade * Time.deltaTime
            );

            // Animação
            timer += Time.deltaTime;
            if (timer >= tempoCadaFrame && frames.Length > 0)
            {
                timer = 0f;
                frameAtual = (frameAtual + 1) % frames.Length;
                rendererRato.sprite = frames[frameAtual];
            }

            yield return null;
        }

        // Para o som antes de desativar o rato
        if (somRato != null) somRato.Stop();

        rato.SetActive(false);
    }
}