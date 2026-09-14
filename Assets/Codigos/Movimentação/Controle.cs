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
    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }
    // Update is called once per frame
    void Update()
    {
    }
    private void FixedUpdate()
    {
        Vector2 direcao = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        rb.velocity = direcao.normalized * velocidade;
        if (direcao != Vector2.zero)
        {
            anim.SetBool("Andando", true);
        }else
        {
            anim.SetBool("Andando", false);
        }
    }
}
