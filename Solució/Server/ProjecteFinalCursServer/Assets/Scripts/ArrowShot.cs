using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0618
public class ArrowShot : MonoBehaviour
{
    public float speed = 45f;
    private Transform target;
    private int dmg;
    private PlayerController shooter;
    // Update is called once per frame
    void Update()
    {
        if (target != null)//Fem que la vala vagi cap a l'enemic
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(target.transform.position.x, target.transform.position.y + 3, target.transform.position.z), speed * Time.deltaTime);
            this.transform.LookAt(target);
        }

        if (!target.gameObject.active)//Si maten a l'enemic abans que la bala impacti, la destruim
        {
            Destroy(this.gameObject);
        }
    }

    public void SetBullet(int damage, Transform Target, PlayerController shooter)//Inicialitza les propietats de la bala
    {
        this.dmg = damage;
        this.target = Target;
        this.shooter = shooter;
    }

    private void OnTriggerEnter(Collider other)//Impacta amb l'enemic
    {
        if (other.gameObject == target.gameObject)
        {
            target.GetComponent<EnemyController>().DoDamage(dmg, shooter);
            Destroy(this.gameObject);
        }
    }
}
