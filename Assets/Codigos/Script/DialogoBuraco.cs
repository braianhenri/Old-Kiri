using System.Collections;
using UnityEngine;
using TMPro;

public class DialogoBuraco : MonoBehaviour
{
    [Header("Caixa de diálogo")]
    public GameObject caixaDialogo;
    public TMP_Text texto;

    [Header("Fala")]
    [TextArea(3, 5)]
    public string fala = "Nunca vi uma rua tão despedaçada... Onde eu nasci não é assim.";

    [Header("Velocidade")]
    public float velocidadeTexto = 0.05f;

    [Header("Tempo até desaparecer")]
    public float tempoAteSumir = 3f;

    private bool jaFalou = false;

    private void Start()
    {
        caixaDialogo.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Player") && !jaFalou)
        {
            jaFalou = true;
            StartCoroutine(MostrarDialogo());
        }
    }

    IEnumerator MostrarDialogo()
    {
        caixaDialogo.SetActive(true);
        texto.text = "";

        foreach (char letra in fala)
        {
            texto.text += letra;
            yield return new WaitForSeconds(velocidadeTexto);
        }

        yield return new WaitForSeconds(tempoAteSumir);

        caixaDialogo.SetActive(false);
    }
}
