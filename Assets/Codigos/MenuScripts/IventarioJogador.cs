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

    [Header("Botão para usar o item selecionado")]
    [SerializeField]
    private Button botaoUsar;

    [Header("Quantidade (ex: 1x) ao lado de cada item")]
    [SerializeField]
    private float tamanhoFonteQuantidade = 30f;

    [Tooltip("Posição do texto em relação ao centro do ícone do item.")]
    [SerializeField]
    private Vector2 deslocamentoQuantidade = new Vector2(-79f, 0f);

    [SerializeField]
    private float larguraQuantidade = 36f;

    private const int LimitePilha = 99;

    // Os itens ficam no EstadoXomi para continuarem iguais em todas as cenas.
    private DadosItem[] itensInventario { get { return EstadoXomi.Itens; } }
    private int[] quantidadesInventario { get { return EstadoXomi.Quantidades; } }
    private TMP_Text[] textosQuantidade;
    private Coroutine mensagemAtual;
    private Coroutine descricaoAtual;
    private int itemSelecionado = -1;
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

        CriarTextosQuantidade();
        RegistrarEventosDosItens();
        OcultarItensNaTela();

        if (botaoUsar != null)
        {
            botaoUsar.onClick.AddListener(UsarItemSelecionado);
        }

        LimparSelecao();
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

        // Já tem esse tipo de item: soma na pilha (1x, 2x, 3x...)
        for (int i = 0; i < itensInventario.Length; i++)
        {
            if (itensInventario[i] != null &&
                MesmoTipo(itensInventario[i], item) &&
                quantidadesInventario[i] < LimitePilha)
            {
                quantidadesInventario[i]++;

                MostrarMensagem("Item pego");
                AtualizarInventarioNaTela();

                return true;
            }
        }

        for (int i = 0; i < itensInventario.Length; i++)
        {
            if (itensInventario[i] == null)
            {
                itensInventario[i] = new DadosItem(item);
                quantidadesInventario[i] = 1;

                MostrarMensagem("Item pego");
                AtualizarInventarioNaTela();

                return true;
            }
        }

        MostrarMensagem("Inventário cheio");
        return false;
    }

    private static bool MesmoTipo(DadosItem a, Item b)
    {
        return (a.SpriteItem != null || a.Nome == b.name) &&
               a.SpriteItem == b.SpriteItem &&
               a.DescricaoItem == b.DescricaoItem &&
               a.Caracteristica == b.Caracteristica &&
               a.Valor == b.Valor;
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
            LimparSelecao();

            Time.timeScale = escalaTempoAnterior;
            return;
        }

        escalaTempoAnterior = Time.timeScale;
        Time.timeScale = 0f;

        painelInventario.SetActive(true);
        LimparSelecao();
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

            DefinirQuantidadeNaTela(i, 0);
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

            DadosItem item = itensInventario[i];
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

            DefinirQuantidadeNaTela(i, possuiItem ? quantidadesInventario[i] : 0);
        }
    }

    // ---------- quantidade (1x) ----------

    private void CriarTextosQuantidade()
    {
        if (imagensItens == null)
        {
            return;
        }

        textosQuantidade = new TMP_Text[imagensItens.Length];

        for (int i = 0; i < imagensItens.Length; i++)
        {
            Image imagem = imagensItens[i];

            if (imagem == null)
            {
                continue;
            }

            RectTransform alvo = imagem.rectTransform;

            GameObject objeto = new GameObject(
                "Quantidade_" + i, typeof(RectTransform));

            RectTransform rect = objeto.GetComponent<RectTransform>();
            rect.SetParent(alvo.parent, false);
            rect.anchorMin = alvo.anchorMin;
            rect.anchorMax = alvo.anchorMax;
            rect.pivot = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(larguraQuantidade, 60f);
            rect.anchoredPosition =
                alvo.anchoredPosition + deslocamentoQuantidade;

            TextMeshProUGUI texto = objeto.AddComponent<TextMeshProUGUI>();

            if (descricaoItemTela != null)
            {
                texto.font = descricaoItemTela.font;
            }

            texto.enableAutoSizing = true;
            texto.fontSizeMin = 14f;
            texto.fontSizeMax = tamanhoFonteQuantidade;
            texto.fontStyle = FontStyles.Bold;
            texto.alignment = TextAlignmentOptions.MidlineRight;
            texto.color = Color.white;
            texto.raycastTarget = false;

            objeto.SetActive(false);
            textosQuantidade[i] = texto;
        }
    }

    private void DefinirQuantidadeNaTela(int indice, int quantidade)
    {
        if (textosQuantidade == null ||
            indice < 0 ||
            indice >= textosQuantidade.Length ||
            textosQuantidade[indice] == null)
        {
            return;
        }

        bool mostrar = quantidade > 0;
        textosQuantidade[indice].gameObject.SetActive(mostrar);

        if (mostrar)
        {
            textosQuantidade[indice].text = quantidade + "x";
        }
    }

    // ---------- mensagens e uso ----------

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

        DadosItem item = itensInventario[posicao];

        if (item == null)
        {
            return;
        }

        // Se o item nao teve efeito, ele nao e gasto.
        if (!AplicarEfeito(item))
        {
            return;
        }

        quantidadesInventario[posicao]--;

        if (quantidadesInventario[posicao] <= 0)
        {
            quantidadesInventario[posicao] = 0;
            itensInventario[posicao] = null;
            OcultarDescricao();

            if (itemSelecionado == posicao)
            {
                LimparSelecao();
            }
        }

        AtualizarInventarioNaTela();
    }

    private bool AplicarEfeito(DadosItem item)
    {
        if (item.Caracteristica == CaracteristicaItem.Nenhuma || item.Valor == 0)
        {
            MostrarMensagem(NomeDoItem(item) + " não pode ser usado");
            return false;
        }

        if (item.Caracteristica == CaracteristicaItem.RecuperarVida)
        {
            if (VidaJogador.EstaCheia)
            {
                MostrarMensagem("Sua vida já está cheia (" + VidaJogador.Atual + "/" + VidaJogador.Maxima + ")");
                return false;
            }

            int curou = VidaJogador.Curar(item.Valor);
            MostrarMensagem("Usou " + NomeDoItem(item) + ": +" + curou + " de vida (" + VidaJogador.Atual + "/" + VidaJogador.Maxima + ")");
            return true;
        }

        Atributos atributos = GetComponent<Atributos>();

        if (atributos == null)
        {
            Debug.LogError("IventarioJogador: o Xomi está sem o componente Atributos.", this);
            return false;
        }

        int total = atributos.Somar(item.Caracteristica, item.Valor);
        string nomeAtributo = item.Caracteristica == CaracteristicaItem.Forca
            ? "Força"
            : item.Caracteristica.ToString();

        MostrarMensagem("Usou " + NomeDoItem(item) + ": " + item.Valor.ToString("+#;-#") + " em " + nomeAtributo + " (agora " + total + ")");
        return true;
    }

    private void UsarItemSelecionado()
    {
        UsarItemNaPosicao(itemSelecionado);
    }

    // Sem item selecionado o botao Usar fica desativado.
    private void LimparSelecao()
    {
        itemSelecionado = -1;

        if (botaoUsar != null)
        {
            botaoUsar.interactable = false;
        }
    }

    // Nome para mostrar (ex: Item_LataVermelha vira Lata Vermelha)
    private static string NomeDoItem(DadosItem item)
    {
        string nome = item.Nome.Replace("(Clone)", "").Trim();

        if (nome.StartsWith("Item_", System.StringComparison.OrdinalIgnoreCase))
        {
            nome = nome.Substring(5);
        }

        nome = nome.Replace("_", " ");

        System.Text.StringBuilder resultado = new System.Text.StringBuilder();

        for (int i = 0; i < nome.Length; i++)
        {
            if (i > 0 && char.IsUpper(nome[i]) && char.IsLower(nome[i - 1]))
            {
                resultado.Append(' ');
            }

            resultado.Append(nome[i]);
        }

        return resultado.ToString();
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

        DadosItem item = itensInventario[indice];

        if (item == null)
        {
            return;
        }

        itemSelecionado = indice;

        if (botaoUsar != null)
        {
            botaoUsar.interactable = true;
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
