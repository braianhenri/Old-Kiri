using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Informações do item")]
    [SerializeField]
    private Sprite spriteItem;

    [TextArea(3, 6)]
    [SerializeField]
    private string descricaoItem;

    public Sprite SpriteItem
    {
        get { return spriteItem; }
    }

    public string DescricaoItem
    {
        get { return descricaoItem; }
    }

    private IventarioJogador inventarioJogador;
    private bool jogadorPerto;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!jogadorPerto)
            {
                return;
            }

            if (inventarioJogador != null &&
                inventarioJogador.AdicionarItem(this))
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D outroObjeto)
    {
        if (!outroObjeto.CompareTag("Player"))
        {
            return;
        }

        inventarioJogador =
            outroObjeto.GetComponentInParent<IventarioJogador>();

        if (inventarioJogador != null)
        {
            jogadorPerto = true;
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
