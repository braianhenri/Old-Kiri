using System;
using UnityEngine;

/// <summary>
/// Vida do Xomi que vale em qualquer cena (exploracao e batalha).
/// Guarda o valor atual para a vida nao "resetar" ao trocar de cena e avisa quem estiver
/// mostrando a vida (a BarraHP). Itens de cura (ex: lata vermelha) mexem aqui.
/// </summary>
public static class VidaJogador
{
    public const int VidaMaximaPadrao = 100;

    private static int maxima = VidaMaximaPadrao;
    private static int atual = VidaMaximaPadrao;

    /// <summary>Avisa quando a vida muda: (vida atual, vida maxima).</summary>
    public static event Action<int, int> OnMudou;

    public static int Atual { get { return atual; } }
    public static int Maxima { get { return maxima; } }
    public static bool EstaCheia { get { return atual >= maxima; } }

    // Garante comecar limpo a cada vez que o jogo roda (mesmo com "Enter Play Mode Options" ligado).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Resetar()
    {
        maxima = VidaMaximaPadrao;
        atual = VidaMaximaPadrao;
        OnMudou = null;
    }

    /// <summary>Define a vida maxima real (a BarraHP calcula pelos atributos do jogador).</summary>
    public static void DefinirMaxima(int novaMaxima)
    {
        novaMaxima = Mathf.Max(1, novaMaxima);
        if (novaMaxima == maxima) return;

        bool estavaCheia = atual >= maxima;
        maxima = novaMaxima;
        atual = estavaCheia ? maxima : Mathf.Min(atual, maxima);
        Avisar();
    }

    /// <summary>Define a vida atual (entre 0 e a maxima).</summary>
    public static void Definir(int valor)
    {
        valor = Mathf.Clamp(valor, 0, maxima);
        if (valor == atual) return;

        atual = valor;
        Avisar();
    }

    /// <summary>Cura e devolve quanto realmente curou (0 se ja estava cheia).</summary>
    public static int Curar(int quantidade)
    {
        int antes = atual;
        Definir(atual + Mathf.Max(0, quantidade));
        return atual - antes;
    }

    private static void Avisar()
    {
        if (OnMudou != null) OnMudou.Invoke(atual, maxima);
    }
}
