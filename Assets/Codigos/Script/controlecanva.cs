using UnityEngine;

public class ControleCanvas : MonoBehaviour
{
    [SerializeField]
    private GameObject texto;

    private void Awake()
    {
        if (texto == null)
        {
            Debug.LogError(
                "O objeto de texto não foi configurado no Inspector.",
                this);

            return;
        }

        texto.SetActive(false);
    }

    public void MostrarTexto()
    {
        if (texto != null)
        {
            texto.SetActive(true);
        }
    }

    public void OcultarTexto()
    {
        if (texto != null)
        {
            texto.SetActive(false);
        }
    }

    public void AlternarTexto()
    {
        if (texto != null)
        {
            texto.SetActive(!texto.activeSelf);
        }
    }
}