using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Sistema de navegação em grid com seleção de texto e alteração de cor
/// Permite navegar com todas as 4 setas do teclado em um layout de grid
/// </summary>
public class Navegacao : MonoBehaviour
{
    [Header("Configuração de Cores")]
    [SerializeField] private Color corSelecionada = Color.yellow;
    [SerializeField] private Color corDeselecionada = Color.white;

    [Header("Textos do Menu")]
    [SerializeField] private TextMeshProUGUI[] textoOpcoes;

    [Header("Configuração de Grid")]
    [SerializeField] private int colunasGrid = 2; // Número de colunas no grid

    [Header("Som (Opcional)")]
    [SerializeField] private AudioClip somSeleção;
    [SerializeField] private AudioSource audioSource;

    private int indiceAtual = 0;
    private int totalOpcoes = 0;
    private int totalLinhas = 0;

    private void Start()
    {
        // Valida se há opções configuradas
        if (textoOpcoes == null || textoOpcoes.Length == 0)
        {
            Debug.LogError("Navegacao: Nenhum texto de opção foi configurado!");
            return;
        }

        totalOpcoes = textoOpcoes.Length;
        
        // Calcula o número de linhas necessárias
        totalLinhas = Mathf.CeilToInt((float)totalOpcoes / colunasGrid);

        // Inicializa com primeira opção selecionada
        AtualizarSelecao();
    }

    private void Update()
    {
        // Navegar para cima
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoverCima();
            AtualizarSelecao();
        }

        // Navegar para baixo
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoverBaixo();
            AtualizarSelecao();
        }

        // Navegar para direita
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoverDireita();
            AtualizarSelecao();
        }

        // Navegar para esquerda
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoverEsquerda();
            AtualizarSelecao();
        }

        // Selecionar opção (Enter ou Space)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            SelecionarOpcaoAtual();
        }
    }

    /// <summary>
    /// Move para a opção acima na grid
    /// </summary>
    private void MoverCima()
    {
        int novoIndice = indiceAtual - colunasGrid;

        // Se ficar negativo, fica na mesma posição (não volta ao final)
        if (novoIndice >= 0)
        {
            indiceAtual = novoIndice;
        }
    }

    /// <summary>
    /// Move para a opção abaixo na grid
    /// </summary>
    private void MoverBaixo()
    {
        int novoIndice = indiceAtual + colunasGrid;

        // Se ultrapassar o total, fica na mesma posição (não volta ao início)
        if (novoIndice < totalOpcoes)
        {
            indiceAtual = novoIndice;
        }
    }

    /// <summary>
    /// Move para a opção à direita na grid
    /// </summary>
    private void MoverDireita()
    {
        int coluna = indiceAtual % colunasGrid;
        int linha = indiceAtual / colunasGrid;

        // Se não estiver na última coluna
        if (coluna < colunasGrid - 1)
        {
            int novoIndice = indiceAtual + 1;
            
            // Verifica se a nova posição ainda está dentro do array
            if (novoIndice < totalOpcoes)
            {
                indiceAtual = novoIndice;
            }
        }
    }

    /// <summary>
    /// Move para a opção à esquerda na grid
    /// </summary>
    private void MoverEsquerda()
    {
        int coluna = indiceAtual % colunasGrid;

        // Se não estiver na primeira coluna
        if (coluna > 0)
        {
            indiceAtual--;
        }
    }

    /// <summary>
    /// Atualiza as cores dos textos baseado na seleção
    /// </summary>
    private void AtualizarSelecao()
    {
        for (int i = 0; i < textoOpcoes.Length; i++)
        {
            if (i == indiceAtual)
            {
                // Texto selecionado: cor selecionada
                textoOpcoes[i].color = corSelecionada;
            }
            else
            {
                // Texto não selecionado: cor deselecionada
                textoOpcoes[i].color = corDeselecionada;
            }
        }

        // Reproduz som de seleção se disponível
        if (somSeleção != null && audioSource != null)
        {
            audioSource.PlayOneShot(somSeleção);
        }
    }

    /// <summary>
    /// Executa a ação da opção selecionada
    /// </summary>
    private void SelecionarOpcaoAtual()
    {
        Debug.Log($"Opção selecionada: {textoOpcoes[indiceAtual].text}");

        // Tenta invocar o Button associado ao TextMeshProUGUI
        Button botao = textoOpcoes[indiceAtual].GetComponentInParent<Button>();
        if (botao != null)
        {
            botao.onClick.Invoke();
        }
    }

    /// <summary>
    /// Método público para mudar a cor de seleção
    /// </summary>
    public void MudarCorSelecionada(Color novaCor)
    {
        corSelecionada = novaCor;
        AtualizarSelecao();
    }

    /// <summary>
    /// Método público para mudar a cor de deseleção
    /// </summary>
    public void MudarCorDeselecionada(Color novaCor)
    {
        corDeselecionada = novaCor;
        AtualizarSelecao();
    }

    /// <summary>
    /// Retorna o índice da opção atualmente selecionada
    /// </summary>
    public int ObterIndiceSelecionado()
    {
        return indiceAtual;
    }

    /// <summary>
    /// Define qual opção deve ser selecionada
    /// </summary>
    public void DefinirSelecionada(int novoIndice)
    {
        if (novoIndice >= 0 && novoIndice < textoOpcoes.Length)
        {
            indiceAtual = novoIndice;
            AtualizarSelecao();
        }
        else
        {
            Debug.LogWarning($"Navegacao: Índice {novoIndice} fora do intervalo!");
        }
    }

    /// <summary>
    /// Muda o número de colunas do grid
    /// </summary>
    public void DefinirColunasGrid(int novasColunasGrid)
    {
        if (novasColunasGrid > 0)
        {
            colunasGrid = novasColunasGrid;
            totalLinhas = Mathf.CeilToInt((float)totalOpcoes / colunasGrid);
        }
        else
        {
            Debug.LogWarning("Navegacao: O número de colunas deve ser maior que 0!");
        }
    }
}                   
