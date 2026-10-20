using System;
using UnityEngine;
using TMPro;

/// <summary>
/// Gerenciador principal da batalha
/// Controla o fluxo, turnos, estados e eventos da batalha
/// </summary>
public class GerenciadorBatalha : MonoBehaviour
{
    #region Singleton
    public static GerenciadorBatalha Instancia { get; private set; }

    private void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Estados de Batalha
    public enum EstadoBatalha
    {
        Inativo,      // Batalha não iniciada
        Andamento,    // Batalha em progresso
        TurnoJogador, // Esperando ação do jogador
        TurnoInimigo, // IA do inimigo agindo
        Vitoria,      // Jogador venceu
        Derrota,      // Jogador perdeu
        Pausa         // Batalha pausada
    }
    #endregion

    #region Campos Serializados
    [Header("Configuração de Batalha")]
    [SerializeField] private EstadoBatalha estadoAtual = EstadoBatalha.Inativo;
    [SerializeField] private bool permitirDebug = true;

    [Header("Referências de UI")]
    [SerializeField] private GameObject hudBatalha;
    [SerializeField] private TextMeshProUGUI textoBatalhaStatus;
    [SerializeField] private TextMeshProUGUI textoTurno;

    [Header("Referências de Personagens")]
    [SerializeField] private BarraHP barraHPJogador;
    [SerializeField] private AtributosJogador atributosJogador;

    [Header("Configuração de Turnos")]
    [SerializeField] private float delayAcaoInimigo = 1.5f;
    [SerializeField] private float delayTransicaoEstado = 0.5f;

    [Header("Áudio")]
    [SerializeField] private AudioClip somBatalhaInicio;
    [SerializeField] private AudioClip somTurnoJogador;
    [SerializeField] private AudioClip somVitoria;
    [SerializeField] private AudioClip somDerrota;
    [SerializeField] private AudioSource audioSource;
    #endregion

    #region Campos Privados
    private int turnoAtual = 0;
    private bool esperandoAcaoJogador = false;
    private float tempoUltimaAcao = 0f;
    #endregion

    #region Eventos
    public static event Action<EstadoBatalha> OnEstadoMudou;
    public static event Action<int> OnTurnoMudou;
    public static event Action OnBatalhaIniciou;
    public static event Action OnBatalhaTerminou;
    #endregion

    #region MonoBehaviour
    private void Start()
    {
        InicializarBatalha();
    }

    private void Update()
    {
        if (estadoAtual == EstadoBatalha.Andamento)
        {
            ProcessarLogicaBatalha();
        }

        // DEBUG: Teclas para testes
        if (permitirDebug)
        {
            if (Input.GetKeyDown(KeyCode.P))
                PausarBatalha();
            if (Input.GetKeyDown(KeyCode.R))
                Retomar();
            if (Input.GetKeyDown(KeyCode.E))
                FinalizarBatalha(true);
        }
    }
    #endregion

    #region Inicialização
    private void InicializarBatalha()
    {
        if (hudBatalha != null)
            hudBatalha.SetActive(false);

        Log("Sistema de batalha inicializado");
    }

    public void AtivarBatalha()
    {
        if (estadoAtual != EstadoBatalha.Inativo)
        {
            LogWarning("Batalha já está ativa!");
            return;
        }

        MudarEstado(EstadoBatalha.Andamento);

        if (hudBatalha != null)
            hudBatalha.SetActive(true);

        turnoAtual = 0;
        OnBatalhaIniciou?.Invoke();

        TocarAudio(somBatalhaInicio);
        Log("Batalha Ativada!");

        ComecaTurnoJogador();
    }
    #endregion

    #region Lógica de Turnos
    private void ProcessarLogicaBatalha()
    {
        switch (estadoAtual)
        {
            case EstadoBatalha.TurnoJogador:
                // Aguarda ação do jogador
                break;

            case EstadoBatalha.TurnoInimigo:
                // Aguarda delay antes de executar ação inimiga
                if (Time.time - tempoUltimaAcao >= delayAcaoInimigo)
                {
                    ExecutarAcaoInimiga();
                    ComecaTurnoJogador();
                }
                break;
        }
    }

