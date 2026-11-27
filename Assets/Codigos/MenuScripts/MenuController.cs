using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void Jogar()
    {
        EstadoXomi.NovoJogo(); // jogo novo: inventario e atributos zerados
        SceneManager.LoadScene("cena teste");
    }
}