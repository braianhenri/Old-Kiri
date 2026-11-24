using UnityEngine;
using UnityEngine.SceneManagement;

// Coloque num objeto com Collider2D (Is Trigger). Quando o Player encosta, carrega a cena indicada.
public class TrocaDeCena : MonoBehaviour
{
    public string nomeDaCena = "cena03";
    bool trocando;

    void OnTriggerEnter2D(Collider2D outro)
    {
        if (trocando || !outro.CompareTag("Player")) return;
        trocando = true;
        SceneManager.LoadScene(nomeDaCena);
    }
}
