using System;
using UnityEngine;

/// <summary>Regioes do corpo do inimigo que podem ser atingidas.</summary>
public enum TipoRegiao
{
    Cabeca,
    Tronco,
    BracoArmado,
    BracoLivre,
    PernaEsquerda,
    PernaDireita
}

/// <summary>
/// Configuracao de uma regiao do corpo. Todas as medidas em pixels do sprite do corpo
/// (origem no canto inferior esquerdo).
/// </summary>
[Serializable]
public class RegiaoCorpo
{
    public TipoRegiao tipo;
    public string nome = "Regiao";
    public int vidaMaxima = 20;
    [Tooltip("Dano recebido e multiplicado por este valor quando esta regiao e atingida.")]
    public float multiplicadorDano = 1f;
    [Range(0f, 1f)] public float precisao = 0.9f;

    [Tooltip("Area atingivel no sprite do corpo.")]
    public RectInt areaPixels;

    [Header("Peca que se solta ao perder a regiao")]
    public bool destacavel = true;
    public Sprite pecaSolta;
    public RectInt posicaoPeca;
    [Tooltip("Pixels extras apagados ao redor da peca no corpo.")]
    public int expandirRecorte = 0;

    [NonSerialized] public int vidaAtual;
    [NonSerialized] public bool perdida;
}
