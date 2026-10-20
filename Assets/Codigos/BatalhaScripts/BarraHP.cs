using TMPro;
using UnityEngine;

/// <summary>
/// Gerencia a barra de vida do jogador
/// </summary>
public class BarraHP : MonoBehaviour
{
    [SerializeField] private AtributosJogador atributosJogador;
    [SerializeField] private GameObject vidaVisual;
    [SerializeField] private TMP_Text textoVida;

    [Header("Animação")]
    [SerializeField] private float velocidadeAnimacao = 5f;

    private int vidaMaxima;
    private int vidaAtual;
    private float vidaVisualAtual;

    private void Start()
    {
        if (atributosJogador == null)
        {
            Debug.LogError("BarraHP: AtributosJogador não foi atribuído!");
            return;
        }

        vidaMaxima = atributosJogador.VidaMaxima();
        vidaAtual = vidaMaxima;
        vidaVisualAtual = 1f;

        AtualizarHUD();
    }

    private void Update()
    {
        // Anima a barra de vida suavemente
        if (vidaVisual != null)
        {
            float vidaPercent = (float)vidaAtual / vidaMaxima;
            vidaVisualAtual = Mathf.Lerp(vidaVisualAtual, vidaPercent, Time.deltaTime * velocidadeAnimacao);
            vidaVisual.transform.localScale = new Vector3(vidaVisualAtual, 1f, 1f);
        }
    }

    /// <summary>
    /// Atualiza o texto da vida
    /// </summary>
    private void AtualizarHUD()
    {
        if (textoVida != null)
            textoVida.text = $"{vidaAtual}/{vidaMaxima}";
    }

    /// <summary>
    /// Aplica dano ao jogador
    /// </summary>
    public void ReceberDano(int quantidade = 20)
    {
        vidaAtual -= quantidade;
        if (vidaAtual < 0)
            vidaAtual = 0;

        AtualizarHUD();
        Debug.Log($"Jogador recebeu {quantidade} de dano! Vida: {vidaAtual}");
    }

    /// <summary>
    /// Cura o jogador
    /// </summary>
    public void Curar(int quantidade = 20)
    {
        vidaAtual += quantidade;
        if (vidaAtual > vidaMaxima)
            vidaAtual = vidaMaxima;

        AtualizarHUD();
        Debug.Log($"Jogador foi curado em {quantidade}! Vida: {vidaAtual}");
    }

    /// <summary>
    /// Retorna a vida atual
    /// </summary>
    public int ObterVidaAtual() => vidaAtual;

    /// <summary>
    /// Retorna a vida máxima
    /// </summary>
    public int ObterVidaMaxima() => vidaMaxima;
}
