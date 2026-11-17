
using UnityEngine;

public class GerenciadorBatalha : MonoBehaviour
{
    public GameObject painelAcoes;
    public GameObject painelAtacar;
    public GameObject painelConversar;
    public GameObject painelItem;

    public void AbrirAtacar()
    {
        painelAcoes.SetActive(false);
        painelAtacar.SetActive(true);
    }

    public void AbrirConversar()
    {
        painelAcoes.SetActive(false);
        painelConversar.SetActive(true);
    }

    public void AbrirItem()
    {
        painelAcoes.SetActive(false);
        painelItem.SetActive(true);
    }

    public void Fugir()
    {
        Debug.Log("Tentando fugir da batalha...");
    }

    public void VoltarMenu()
    {
        painelAtacar.SetActive(false);
        painelConversar.SetActive(false);
        painelItem.SetActive(false);

        painelAcoes.SetActive(true);
    }
}
