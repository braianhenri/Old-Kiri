using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogoGatilho : MonoBehaviour
{
    [Header("Caixa de diálogo")]
    public GameObject caixaDialogo;
    public TMP_Text texto;

    [Header("Rosto")]
    public Image rostoImagem;
    public Sprite rostoParado;
    public Sprite rostoFalando;

    [Header("Fala")]
    [TextArea(3, 5)]
    public string fala;

    [Header("Velocidade")]
    public float velocidadeTexto = 0.05f;

    [Header("Tempo até desaparecer")]
    public float tempoAteSumir = 3f;

    [Header("Som de digitação")]
    public AudioSource audioSource;
    public AudioClip somTeclado;
    public float intervaloSom = 0.08f;

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

        if (rostoImagem != null && rostoParado != null)
            rostoImagem.sprite = rostoParado;

        float contadorSom = 0f;

        foreach (char letra in fala)
        {
            texto.text += letra;

            if (letra != ' ')
            {
                if (audioSource != null && somTeclado != null && contadorSom <= 0f)
                {
                    audioSource.PlayOneShot(somTeclado);
                    contadorSom = intervaloSom;
                }

                if (rostoImagem != null && rostoParado != null && rostoFalando != null)
                {
                    rostoImagem.sprite =
                        (rostoImagem.sprite == rostoParado) ? rostoFalando : rostoParado;
                }
            }

            contadorSom -= velocidadeTexto;
            yield return new WaitForSeconds(velocidadeTexto);
        }

        if (rostoImagem != null && rostoParado != null)
            rostoImagem.sprite = rostoParado;

        yield return new WaitForSeconds(tempoAteSumir);
        caixaDialogo.SetActive(false);
    }
}