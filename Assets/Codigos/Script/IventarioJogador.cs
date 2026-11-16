using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IventarioJogador : MonoBehaviour
{
    [Header("Mensagem")]
    [SerializeField]
    private TMP_Text mensagemTela;

    [Header("Painel do inventário")]
    [SerializeField]
    private GameObject painelInventario;

    [Header("Espaços visuais do inventário")]
    [SerializeField]
    private Image[] imagensItens;

    [SerializeField]
    private TMP_Text[] descricoesItens;

    private Item[] itensInventario = new Item[10];
    private Coroutine mensagemAtual;
    private float escalaTempoAnterior = 1f;
    private bool detalhesVisiveis;

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

        detalhesVisiveis = false;
        AtualizarInventarioNaTela();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            AlternarInventario();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UsarItemNaPosicao(0);
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = escalaTempoAnterior;
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

        MostrarMensagem("Inventário cheio");
        return false;
    }

    private void AlternarInventario()
    {
        if (painelInventario == null)
        {
            Debug.LogError(
                "O painel do inventário não foi configurado no Inspector.");
            return;
        }

        bool inventarioAberto = painelInventario.activeSelf;

        if (inventarioAberto)
        {
            painelInventario.SetActive(false);
            OcultarItensNaTela();

            Time.timeScale = escalaTempoAnterior;
            return;
        }

        escalaTempoAnterior = Time.timeScale;
        Time.timeScale = 0f;

        painelInventario.SetActive(true);
        AtualizarInventarioNaTela();
    }

    private void OcultarItensNaTela()
    {
        for (int i = 0; i < imagensItens.Length; i++)
        {
            if (imagensItens[i] != null)
            {
                imagensItens[i].gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < descricoesItens.Length; i++)
        {
            if (descricoesItens[i] != null)
            {
                descricoesItens[i].gameObject.SetActive(false);
            }
        }
    }

    private void AlternarDetalhes()
    {
        if (painelInventario == null ||
            !painelInventario.activeSelf)
        {
            return;
        }

        detalhesVisiveis = !detalhesVisiveis;
        AtualizarInventarioNaTela();
    }

    private void AtualizarInventarioNaTela()
    {
        if (painelInventario == null ||
            !painelInventario.activeSelf)
        {
            OcultarItensNaTela();
            return;
        }

        for (int i = 0; i < imagensItens.Length; i++)
        {
            if (i >= itensInventario.Length)
            {
                break;
            }

            Item item = itensInventario[i];
            bool possuiItem = item != null;

            if (imagensItens[i] != null)
            {
                imagensItens[i].gameObject.SetActive(possuiItem);

                if (possuiItem)
                {
                    imagensItens[i].sprite = item.SpriteItem;
                }
            }

            if (i < descricoesItens.Length &&
                descricoesItens[i] != null)
            {
                descricoesItens[i].gameObject.SetActive(possuiItem);

                if (possuiItem)
                {
                    descricoesItens[i].text = item.DescricaoItem;
                }
                else
                {
                    descricoesItens[i].text = string.Empty;
                }
            }
        }
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

        yield return new WaitForSecondsRealtime(2f);

        mensagemTela.gameObject.SetActive(false);
    }   

    private void UsarItemNaPosicao(int posicao)
    {
        if (posicao < 0 || posicao >= itensInventario.Length)
        {
            return;
        }

        Item item = itensInventario[posicao];

        if (item != null)
        {
            // Lógica para usar o item
            // Exemplo: item.UsoEfeito();
            MostrarMensagem("Usou item " + item.name);

            // Remover o item do inventário                     
            itensInventario[posicao] = null;
            AtualizarInventarioNaTela();
        }
    }
}
