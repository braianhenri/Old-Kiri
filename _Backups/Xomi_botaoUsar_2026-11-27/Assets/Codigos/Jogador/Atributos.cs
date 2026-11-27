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
