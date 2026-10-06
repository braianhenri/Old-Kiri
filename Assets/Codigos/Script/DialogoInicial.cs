using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogoInicial : MonoBehaviour
{
    public GameObject caixaDialogo;
    public TMP_Text TextoFalasIniciais;
    public Image RostoXomiDialogando1;

    [Header("Sprites do rosto")]
    public Sprite rosto1;
    public Sprite rosto2;
    public Sprite rosto3;
    public Sprite rosto4;

    [Header("Fala")]
    [TextArea(3, 5)]
    public string fala = "Onde... onde eu estou?";

    [Header("Velocidade")]
    public float velocidadeTexto = 0.05f;
    public float tempoCadaSprite = 0.4f;

    [Header("Repetições")]
    public int repeticoesSprites = 2;

    [Header("Tempo até desaparecer")]
    public float tempoAteSumir = 3f;

    void Start()
    {
        // Ativa a caixa assim que a cena começa
        caixaDialogo.SetActive(true);

        StartCoroutine(DigitarTexto());
    }

    IEnumerator DigitarTexto()
    {
        TextoFalasIniciais.text = "";

        Sprite[] sprites =
        {
            rosto1,
            rosto2,
            rosto3,
            rosto4
        };

        int spriteAtual = 0;
        int trocas = 0;

        RostoXomiDialogando1.sprite = rosto1;

        float contador = 0f;

        int totalTrocas = (sprites.Length * repeticoesSprites) - 1;

        foreach (char letra in fala)
        {
            TextoFalasIniciais.text += letra;

            contador += velocidadeTexto;

            if (contador >= tempoCadaSprite && trocas < totalTrocas)
            {
                contador = 0f;

                trocas++;
                spriteAtual++;

                if (spriteAtual >= sprites.Length)
                    spriteAtual = 0;

                RostoXomiDialogando1.sprite = sprites[spriteAtual];
            }

            yield return new WaitForSeconds(velocidadeTexto);
        }

        RostoXomiDialogando1.sprite = rosto1;

        // Espera antes de desaparecer
        yield return new WaitForSeconds(tempoAteSumir);

        caixaDialogo.SetActive(false);
    }
}