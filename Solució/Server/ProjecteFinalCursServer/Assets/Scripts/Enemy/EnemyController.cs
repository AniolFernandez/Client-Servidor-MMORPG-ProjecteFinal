using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;
#pragma warning disable CS0618
public class EnemyController : MonoBehaviour
{
    public NPCInstance thisNPC;

    NavMeshAgent agent;
    public EnemyNearbyUnits nearPlayers;
    private Vector3 startPosition;
    public Transform target;
    public Vector3 lastDir;
    public GameObject basicAtack;
    public GameObject goldBag;
    bool canAtack = true;
    public Light iaLight;
    private short moves = 0;
    private bool goingBase = false;

    private int lastHP=0;
    private int lastMANA = 0;
    float lastPkt = 0;
    public List<Hitter> hitters = new List<Hitter>();

    void SendInfoToListeners()//Envia actualització a qui escolti la info
    {
        bool hasChanges = CheckIfDefUpdate();
        if (hasChanges)
        {
            UpdateNPCDef def = new UpdateNPCDef();
            def.npc_id = thisNPC.id;
            def.curr_hp = thisNPC.curr_hp;
            def.curr_mana = thisNPC.curr_mana;
            def.name = thisNPC.name;
            def.type = thisNPC.type;
            def.lvl = thisNPC.level;
            def.npc_id = thisNPC.id;
            def.max_hp = thisNPC.max_hp;
            def.max_mana = thisNPC.max_mana;
            foreach (NetworkConnection nc in thisNPC.infoListeners)
            {
                nc.Send(AllNetworkPackets.UpdateNPCDef, def);
            }
        }
    }
    /// <summary>
    /// Comprova si hi ha canvis en la vida o mana per a actualitzar la descripció de qui la demani
    /// </summary>
    bool CheckIfDefUpdate()
    {
        bool changes = false;
        if (lastHP != thisNPC.curr_hp)
        {
            lastHP = thisNPC.curr_hp;
            changes = true;
        }
        if (lastMANA != thisNPC.curr_mana)
        {
            lastMANA = thisNPC.curr_mana;
            changes = true;
        }
        return changes;
    }

