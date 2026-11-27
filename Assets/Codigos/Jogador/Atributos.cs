using System.Collections;
using UnityEngine;

public class Atributos : MonoBehaviour
{
    public int vida;
    public int força;
    public int defesa;
    public int sorte;

    private void Awake()
    {
        EstadoXomi.SincronizarAtributos(this);
    }

    private void OnDisable()
    {
        EstadoXomi.GuardarAtributos(this);
    }

    /// <summary>Soma o valor no atributo escolhido, guarda para as outras cenas e devolve o novo total.</summary>
    public int Somar(CaracteristicaItem caracteristica, int quantidade)
    {
        int total;

        switch (caracteristica)
        {
            case CaracteristicaItem.Vida: total = vida += quantidade; break;
            case CaracteristicaItem.Forca: total = força += quantidade; break;
            case CaracteristicaItem.Defesa: total = defesa += quantidade; break;
            case CaracteristicaItem.Sorte: total = sorte += quantidade; break;
            default: return 0;
        }

        EstadoXomi.GuardarAtributos(this);

        // A vida maxima depende do atributo Vida.
        AtributosJogador jogador = GetComponent<AtributosJogador>();

        if (jogador != null)
        {
            VidaJogador.DefinirMaxima(jogador.VidaMaxima());
        }

        return total;
    }

    public void AumentarVida()
    {         
        vida++;
        EstadoXomi.GuardarAtributos(this);
    }

    public void AumentarForça()
    {
        força++;
        EstadoXomi.GuardarAtributos(this);
    }

    public void AumentarDefesa()
    {
        defesa++;
        EstadoXomi.GuardarAtributos(this);
    }

    public void AumentarSorte()
    {
        sorte++;
        EstadoXomi.GuardarAtributos(this);
    }
}
