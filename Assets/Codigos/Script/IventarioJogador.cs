using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public class IventarioJogador : MonoBehaviour
{
    [Header("Mensagem")]
    [SerializeField]
    private TMP_Text mensagemTela;

    [Header("Inventário")]
    [SerializeField]
    private GameObject painelInventario;

    [SerializeField]
    private TMP_Text textoItens;

    private Item[] itensInventario = new Item[10];
    private Coroutine mensagemAtual;

    private void Start()
    {
        if (mensagemTela != null)
        {
            mensagemTela.gameObject.SetActive(false);
        }

        if (painelInventario != null)
        {
            painelInventario.SetActive(false);
        }

        AtualizarInventarioNaTela();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            AlternarInventario();
        }
    }

    public bool AdicionarItem(Item item)
    {
        if (item == null)
        {
            MostrarMensagem("Nenhum item");
            return false;
        }

        for (int i = 0; i < itensInventario.Length; i++)
        {
            if (itensInventario[i] == null)
            {
                itensInventario[i] = item;

                MostrarMensagem("Item pego");
                AtualizarInventarioNaTela();

                return true;
            }
        }

        MostrarMensagem("Nenhum item");
        return false;
    }

    private void AlternarInventario()
    {
        if (painelInventario == null)
        {
            Debug.LogError("O painel do inventário não foi configurado.");
            return;
        }

        bool inventarioAberto = painelInventario.activeSelf;
        painelInventario.SetActive(!inventarioAberto);

        if (!inventarioAberto)
        {
            AtualizarInventarioNaTela();
        }
    }

    private void AtualizarInventarioNaTela()
    {
        if (textoItens == null)
        {
            return;
        }

        StringBuilder texto = new StringBuilder();

        for (int i = 0; i < itensInventario.Length; i++)
        {
            if (itensInventario[i] != null)
            {
                texto.AppendLine(
                    (i + 1) + " - " + itensInventario[i].name);
            }
        }

        if (texto.Length == 0)
        {
            texto.Append("Inventário vazio");
        }

        textoItens.text = texto.ToString();
    }

    private void MostrarMensagem(string mensagem)
    {
        if (mensagemTela == null)
        {
            return;
        }

        if (mensagemAtual != null)
        {
            StopCoroutine(mensagemAtual);
        }

        mensagemAtual = StartCoroutine(
            ExibirMensagemTemporariamente(mensagem));
    }

    private IEnumerator ExibirMensagemTemporariamente(string mensagem)
    {
        mensagemTela.text = mensagem;
        mensagemTela.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        mensagemTela.gameObject.SetActive(false);
    }
}
