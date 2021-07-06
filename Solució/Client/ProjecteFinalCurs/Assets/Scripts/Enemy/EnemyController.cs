using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private NavMeshAgent agent;
    public int enemyID;
    public TextMeshPro enemyName;

    public MoveNPC moveTo;
    public Animator anim;

    void Start()
    {
        anim = this.GetComponent<Animator>();
        agent = this.GetComponent<NavMeshAgent>();
        anim.Play("NormalAnim");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        RotateEnemyNameToCamera();
        RemoteClientUpdate();
    }

    /// <summary>
    /// Gira el nom de l'enemic per a que sempre estigui mirant cap a camera
    /// </summary>
    private void RotateEnemyNameToCamera()
    {
        Vector3 v = Camera.main.transform.position - enemyName.transform.position;
        v.x = v.z = 0.0f;
        enemyName.transform.LookAt(Camera.main.transform.position - v);
        enemyName.transform.Rotate(0, 180, 0);
    }

    /// <summary>
    /// Actualització del destió de l'enemic.
    /// </summary>
    public void RemoteClientUpdate()
    {
        if (moveTo != null)
        {
            agent.SetDestination(new Vector3(moveTo.x, moveTo.y,moveTo.z));
            moveTo = null;
        }
    }

    /// <summary>
    /// Cridat pel gestor de connexió quan l'enemic ha d'atacar
    /// </summary>
    public void DoAtack()
    {
        if (this.gameObject != null)
        {
            agent.SetDestination(this.transform.position);
            anim.Play("EnemyAnim");
        }
    }

    /// <summary>
    /// Quan es rep el paquet de que la unitat ha mort, l'animem per a que aixi ho fagi
    /// </summary>
    public void KillThis()
    {
        if (this.gameObject != null)
        {
            agent.SetDestination(this.transform.position);
            agent.isStopped = true;
            this.GetComponent<CapsuleCollider>().enabled = false;
            anim.Play("DieAnim");
            Invoke("DestroyNPC", 3f);
            if(LocalPlayer.Instance.id_target == enemyID)
            {
                LocalPlayer.Instance.id_target = 0;
                OtherInfoHUD.instance.gameObject.SetActive(false);
            }
            this.gameObject.tag = "dead";    
        }
    }

    void DestroyNPC()
    {
        Destroy(this.gameObject);
    }
}
