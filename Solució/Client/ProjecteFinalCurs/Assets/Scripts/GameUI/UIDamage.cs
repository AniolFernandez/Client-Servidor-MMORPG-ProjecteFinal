using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDamage : MonoBehaviour
{
    private void Start()
    {
        Invoke("DestroyThis", 1f);
    }
    private void Update()//Moviment dels numeros al curar-se / fer mal
    {
        Vector3 v = Camera.main.transform.position - this.transform.position;
        v.x = v.z = 0.0f;
        this.transform.LookAt(Camera.main.transform.position - v);
        this.transform.Rotate(0, 180, 0);
    }

    void DestroyThis()
    {
        Destroy(this.transform.parent.gameObject);
    }
}
