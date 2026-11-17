using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtributosJogador : MonoBehaviour
{
    public Atributos atributos;

    private int vidaMaximaCache;
    private int ataqueCache;
    private int ataqueCriticoCache;

    void Start()
    {
        AtualizarCache();
    }

    private void AtualizarCache()
    {
        vidaMaximaCache = 100 + atributos.vida * 5;
        ataqueCache = 10 + atributos.força * 2;
        ataqueCriticoCache = atributos.força * 10;
    }

    public int VidaMaxima()
    {
        return vidaMaximaCache;
    }

    public int Ataque()
    {
        return ataqueCache;
    }

    public int AtaqueCritico()
    {
        return ataqueCriticoCache;
    }
}
