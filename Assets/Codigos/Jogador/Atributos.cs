using System.Collections;
using UnityEngine;

public class Atributos : MonoBehaviour
{
    public int vida;
    public int força;
    public int defesa;
    public int sorte;

    public void AumentarVida()
    {         
        vida++;
    }

    public void AumentarForça()
    {
        força++;
    }

    public void AumentarDefesa()
    {
        defesa++;
    }

    public void AumentarSorte()
    {
        sorte++;
    }
}
