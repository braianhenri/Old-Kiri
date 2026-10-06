using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtributosJogador : MonoBehaviour
{
    public Atributos atributos;
   public int VidaMaxima()
    {
        return 100 + atributos.vida * 5;
    }
    public int ataque() 
    {
        return 10 + atributos.força * 10;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
