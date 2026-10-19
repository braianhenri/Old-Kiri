using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GerenciadorJogo : MonoBehaviour
{
    public static GerenciadorJogo Instancia { get; private set; }
    public Atributos atributos;
    public AtributosJogador atributosJogador;

    private void Awake()
    {
        // Se não houver singleton, tenta GetComponent no mesmo GameObject
        if (atributos == null)
            atributos = GetComponent<Atributos>();

        if (atributos == null)
            Debug.LogWarning("ReferenciaAtributos: não encontrou componente Atributos na cena.");

        // --- AtributosJogador: tenta GetComponent primeiro (mesmo GameObject)
        if (atributosJogador == null)
            atributosJogador = GetComponent<AtributosJogador>();

        if (atributosJogador == null)
            Debug.LogWarning("ReferenciaAtributos: não encontrou componente AtributosJogador na cena.");
    }
}
