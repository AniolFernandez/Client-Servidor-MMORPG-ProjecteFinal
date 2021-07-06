using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    public GameObject magicianAtack;
    public GameObject swordsmanAtack;
    public GameObject archerAtack;
    private Transform target;
    public GameObject chest;
    public NavMeshAgent agent;
    public PlayerData pData = new PlayerData();
    public TextMeshPro playerName;
    public Animator anim;
    Vector3 destPosition;
    bool runing = false;
    public MoveTo moveTo;

    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        anim = this.GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        Animate();
        RotatePlayerNameToCamera();
        RemoteClientUpdate();
    }

    private void Animate()
    {
        if (agent.remainingDistance > 1f && pData.curr_hp>0 && !agent.isStopped)//El jugador s'esta moguent
        {
            anim.Play("Player_move");
            runing = true;
        }
        else if (agent.remainingDistance < 1f && runing)//El jugador esta quiet
        {
            anim.Play("Player_idle");
            runing = false;
        }
    }

    private void RotatePlayerNameToCamera()//Rotem el nom del jugador per a mirar sempre a camara
    {
        Vector3 v = Camera.main.transform.position - playerName.transform.position;
        v.x = v.z = 0.0f;
        playerName.transform.LookAt(Camera.main.transform.position - v);
        playerName.transform.Rotate(0, 180, 0);
    }

    public void RemoteClientUpdate()//Actualització de la posició de destí del jugador
    {
        if (moveTo != null && pData.curr_hp>0)
        {
            destPosition = new Vector3(moveTo.x, moveTo.y, moveTo.z);
            agent.SetDestination(destPosition);
            moveTo = null;
        }
    }

    #region MagicianAtack
    public void MagicianAtack()//realitza l'atac de mag
    {
        agent.isStopped = true;
        moveTo = null;
        anim.Play("Player_magicianAtack");
        Invoke("magicianAtackAnim", 0.1f);
        Invoke("moveAgain", 1.3f);
    }

    private void magicianAtackAnim()
    {
        Instantiate(magicianAtack, new Vector3(this.transform.position.x, this.transform.position.y + 2f, this.transform.position.z), this.transform.rotation);
    }
    #endregion

    #region SwordsmanAtack
    public void SwordsmanAtack()//Realitza l'atac d'espadachin
    {
        agent.isStopped = true;
        moveTo = null;
        anim.Play("Player_swordAtack");
        Invoke("swordmanAtackAnim", 0.4f);
        Invoke("moveAgain", 0.85f);
    }

    private void swordmanAtackAnim()
    {
        swordsmanAtack.SetActive(true);
        Invoke("swordmanAtackOff", 0.5f);
    }

    private void swordmanAtackOff()
    {
        swordsmanAtack.SetActive(false);
    }
    #endregion

    #region ArcherAtack
    public void ArcherAtack(Transform target)//Realitza l'atac d'arquer
    {
        this.target = target;
        this.transform.LookAt(target);
        agent.isStopped = true;
        moveTo = null;
        anim.Play("Player_bowAnim");
        Invoke("archerAtackAnim", 0.25f);
        Invoke("moveAgain", 0.35f);
        
    }

    private void archerAtackAnim()
    {
        Instantiate(archerAtack, new Vector3(this.transform.position.x, this.transform.position.y + 4, this.transform.position.z), Quaternion.identity).GetComponent<ArrowShot>().target = target;
    }
    #endregion

    public void Dab()//Fa que el jugador realitzi un dab
    {
        agent.isStopped = true;
        moveTo = null;
        anim.Play("Player_dab");
        Invoke("moveAgain", 0.5f);
    }

    private void moveAgain()//Permet tornar a moure el jugador
    {
        if (pData.curr_hp>0)
            agent.isStopped = false;
    }

    public void Stop()//Para el jugador
    {
        runing = false;
        agent.isStopped = true;
        moveTo = null;
    }
}
