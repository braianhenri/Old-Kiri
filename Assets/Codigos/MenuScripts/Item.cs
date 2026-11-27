using UnityEngine;

// Os numeros sao salvos nos prefabs: novas opcoes devem entrar no final.
public enum CaracteristicaItem
{
    [InspectorName("Nenhuma (não pode ser usado)")] Nenhuma = 0,
    [InspectorName("Recuperar vida (cura)")] RecuperarVida = 1,
    [InspectorName("Atributo Vida")] Vida = 2,
    [InspectorName("Atributo Força")] Forca = 3,
    [InspectorName("Atributo Defesa")] Defesa = 4,
    [InspectorName("Atributo Sorte")] Sorte = 5
}

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
    [Tooltip("O que este item muda no Xomi quando é usado")]
    [SerializeField]
    private CaracteristicaItem caracteristica = CaracteristicaItem.Nenhuma;

    [Tooltip("Quanto o item adiciona na característica escolhida")]
    [SerializeField]
    private int valor;

    public CaracteristicaItem Caracteristica
    {
        get { return caracteristica; }
    }

    public int Valor
    {
        get { return valor; }
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
