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

    [Header("Efeito ao usar")]
    [Tooltip("Quanto de vida o item recupera quando e usado (0 = nao cura)")]
    [SerializeField]
    private int curaVida;

    public int CuraVida
    {
        get { return curaVida; }
    }

    private IventarioJogador inventarioJogador;
    private bool jogadorPerto;
    private string idNaCena;

    private void Awake()
    {
        // Item que ja foi pego nao reaparece quando o Xomi volta para a cena.
        idNaCena = gameObject.scene.name + "/" + name + "@" + transform.position.ToString("F2");

        if (EstadoXomi.FoiColetado(idNaCena))
        {
            gameObject.SetActive(false);
        }
    }

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
                EstadoXomi.MarcarColetado(idNaCena);
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
