using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Dados de um item guardado no inventario. Nao depende do objeto da cena,
/// por isso continua existindo quando a cena troca.
/// </summary>
public class DadosItem
{
    public readonly string Nome;
    public readonly Sprite SpriteItem;
    public readonly string DescricaoItem;
    public readonly int CuraVida;

    public DadosItem(Item item)
    {
        Nome = item.name;
        SpriteItem = item.SpriteItem;
        DescricaoItem = item.DescricaoItem;
        CuraVida = item.CuraVida;
    }
}

/// <summary>
/// Tudo o que o Xomi leva de uma cena para outra: inventario, atributos e itens ja coletados.
/// A vida atual fica em VidaJogador. Vale enquanto o jogo esta aberto.
/// </summary>
public static class EstadoXomi
{
    public const int Espacos = 10;

    public static readonly DadosItem[] Itens = new DadosItem[Espacos];
    public static readonly int[] Quantidades = new int[Espacos];

    private static readonly HashSet<string> itensColetados = new HashSet<string>();

    private static bool temAtributos;
    private static int vida;
    private static int forca;
    private static int defesa;
    private static int sorte;

    // Comeca limpo a cada Play (mesmo com "Enter Play Mode Options" ligado).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AoAbrirJogo()
    {
        Limpar();
        SceneManager.sceneLoaded -= AoCarregarCena;
        SceneManager.sceneLoaded += AoCarregarCena;
    }

    /// <summary>Zera o progresso do Xomi (botao Jogar do menu).</summary>
    public static void NovoJogo()
    {
        Limpar();
        VidaJogador.DefinirMaxima(VidaJogador.VidaMaximaPadrao);
        VidaJogador.Definir(VidaJogador.VidaMaximaPadrao);
    }

    private static void Limpar()
    {
        System.Array.Clear(Itens, 0, Itens.Length);
        System.Array.Clear(Quantidades, 0, Quantidades.Length);
        itensColetados.Clear();
        temAtributos = false;
    }

    public static void MarcarColetado(string id)
    {
        if (!string.IsNullOrEmpty(id)) itensColetados.Add(id);
    }

    public static bool FoiColetado(string id)
    {
        return !string.IsNullOrEmpty(id) && itensColetados.Contains(id);
    }

    /// <summary>Na primeira vez guarda os valores do componente; depois aplica os guardados nele.</summary>
    public static void SincronizarAtributos(Atributos a)
    {
        if (!temAtributos)
        {
            GuardarAtributos(a);
            return;
        }

        a.vida = vida;
        a.força = forca;
        a.defesa = defesa;
        a.sorte = sorte;
    }

    public static void GuardarAtributos(Atributos a)
    {
        vida = a.vida;
        forca = a.força;
        defesa = a.defesa;
        sorte = a.sorte;
        temAtributos = true;
    }

    // Roda depois do Awake e antes do Start. Inclui objetos desativados
    // (na ModoBatalha o Xomi fica desativado e o Awake dele nao roda).
    private static void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        foreach (Atributos a in Object.FindObjectsByType<Atributos>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            SincronizarAtributos(a);
        }

        AtributosJogador jogador = Object.FindFirstObjectByType<AtributosJogador>(FindObjectsInactive.Include);

        if (jogador != null && jogador.atributos != null)
        {
            VidaJogador.DefinirMaxima(jogador.VidaMaxima());
        }
    }
}
