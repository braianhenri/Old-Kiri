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

        // vida global: continua valendo entre cenas (cura de itens, dano da batalha)
        VidaJogador.DefinirMaxima(vidaMaxima);
        if (VidaJogador.Atual <= 0) VidaJogador.Definir(vidaMaxima); // nova batalha depois de derrota: volta cheio
        vidaAtual = VidaJogador.Atual;
        vidaVisualAtual = (float)vidaAtual / vidaMaxima;

        AtualizarHUD();
    }

    private void OnEnable()
    {
        VidaJogador.OnMudou += AoVidaGlobalMudar;
    }

    private void OnDisable()
    {
        VidaJogador.OnMudou -= AoVidaGlobalMudar;
    }

    // Alguem mudou a vida fora daqui (ex: item de cura): acompanha.
    private void AoVidaGlobalMudar(int atual, int maxima)
    {
        if (vidaMaxima <= 0) return; // ainda nao iniciou
        if (atual == vidaAtual && maxima == vidaMaxima) return;

        vidaMaxima = maxima;
        vidaAtual = atual;
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
        VidaJogador.Definir(vidaAtual);
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
        VidaJogador.Definir(vidaAtual);
        Debug.Log($"Jogador foi curado em {quantidade}! Vida: {vidaAtual}");
    }

    /// <summary>
    /// Retorna a vida atual
    /// </summary>
    public void Restaurar()
    {
        vidaAtual = vidaMaxima;
        VidaJogador.Definir(vidaAtual);
        AtualizarHUD();
    }

    public int ObterVidaAtual() => vidaAtual;

    /// <summary>
    /// Retorna a vida máxima
    /// </summary>
    public int ObterVidaMaxima() => vidaMaxima;
}
