using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarraHP : MonoBehaviour
{
    [SerializeField] GameObject vida;
    // Start is called before the first frame update
    void Start()
    {
        vida.transform.localScale = new Vector3(0.5f, 1f);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void setHp(float HPnormalized)
    {

    }
}
