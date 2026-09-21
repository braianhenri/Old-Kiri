using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject painelPause;
    public GameObject painelOpcoes; // opcional, se quiser Opções aqui também

    public static bool jogoPausado = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Se as opções estiverem abertas, ESC volta pro pause em vez de despausar
            if (painelOpcoes != null && painelOpcoes.activeSelf)
            {
                FecharOpcoes();
                return;
            }

            if (jogoPausado)
                Continuar();
            else
                Pausar();
        }
    }

    public void Pausar()
    {
        painelPause.SetActive(true);
        Time.timeScale = 0f; // congela o jogo
        jogoPausado = true;
    }

    public void Continuar()
    {
        painelPause.SetActive(false);
        Time.timeScale = 1f; // volta ao normal
        jogoPausado = false;
    }

    public void AbrirOpcoes()
    {
        painelPause.SetActive(false);
        painelOpcoes.SetActive(true);
    }

    public void FecharOpcoes()
    {
        painelOpcoes.SetActive(false);
        painelPause.SetActive(true);
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f; // IMPORTANTE: destravar antes de trocar de cena
        jogoPausado = false;
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void SairDoJogo()
    {
        Application.Quit();
        Debug.Log("Saiu do jogo"); // no Editor o Quit não faz nada, só no build
    }
}