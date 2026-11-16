using System.Collections;
using UnityEngine;

public class InteracaoCasa : MonoBehaviour
{
    [Header("Interface")]
    public GameObject avisoE;
    public FadeTransicao fade;
    public GameObject setaGuia;

    [Header("Comportamento")]
    public bool exigirTeclaE = true;   // desmarque para o fade começar ao chegar

    [Header("Depois da interacao")]
    public GameObject npc;             // deixe DESLIGADO na cena
    public Transform posicaoDoNpc;     // opcional: onde o NPC aparece

    bool perto, usou;

    void Start()
    {
        if (avisoE) avisoE.SetActive(false);
        if (npc) npc.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D o)
    {
        if (usou || !o.CompareTag("Player")) return;

        if (exigirTeclaE)
        {
            perto = true;
            if (avisoE) avisoE.SetActive(true);
        }
        else StartCoroutine(Sequencia());
    }

    void OnTriggerExit2D(Collider2D o)
    {
        if (!o.CompareTag("Player")) return;
        perto = false;
        if (avisoE) avisoE.SetActive(false);
    }

    void Update()
    {
        if (exigirTeclaE && perto && !usou && ApertouE())
            StartCoroutine(Sequencia());
    }

    bool ApertouE()
    {
#if ENABLE_INPUT_SYSTEM
        return UnityEngine.InputSystem.Keyboard.current != null
            && UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    IEnumerator Sequencia()
    {
        usou = true; perto = false;
        if (avisoE) avisoE.SetActive(false);
        if (setaGuia) setaGuia.SetActive(false);

        yield return fade.Escurecer(1f);
        if (npc)
        {
            if (posicaoDoNpc) npc.transform.position = posicaoDoNpc.position;
            npc.SetActive(true);
        }
        yield return new WaitForSecondsRealtime(0.5f);
        yield return fade.Clarear(1f);
    }
}