    private void ComecaTurnoJogador()
    {
        turnoAtual++;
        MudarEstado(EstadoBatalha.TurnoJogador);
        esperandoAcaoJogador = true;

        if (textoTurno != null)
            textoTurno.text = $"Turno {turnoAtual}";

        TocarAudio(somTurnoJogador);
        OnTurnoMudou?.Invoke(turnoAtual);
        Log($"Turno {turnoAtual} - Ação do Jogador");
    }

    public void ExecutarAcaoJogador(int tipoAcao)
    {
        if (estadoAtual != EstadoBatalha.TurnoJogador || !esperandoAcaoJogador)
        {
            LogWarning("Não é o turno do jogador!");
            return;
        }

        esperandoAcaoJogador = false;

        // Implementar lógica de ação baseada no tipo
        switch (tipoAcao)
        {
            case 0: // Ataque Normal
                Log("Jogador executou: Ataque Normal");
                break;
            case 1: // Ataque Crítico
                Log("Jogador executou: Ataque Crítico");
                break;
            case 2: // Defesa
                Log("Jogador executou: Defesa");
                break;
            default:
                LogWarning("Tipo de ação desconhecido: " + tipoAcao);
                break;
        }

        // Transição para turno do inimigo
        MudarEstado(EstadoBatalha.TurnoInimigo);
        tempoUltimaAcao = Time.time;
    }

    private void ExecutarAcaoInimiga()
    {
        Log("Inimigo executou ação!");
        // Implementar IA do inimigo aqui
    }
    #endregion

    #region Gerenciamento de Dano
    public void AplicarDanoJogador(int quantidade)
    {
        if (barraHPJogador != null)
        {
            barraHPJogador.ReceberDano();
            Log($"Jogador recebeu {quantidade} de dano!");

            // Verifica se o jogador morreu
            if (barraHPJogador.ObterVidaAtual() <= 0)
            {
                FinalizarBatalha(false);
            }
        }
    }

    public void CurarJogador(int quantidade)
    {
        if (barraHPJogador != null)
        {
            barraHPJogador.Curar(quantidade);
            Log($"Jogador foi curado em {quantidade}!");
        }
    }
    #endregion

    #region Estados de Batalha
    private void MudarEstado(EstadoBatalha novoEstado)
    {
        if (estadoAtual == novoEstado)
            return;

        EstadoBatalha estadoAnterior = estadoAtual;
        estadoAtual = novoEstado;

        if (textoBatalhaStatus != null)
            textoBatalhaStatus.text = $"Status: {estadoAtual}";

        OnEstadoMudou?.Invoke(estadoAtual);
        Log($"Estado mudou de {estadoAnterior} para {estadoAtual}");
    }

    public void PausarBatalha()
    {
        if (estadoAtual == EstadoBatalha.Pausa)
            return;

        MudarEstado(EstadoBatalha.Pausa);
        Time.timeScale = 0f;
        Log("Batalha Pausada");
    }

    public void Retomar()
    {
        if (estadoAtual != EstadoBatalha.Pausa)
            return;

        // Voltar ao estado anterior
        if (esperandoAcaoJogador)
            MudarEstado(EstadoBatalha.TurnoJogador);
        else
            MudarEstado(EstadoBatalha.TurnoInimigo);

        Time.timeScale = 1f;
        Log("Batalha Retomada");
    }

    public void FinalizarBatalha(bool jogadorVenceu)
    {
        EstadoBatalha estadoFinal = jogadorVenceu ? EstadoBatalha.Vitoria : EstadoBatalha.Derrota;
        MudarEstado(estadoFinal);

        Time.timeScale = 1f;
        OnBatalhaTerminou?.Invoke();

        if (jogadorVenceu)
        {
            TocarAudio(somVitoria);
            Log("Batalha Vencida!");
        }
        else
        {
            TocarAudio(somDerrota);
            Log("Batalha Perdida!");
        }
    }
    #endregion

    #region Utilitários
    private void TocarAudio(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    private void Log(string mensagem)
    {
        if (permitirDebug)
            Debug.Log($"[GerenciadorBatalha] {mensagem}");
    }

    private void LogWarning(string mensagem)
    {
        Debug.LogWarning($"[GerenciadorBatalha] {mensagem}");
    }

    // Getters públicos
    public EstadoBatalha ObterEstadoAtual() => estadoAtual;
    public int ObterTurnoAtual() => turnoAtual;
    public bool EstaEsperandoAcaoJogador() => esperandoAcaoJogador;
    #endregion
}
