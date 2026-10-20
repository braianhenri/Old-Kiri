using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GerenciadorBatalha : MonoBehaviour
{
    public static GerenciadorBatalha instance;
    public bool batalhaAtiva;
    [SerializeField] private GameObject HudBatalha;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        batalhaAtiva = false;
        if ( HudBatalha != null)
            HudBatalha.SetActive(false);
    }
    public void AtivarBatalha()
    {
        batalhaAtiva = true;
        if (HudBatalha != null)
            HudBatalha.SetActive(true);
        Debug.Log("Batalha Ativada");

    }
    public void FimBatalha()
    {
        batalhaAtiva = false;
        if (HudBatalha != null)
            HudBatalha.SetActive(false);
        Debug.Log("Batalha Finalizada");
    }
}
