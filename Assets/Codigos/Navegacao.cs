using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NavegacaoBatalha : MonoBehaviour
{
    public Button[] botoes;

    public Color corNormal = Color.white;
    public Color corSelecionado = Color.yellow;

    private int indiceSelecionado = 0;

    void Start()
    {
        AtualizarSelecao();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.DownArrow))
        {
            indiceSelecionado++;

            if (indiceSelecionado >= botoes.Length)
                indiceSelecionado = 0;

            AtualizarSelecao();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) ||
            Input.GetKeyDown(KeyCode.UpArrow))
        {
            indiceSelecionado--;

            if (indiceSelecionado < 0)
                indiceSelecionado = botoes.Length - 1;

            AtualizarSelecao();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            botoes[indiceSelecionado].onClick.Invoke();
        }
    }

    void AtualizarSelecao()
    {
        for (int i = 0; i < botoes.Length; i++)
        {
            botoes[i].targetGraphic.color = corNormal;
        }

        botoes[indiceSelecionado].targetGraphic.color =
            corSelecionado;
    }
}
