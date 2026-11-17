    using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IventarioJogador : MonoBehaviour
{
    [Header("Mensagem")]
    [SerializeField]
    private TMP_Text mensagemTela;

    [Header("Descrição do item")]
    [SerializeField]
    private TMP_Text descricaoItemTela;

    [SerializeField]
    private float velocidadeDigitacao = 0.03f;

    [Header("Painel do inventário")]
    [SerializeField]
    private GameObject painelInventario;

    [Header("Espaços visuais do inventário")]
    [SerializeField]
    private Image[] imagensItens;

    [Header("Texto exibido ao abrir o inventário")]
    [SerializeField]
    private GameObject textoInventario;

    private Item[] itensInventario = new Item[10];
    private Coroutine mensagemAtual;
    private Coroutine descricaoAtual;
    private float escalaTempoAnterior = 1f;

    private void Start()
    {
        if (mensagemTela != null)
        {
            mensagemTela.gameObject.SetActive(false);
        }

        if (descricaoItemTela != null)
        {
            descricaoItemTela.text = string.Empty;
            descricaoItemTela.gameObject.SetActive(false);
        }

        if (painelInventario != null)
        {
            painelInventario.SetActive(false);
        }

        if (textoInventario != null)
        {
            textoInventario.SetActive(false);
        }

        RegistrarEventosDosItens();
        OcultarItensNaTela();
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
            OcultarDescricao();

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
        if (imagensItens == null)
        {
            return;
        }

        for (int i = 0; i < imagensItens.Length; i++)
        {
            if (imagensItens[i] != null)
            {
                imagensItens[i].gameObject.SetActive(false);
            }
        }
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

            if (imagensItens[i] == null)
            {
                continue;
            }

            imagensItens[i].gameObject.SetActive(possuiItem);

            if (possuiItem)
            {
                imagensItens[i].sprite = item.SpriteItem;
                imagensItens[i].raycastTarget = true;
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

        if (item == null)
        {
            return;
        }

        MostrarMensagem("Usou item " + item.name);

        itensInventario[posicao] = null;
        OcultarDescricao();
        AtualizarInventarioNaTela();
    }

    private void RegistrarEventosDosItens()
    {
        if (imagensItens == null)
        {
            Debug.LogError(
                "O campo Imagens Itens não foi preenchido no Inspector.");
            return;
        }

        for (int i = 0; i < imagensItens.Length; i++)
        {
            if (imagensItens[i] == null)
            {
                continue;
            }

            EventTrigger evento =
                imagensItens[i].GetComponent<EventTrigger>();

            if (evento == null)
            {
                evento = imagensItens[i].gameObject
                    .AddComponent<EventTrigger>();
            }

            int indiceItem = i;

            EventTrigger.Entry entradaClique =
                new EventTrigger.Entry();

            entradaClique.eventID =
                EventTriggerType.PointerClick;

            entradaClique.callback.AddListener(
                dados => MostrarDescricao(indiceItem));

            evento.triggers.Add(entradaClique);
        }
    }

    private void MostrarDescricao(int indice)
    {
        if (painelInventario == null ||
            !painelInventario.activeSelf ||
            descricaoItemTela == null)
        {
            return;
        }

        if (indice < 0 ||
            indice >= itensInventario.Length)
        {
            return;
        }

        Item item = itensInventario[indice];

        if (item == null)
        {
            return;
        }

        if (descricaoAtual != null)
        {
            StopCoroutine(descricaoAtual);
        }

        descricaoAtual = StartCoroutine(
            DigitarDescricao(item.DescricaoItem));
    }

    private IEnumerator DigitarDescricao(string descricao)
    {
        descricaoItemTela.text = string.Empty;
        descricaoItemTela.gameObject.SetActive(true);

        if (string.IsNullOrEmpty(descricao))
        {
            yield break;
        }

        foreach (char caractere in descricao)
        {
            descricaoItemTela.text += caractere;

            yield return new WaitForSecondsRealtime(
                velocidadeDigitacao);
        }
    }

    private void OcultarDescricao()
    {
        if (descricaoAtual != null)
        {
            StopCoroutine(descricaoAtual);
            descricaoAtual = null;
        }

        if (descricaoItemTela == null)
        {
            return;
        }

        descricaoItemTela.text = string.Empty;
        descricaoItemTela.gameObject.SetActive(false);
    }
}
