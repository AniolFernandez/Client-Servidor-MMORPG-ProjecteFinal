using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyMe : MonoBehaviour
{
    public float timeToDestroy = 3f;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("DestroyThis", timeToDestroy);
    }

    void DestroyThis()
    {
        Destroy(this.gameObject);
    }
}
