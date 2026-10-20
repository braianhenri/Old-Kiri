using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BarraHP : MonoBehaviour
{
    public AtributosJogador AtJogador;
    [SerializeField] GameObject vida;
    [SerializeField] TMP_Text TxtVida;
    private int vidaMax;
    private int vidaAtual;
    // Start is called before the first frame update
    void Start()
    {
        vidaMax = AtJogador.VidaMaxima();
        vidaAtual = vidaMax;
        updateHUD();
        vida.transform.localScale = new Vector3(0.5f, 1f);
    }

    public void updateHUD()
    {
        float vidaPercent = (float)vidaAtual / vidaMax;
        vida.transform.localScale = new Vector3(vidaPercent, 1f);
        TxtVida.text = vidaAtual + "/" + vidaMax;
    }
    public void ReceberDano()
    {
        vidaAtual -= 20;
        if (vidaAtual < 0)
        {
            vidaAtual = 0;
        }
        updateHUD();
    }
     public void OnButtonClick(){
        vidaAtual -= 20;
        if (vidaAtual < 0)
        {
            vidaAtual = 0;
        }
        updateHUD();
    }
}
