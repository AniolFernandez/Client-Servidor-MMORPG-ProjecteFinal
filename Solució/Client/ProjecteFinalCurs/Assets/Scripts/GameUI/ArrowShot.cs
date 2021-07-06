using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowShot : MonoBehaviour
{
    public float speed = 45f;
    public Transform target;


    void Update()
    {
        if (target == null || target.tag == "dead")//Sino, eliminem la fletxa
        {
            Destroy(this.gameObject);
        }
        else if (target != null)//Si la fletxa te target fem que es vagi moguent cap a ell
        {
            Vector3 trgt = new Vector3(target.transform.position.x, target.transform.position.y + 3, target.transform.position.z);
            this.transform.position = Vector3.MoveTowards(this.transform.position, trgt, speed * Time.deltaTime);
            this.transform.LookAt(trgt);
        }
        
    }

    private void OnCollisionEnter(Collision collision)//Si la fletxa colisiona amb l'enemic la destruïm
    {
        if (collision.gameObject == target.gameObject)
        {
            Destroy(this.gameObject);
        }
    }
}
