using UnityEngine;

public class Item : MonoBehaviour
{
    private IventarioJogador inventarioJogador;
    private bool jogadorPerto;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("A tecla E foi detectada.");

            if (!jogadorPerto)
            {
                Debug.Log("O jogador ainda não está perto do item.");
                return;
            }

            if (inventarioJogador.AdicionarItem(this))
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D outroObjeto)
    {
        Debug.Log("Objeto entrou no item: " + outroObjeto.name);

        if (!outroObjeto.CompareTag("Player"))
        {
            return;
        }

        inventarioJogador =
            outroObjeto.GetComponentInParent<IventarioJogador>();

        if (inventarioJogador != null)
        {
            jogadorPerto = true;
            Debug.Log("Jogador perto. Pressione E.");
        }
    }

    private void OnTriggerExit2D(Collider2D outroObjeto)
    {
        if (outroObjeto.CompareTag("Player"))
        {
            jogadorPerto = false;
            inventarioJogador = null;
        }
    }
}
