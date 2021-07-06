using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class LocalPlayer : MonoBehaviour
{
    public GameObject arrow;
    private GameObject arrow_instance;
    public GameObject arrowSelector;
    private GameObject arrowSelector_instance;
    public int id_target=0;
    public int target_type=0;
    Camera cam;
    NavMeshAgent agent;
    Vector3 destPosition;
    Animator anim;
    public PlayerController pc;
    private static LocalPlayer _instance;
    public static LocalPlayer Instance
    {
        get
        {
            return _instance;
        }
    }

    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        cam = Camera.main;
        _instance = this;
        anim = this.GetComponent<Animator>();
    }

    void Update()
    {
        if ((agent.remainingDistance < 1 || pc.pData.curr_hp==0) && arrow_instance != null)//Elimina la fletxa del destí
            Destroy(arrow_instance.gameObject);
        if (id_target == 0 && arrowSelector_instance != null)//Elimina la fletxa del target
            Destroy(arrowSelector_instance.gameObject);
        if (Input.GetMouseButtonDown(0)&&pc.pData.curr_hp>0)//Si fem click esquerre i estem vius
        {                
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            //Raycast a on tinguem el ratolí
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.tag == "otherPlayer")//Col·lisió amb un altre jugador
                {
                    //Si clickem un jugador començem a demanar al servidor info d'aquell objecte (pararà quan tanquem la info)
                    SetArrowSelector(hit.collider.gameObject.transform);
                    id_target = hit.collider.gameObject.GetComponent<PlayerController>().pData.player_id;
                    target_type = 0;
                    OtherInfoHUD.instance.id = id_target;
                    ConnectToServer.client.Send(AllNetworkPackets.GetInfoOfPlayer, new GetInfoOfPlayer { playerID = id_target });
                }
                else if (hit.collider.gameObject.tag == "Enemy")//Col·lisió amb un enemic de tipus 1
                {
                    //Si clickem un enemic començem a demanar al servidor info d'aquell objecte (pararà quan tanquem la info)
                    SetArrowSelector(hit.collider.gameObject.transform);
                    OtherInfoHUD.instance.target = hit.collider.gameObject;
                    id_target = hit.collider.gameObject.GetComponent<EnemyController>().enemyID;
                    target_type = 1;
                    ConnectToServer.client.Send(AllNetworkPackets.GetInfoOfNPC, new GetInfoOfNPC { npc_id = id_target, npc_type = target_type });
                }
                else if (hit.collider.gameObject.tag == "goldBag" && Vector3.Distance(hit.transform.position, this.transform.position)<3)//Colisió or
                {
                    ConnectToServer.client.Send(AllNetworkPackets.CatchGold, new CatchGold { id=hit.collider.gameObject.GetComponent<GoldBag>().id });
                }
                else//Moviment del jugador
                {
                    destPosition = hit.point;
                    agent.SetDestination(destPosition);
                    ConnectToServer.client.Send(AllNetworkPackets.MoveTo, new MoveTo { x = destPosition.x, y = destPosition.y, z = destPosition.z });
                    if (arrow_instance != null)
                        Destroy(arrow_instance);
                    arrow_instance = Instantiate(arrow, new Vector3(destPosition.x, destPosition.y + 1, destPosition.z), Quaternion.identity);
                }
            }
        }
        //A l'apretar espai realitzem petició per a realitzar l'atac del nostre personatge
        if (!Chat.chatOpen && Input.GetKeyDown(KeyCode.Space) && pc.pData.curr_hp > 0 && (id_target != 0 || pc.pData.class_id == 1))
        {
            ConnectToServer.client.Send(AllNetworkPackets.Atack, new Atack { npc_id = id_target, npc_type = target_type });
        }
        //A l'apretar X realitzem petició per a realitzar un dab
        else if(!Chat.chatOpen && Input.GetKeyDown(KeyCode.X) && pc.pData.curr_hp > 0)
        {
            ConnectToServer.client.Send(AllNetworkPackets.Dab, new Dab { playerID = pc.pData.player_id });
        }
    }

    void SetArrowSelector(Transform t)//Fem que la fletxa de selecció segueixi el target
    {
        if (arrowSelector_instance != null)
            Destroy(arrowSelector_instance);
        arrowSelector_instance = Instantiate(arrowSelector, new Vector3(t.position.x, t.position.y + 8f, t.position.z+2f), Quaternion.identity);
        arrowSelector_instance.GetComponent<ArrowSelector>().toFollow = t;
    }

    public void DoNotMove()//Fem que el jugador no es mogui
    {
        destPosition = transform.position;
        agent.SetDestination(destPosition);
        ConnectToServer.client.Send(AllNetworkPackets.MoveTo, new MoveTo { x = destPosition.x, y = destPosition.y, z = destPosition.z });
        if (arrow_instance != null)
            Destroy(arrow_instance);
    }

}
