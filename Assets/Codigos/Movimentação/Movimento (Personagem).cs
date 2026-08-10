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
    private Vector2 direção;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        direção = new Vector2 (horizontal, vertical);
    }
    private void FixedUpdate()
    {
        Vector3 movePosition = (velocidade * Time.fixedDeltaTime * direção.normalized) + rb.position;   
    }
}
