using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;

public class NewBehaviourScript : MonoBehaviour
{
    ///pra modificar direto no unity ao inves do codigo
    [SerializeField]
    //velocidade padrao
    private float velocidade = 2f;
    //componente rigidbody para colisões
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator anim;
    private Vector2 direcao;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }
    private void FixedUpdate()
    {
        direcao = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        rb.velocity = direcao.normalized * velocidade;
        if (direcao != Vector2.zero)
        {
            anim.SetBool("Andando", true);
        }
        else
        {
            anim.SetBool("Andando", false);
            anim.SetBool("Lado", false);
        }
        D_E();
        C_B();

    }
    void D_E()
    {
        if (direcao.x > 0)
        {
            transform.eulerAngles = Vector3.zero;
            anim.SetBool("Lado", true);
        }
        else if (direcao.x < 0)
        {
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
            anim.SetBool("Lado", true);
        }
    }
    void C_B()
    {
        if (direcao.y > 0)
        {
            anim.SetBool("Cima", true);
            anim.SetBool("Baixo", false);
        }
        else if (direcao.y < 0)
        {
            anim.SetBool("Baixo", true);
            anim.SetBool("Cima", false);
        }
    }

}

