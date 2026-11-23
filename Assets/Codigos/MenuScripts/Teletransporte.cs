
using UnityEngine;

public class Teletransporte : MonoBehaviour
{
    public Transform destino;

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Player"))
        {
            outro.transform.position = destino.position;
        }
    }
}