    void Start()
    {
        startPosition = this.transform.position;
        lastDir = startPosition;
        agent = this.GetComponent<NavMeshAgent>();
        InvokeRepeating("IAMove", 0, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (canAtack)
        {
            CheckChanges();
            tryAtack();
        }
        SendInfoToListeners();
    }

    void IAMove()//Moviment autonom de l'enemic
    {
        if (target == null)
        {
            if (moves < 5 && !goingBase)
            {
                moves++;
                Vector3 target = RandomSpotLightCirclePoint();
                //Debug.DrawLine(target, target + Vector3.one * 0.05f, Color.red, 0.5f);
                Vector3 origen = new Vector3(this.transform.position.x, this.transform.position.y + 5, this.transform.position.z);
                Ray ray = new Ray(origen, target - origen);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo, 100))
                {
                    lastDir = hitInfo.point;
                    //Debug.DrawLine(ray.origin, hitInfo.point, Color.green);
                }
            }
            else
            {
                moves = 0;
                goingBase = true;
                lastDir = startPosition;  
            }
            if(agent.isOnNavMesh)
            agent.SetDestination(lastDir);
            UpdateDestination();
            if (goingBase && Vector3.Distance(this.transform.position, startPosition) < 3)
            {
                goingBase = false;
            }
        }  
    }
    
    //Calcula un punt aleatori dins del rang marcat per la llum de l'enemic. Ens permet fer un moviment aleatori de l'NPC
    Vector3 RandomSpotLightCirclePoint()
    {
        float radius = Mathf.Tan(Mathf.Deg2Rad * iaLight.spotAngle / 2) * iaLight.range;
        Vector2 circle = Random.insideUnitCircle * radius;
        Vector3 target = iaLight.transform.position + iaLight.transform.forward * iaLight.range + iaLight.transform.rotation * new Vector3(circle.x, circle.y);
        return target;
    }

    public void SetTarget(Transform t)//Actualitza el nou target de l'enimc (null = torna a base)
    {
        target = t;
        if (t != null)
        {
            agent.SetDestination(t.position);
            lastDir = t.position;
        }
        else
        {
            goingBase = true;
            agent.SetDestination(startPosition);
            lastDir = startPosition;
        }
        UpdateDestination();
    }

    private void CheckChanges()//Si el jugador de destí s'ha mogut canviem el desti del nostre jugador ja que ens ha de seguir
    {
        if (target != null && lastDir != target.transform.position)
        {
            lastDir = target.position;
            agent.SetDestination(target.position);//Canvia el desti ja que el jugador pot estar moguent-se
            UpdateDestination();
        }
    }
    //Funciona ok
    #region Atack
    private void tryAtack()//Intenta realitzar l'atac
    {
        if (target != null && Vector3.Distance(this.transform.position, target.transform.position) < 3f)
        {
            Invoke("atackCollider", 0.2f);
            agent.SetDestination(this.transform.position);
            canAtack = false;
            foreach (NetworkConnection nc in nearPlayers.nearbyUser)
            {
                if(nc.lastError!=NetworkError.Timeout)
                    nc.Send(AllNetworkPackets.AtackNPC, new AtackNPC { npc_id = thisNPC.id, npc_type = thisNPC.type });
            }
            Invoke("moveAgain", 0.45983f);
        }
    }

    private void atackCollider()//Activa l'aplicador de dany
    {
        basicAtack.SetActive(true);
    }

    private void moveAgain()//permet tornar a moure
    {
        basicAtack.SetActive(false);
        canAtack = true;
    }
    #endregion

    private void UpdateDestination()//Actualitza la posició de destí
    {
        if (Time.time - lastPkt > 0.03f)
        {
            lastPkt = Time.time;
            List<NetworkConnection> toRm = new List<NetworkConnection>();
            foreach (NetworkConnection nc in nearPlayers.nearbyUser)
            {
                try
                {
                    if (nc.lastError != NetworkError.Timeout)
                        nc.Send(AllNetworkPackets.MoveNPC, new MoveNPC { npc_id = thisNPC.id, npc_type = thisNPC.type, x = lastDir.x, y = lastDir.y, z = lastDir.z });

                }
                catch { toRm.Add(nc); }
            }
            foreach (NetworkConnection nc in toRm)
            {
                try
                {
                    nearPlayers.nearbyUser.Remove(nc);

                }
                catch { }
            }
        }
        
    }

    public void DoDamage(int damge, PlayerController hitter)//Realitzem mal a l'enemic
    {
        try
        {
            if (thisNPC.curr_hp - damge > 0)
            {
                thisNPC.curr_hp -= damge;
                //Afegeix l'atacant a la llista de gent que l'ha picat, repartin la xp per la vida que li ha tret
                Hitter hit = hitters.Find(x => x.hitter.Player.id == hitter.Player.id);
                if (hit == null)
                {
                    hit = new Hitter();
                    hit.damageDealt = 0;
                    hit.damageDealt += damge;
                    hit.hitter = hitter;
                    hitters.Add(hit);
                }
                else
                {
                    hit.damageDealt += damge;
                }
                if (Vector3.Distance(this.transform.position, target.transform.position) > Vector3.Distance(this.transform.position, hitter.transform.position))
                    SetTarget(hitter.transform);//Qui hagi realitzat l'ultim cop si està més aprop que el target actual sera el nou target
            }
            else
            {
                //Experiencia de l'ultim cop
                Hitter hit = hitters.Find(x => x.hitter.Player.id == hitter.Player.id);
                if (hit == null)
                {
                    hit = new Hitter();
                    hit.damageDealt = 0;
                    hit.damageDealt += thisNPC.curr_hp;
                    hit.hitter = hitter;
                    hitters.Add(hit);
                }
                else
                {
                    hit.damageDealt += thisNPC.curr_hp;
                }
                //Spawn d'items just abans de morir
                CreateGold(hitter);
                //
                KillNPC knpc = new KillNPC();
                thisNPC.curr_hp = 0;
                knpc.npc_id = thisNPC.id;
                knpc.npc_type = thisNPC.type;
                Invoke("Respawn", 30f);
                agent.isStopped = true;
                thisNPC.infoListeners = new List<NetworkConnection>();
                //Informem als jugador que l'npc ha mort
                foreach (NetworkConnection nc in nearPlayers.nearbyUser)
                {
                    nc.Send(AllNetworkPackets.KillNPC, new KillNPC { npc_id = thisNPC.id, npc_type = thisNPC.type });
                }
                nearPlayers.nearbyUser = new List<NetworkConnection>();
                this.gameObject.SetActive(false);
                //Enviem xp als que l'han matat
                SendXp();


            }
        }
        catch { }
           
    }

    void Respawn()//Fa reapareixre l'NPC
    {
        target = null;
        goingBase = false;
        moves = 0;
        lastDir = startPosition;
        this.gameObject.transform.position = startPosition;
        this.thisNPC.curr_hp = thisNPC.max_hp;
        this.thisNPC.curr_mana = thisNPC.max_mana;
        this.gameObject.SetActive(true);
    }

    void SendXp()//Envia experiencia a les persones que l'han picat. Repartint equititivament depenen del mal que l'hi hagin realitzat
    {
        int xpTotal = 50;
        foreach(Hitter hit in hitters)
        {
            hit.hitter.AddXP((hit.damageDealt / thisNPC.max_hp) * xpTotal);    
        }
    }

    //Crea aleatoriament una bossa d'or
    void CreateGold(PlayerController pcAllowed)
    {
        if(Random.Range(0, 100) < 40)//40% de dropejar or
        {

            GameObject gbag = Instantiate(goldBag, transform.position, Quaternion.identity);
            GoldBag g = gbag.GetComponent<GoldBag>();
            g.SetBagOfGold(Random.Range(333, 777), Time.time, pcAllowed, nearPlayers.nearbyUser);
        }        
    }

    void ForceUpdatePosition()//Actualitza la posició real
    {
        UpdateCurrentPosNPC cp = new UpdateCurrentPosNPC { npc_id = thisNPC.id, npc_type = thisNPC.type, x = transform.position.x, y = transform.position.y, z = transform.position.z, yr = this.transform.eulerAngles.y };
        foreach (NetworkConnection nc in nearPlayers.nearbyUser)
        {
            nc.Send(AllNetworkPackets.UpdateCurrentPosNPC, cp);
        }
    }
}
