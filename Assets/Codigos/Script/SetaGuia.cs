using UnityEngine;

public class SetaGuia : MonoBehaviour
{
    public Transform alvo;                
    public Transform jogador;             
    public float distanciaDoJogador = 1.2f;  
    public float distanciaParaSumir = 2f;   
    public float anguloExtra = 0f;           

    SpriteRenderer sr;
    void Awake() { sr = GetComponent<SpriteRenderer>(); }

    void LateUpdate()
    {
        if (alvo == null || jogador == null) return;

        Vector2 dir = alvo.position - jogador.position;
        float dist = dir.magnitude;

        if (sr != null) sr.enabled = dist > distanciaParaSumir;
        if (dist <= distanciaParaSumir) return;

        dir.Normalize();
        Vector2 pos = (Vector2)jogador.position + dir * distanciaDoJogador;
        transform.position = new Vector3(pos.x, pos.y, transform.position.z);

        float angulo = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angulo + anguloExtra);
    }
}