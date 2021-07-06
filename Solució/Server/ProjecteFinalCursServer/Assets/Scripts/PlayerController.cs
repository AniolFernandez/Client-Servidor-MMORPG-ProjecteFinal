using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

#pragma warning disable CS0618
public class PlayerController : MonoBehaviour
{
    public PlayerStructure Player = new PlayerStructure();
    public GameObject swordsmanAtack;
    public GameObject archerAtack;
    private Transform target;
    public NavMeshAgent agent;

    public MoveTo pkt = null;
    public NetworkConnection clientConnection;

    public Vector3 destPosition= new Vector3(0, 0, 0);
    public List<PlayerController> nearbyUser = new List<PlayerController>();
    public List<int> notAddeds = new List<int>();
    public List<NetworkConnection> infoListeners = new List<NetworkConnection>();
    public List<EnemyNearbyUnits> nearbyEnemy = new List<EnemyNearbyUnits>();//
    public List<ChangeStateNeeded> csn = new List<ChangeStateNeeded>();
    private int lastHP=0;
    private int lastMANA=0;
    private int lastLV=0;
    private int lastXP=0;
    private float lastHit=0;
    private const float MagicianCD = 5f;
    private float lastAtack = 0;

    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        InvokeRepeating("SavePlayerData", 30, 30);
        InvokeRepeating("HealPlayer", 3, 3);
        InvokeRepeating("ForceUpdatePosition", 5, 2);
        InvokeRepeating("ResendConnection", 0, 1);
    }

    void Update()
    {
        ServerUpdate();
        SendInfoToListeners();
    }

    void ResendConnection()
    {
        foreach(int i in notAddeds)
        {
            PlayerController pc = nearbyUser.Find(x => x.Player.id == i);
            if(pc!=null)
            {
                clientConnection.Send(AllNetworkPackets.PlayerConnected, new PlayerConnected { playerId = pc.Player.id, playername = pc.Player.playername, x = pc.gameObject.transform.position.x, y = pc.gameObject.transform.position.y, z = pc.gameObject.transform.position.z, classid = pc.Player.class_id });
                if (pc.destPosition != new Vector3(0, 0, 0))
                    clientConnection.Send(AllNetworkPackets.MoveTo, new MoveTo { playerId = pc.Player.id, x = pc.destPosition.x, y = pc.destPosition.y, z = pc.destPosition.z });
            }
        }
        foreach(ChangeStateNeeded cstn in csn)
        {
            cstn.destinatary.clientConnection.Send(AllNetworkPackets.ChangeFriendStatus, cstn.cfs);
        }
    }

    public void SavePlayerData()//Actualitza la informació del jugador a la base de dades
    {
        AccountManagement.SavePlayerStats(Player, this.transform.position.x, this.transform.position.y, this.transform.position.z);
    }

    void HealPlayer()//Cura al jugador si compleix les condicions
    {
        if (Time.time - lastHit > 5f && Time.time - lastAtack > 5f && (Player.curr_hp!=Player.max_hp || Player.curr_mana != Player.max_mana) && Player.curr_hp !=0)//Si fa més de 5s que ens han fet mal ens curem
        {
            int healAmount = 5 + Player.level;
            if (Player.curr_hp+healAmount <= Player.max_hp)
                Player.curr_hp += healAmount;
            else
                Player.curr_hp = Player.max_hp;
            int healManaAmount = 10 + (Player.level*2);
            if (Player.curr_mana + healManaAmount <= Player.max_mana)
                Player.curr_mana += healManaAmount;
            else
                Player.curr_mana = Player.max_mana;
        }
    }

    void SendInfoToListeners()//Envia actualitzacions de la vida als qui la demanin
    {
        bool hasChanges = CheckIfDefUpdate();
        if (hasChanges)
        {
            UpdatePlayerDef def = new UpdatePlayerDef();
            def.curr_hp = Player.curr_hp;
            def.curr_mana = Player.curr_mana;
            def.lvl = Player.level;
            def.name = Player.playername;
            def.classid = Player.class_id;
            def.max_hp = Player.max_hp;
            def.max_mana = Player.max_mana;
            def.playerID = Player.id;
            foreach (NetworkConnection nc in infoListeners)
            {
                nc.Send(AllNetworkPackets.UpdatePlayerDef, def);
            }
        }
        if(hasChanges || XpHasChanged())
        {
            try
            {
                if (clientConnection.lastError != NetworkError.Timeout)
                    clientConnection.Send(AllNetworkPackets.MyPlayerUpdate, new MyPlayerUpdate { hp = Player.curr_hp, hp_max = Player.max_hp, mana = Player.curr_mana, mana_max = Player.max_mana, xp = Player.xp, level = Player.level });
            }
            catch { }
        }
    }
   
    /// <summary>
    /// Comprova si hi ha canvis en la vida, mana o nivell, per a actualitzar la descripció de qui la demani
    /// </summary>
    bool CheckIfDefUpdate()
    {
        bool changes = false;
        if(lastHP!=Player.curr_hp)
        {
            if (lastHP > Player.curr_hp)
                lastHit = Time.time;
            lastHP = Player.curr_hp;
            changes = true;
        }
        if (lastMANA != Player.curr_mana)
        {
            lastMANA = Player.curr_mana;
            changes = true;
        }
        if (lastLV != Player.level)
        {
            lastLV = Player.level;
            changes = true;
        }
        return changes;
    }

    /// <summary>
    /// Comprova si la xp ha canviat
    /// </summary>
    bool XpHasChanged()
    {
        return lastXP != Player.xp;
    }

    void ServerUpdate()//Realitza l'actualització del paquet de destí
    {
        if (pkt != null)
        {
            destPosition = new Vector3(pkt.x, pkt.y, pkt.z);
            agent.SetDestination(destPosition);
            pkt.playerId = Player.id;
            SendToAllNearby(AllNetworkPackets.MoveTo, pkt);
            pkt = null;
        }
        if (transform.position == destPosition)
        {
            SendToAllNearby(AllNetworkPackets.CurrentPosition, new CurrentPosition
            {
                playerId = this.Player.id,
                x = transform.position.x,
                y = transform.position.y,
                z = transform.position.z,
                yr = transform.rotation.eulerAngles.y
            });
            destPosition = new Vector3(0, 0, 0);
        }
    }

    void SendToAllNearby(short msgType, MessageBase msg)//Envia a tots els jugadors propers
    {
        foreach(PlayerController pc in nearbyUser)
        {
            pc.clientConnection.Send(msgType, msg);
        }
    }

    public bool ApplyDamage(int damage)//Aplica mal a aquest jugador
    {
        bool death = false;
        int damageDealt = damage - Player.defense;
        if (damageDealt < 1)
            damageDealt = 1;
        if (Player.curr_hp > damageDealt)
        {
            Player.curr_hp -= damageDealt;
        }
        else if (Player.curr_hp > 0)//Si el maten, enviar paquet a tots els propers
        {
            KillPlayer kp = new KillPlayer();
            Player.curr_hp = 0;
            kp.playerID = Player.id;
            SendToAllNearby(AllNetworkPackets.KillPlayer, kp);
            clientConnection.Send(AllNetworkPackets.KillPlayer, kp);
            Invoke("Respawn", 10f + Player.level);
            death = true;
            agent.isStopped = true;
        }
        return death;
    }

    public void AddXP(int xpRaw)//Afegeix xp al jugador
    {
        int xpGet = xpRaw/ Player.level;
        if (xpGet+Player.xp<100)//Aconseguim xp
        {
            Player.xp += xpGet;
        }
        else if(Player.level<100)//max lvl99
        {
            Player.xp = 0;
            Player.level++;
            Player.max_hp += Player.level * 2;
            Player.max_mana += Player.level * 2;
            Player.damage += (int)(Player.damage * 0.1f);
            Player.defense += (int)(Player.defense * 0.1f);
        }
    }

    void ForceUpdatePosition()//Actualització de la posició real
    {
        try
        {
            CurrentPosition cp = new CurrentPosition { playerId = Player.id, x = transform.position.x, y = transform.position.y, z = transform.position.z, yr = this.transform.eulerAngles.y };
            SendToAllNearby(AllNetworkPackets.CurrentPosition, cp);
            if(clientConnection.lastError!=NetworkError.Timeout)
                clientConnection.Send(AllNetworkPackets.CurrentPosition, cp);
        }
        catch { }
    }

    void Respawn()//Fa reapareixre el jugador despres d'haver mort per l'apply damage
    {
        destPosition= new Vector3(0, 0, 0);
        this.transform.position = destPosition;
        SendToAllNearby(AllNetworkPackets.MoveTo, new MoveTo { playerId = this.Player.id, x=0, y=0,z=0});
        agent.SetDestination(this.transform.position);
        this.Player.curr_hp = Player.max_hp;
        this.Player.curr_mana = Player.max_mana;
        ForceUpdatePosition();
        agent.isStopped = false;
        RemoveDebuffs rd = new RemoveDebuffs();
        rd.playerid = Player.id;
        SendToAllNearby(AllNetworkPackets.RemoveDebuffs, rd);
        clientConnection.Send(AllNetworkPackets.RemoveDebuffs, rd);
    }

    public void TryMagicianAtack(NPCInstance target)//Intenta realitzar l'atac de MAG
    {
        if ((Time.time - lastAtack > 2f) && Vector3.Distance(this.transform.position, target.npc_go.transform.position) < 30 && Player.curr_mana > 20 && Player.curr_hp > 0)//Si estem a rang i tenim mana fem l'atac
        {
            lastAtack = Time.time;
            agent.isStopped = true;
            this.transform.LookAt(target.npc_go.transform);
            ForceUpdatePosition();
            CastAtack ca = new CastAtack();
            ca.playerid = Player.id;
            SendToAllNearby(AllNetworkPackets.CastAtack, ca);
            clientConnection.Send(AllNetworkPackets.CastAtack, ca);
            Invoke("MoveAgain", 1.3f);
            target.controller.DoDamage(Player.damage,this);
            Player.curr_mana -= 20;
        }
        
    }

    public void TrySwordsmanAtack()//Intenta realitzar l'atac d'espadachin
    {
        if ((Time.time - lastAtack > 1f) && Player.curr_mana > 10 && Player.curr_hp > 0)//Si estem a rang i tenim mana fem l'atac
        {
            lastAtack = Time.time;
            agent.isStopped = true;
            ForceUpdatePosition();
            CastAtack ca = new CastAtack();
            ca.playerid = Player.id;
            SendToAllNearby(AllNetworkPackets.CastAtack, ca);
            clientConnection.Send(AllNetworkPackets.CastAtack, ca);
            Invoke("MoveAgain", 0.85f);
            Invoke("SwordsmanDmg", 0.2f);
            Player.curr_mana -= 10;
        }

    }

    private void SwordsmanDmg()//Activa l'objecte aplicador de mal de l'espadachin
    {
        swordsmanAtack.SetActive(true);
        Invoke("SwordsmanDmgOff", 0.2f);
    }

    private void SwordsmanDmgOff()//Desactiva l'objecte de mal de l'espadachin
    {
        swordsmanAtack.SetActive(false);
    }

    public void TryArcherAtack(NPCInstance npc)//Intenta realitzar l'atac d'arquer
    {
        if ((Time.time - lastAtack > 1f) && Vector3.Distance(this.transform.position, npc.npc_go.transform.position) < 30 && Player.curr_mana > 5 && Player.curr_hp > 0)//Si estem a rang i tenim mana fem l'atac
        {
            target = npc.npc_go.transform;
            lastAtack = Time.time;
            agent.isStopped = true;
            ForceUpdatePosition();
            CastAtack ca = new CastAtack();
            ca.playerid = Player.id;
            ca.npc_type = npc.type;
            ca.npc_id = npc.id;
            SendToAllNearby(AllNetworkPackets.CastAtack, ca);
            clientConnection.Send(AllNetworkPackets.CastAtack, ca);
            this.transform.LookAt(npc.npc_go.transform);
            Invoke("archerShoot", 0.25f);
            Invoke("MoveAgain", 0.35f);
            Player.curr_mana -= 5;
        }

    }

    private void archerShoot()//Instancia la bala de l'arquer
    {
        Instantiate(archerAtack, new Vector3(this.transform.position.x, this.transform.position.y + 4, this.transform.position.z), Quaternion.identity).GetComponent<ArrowShot>().SetBullet(this.Player.damage, target, this);
    }

    public void dab(Dab packt)//Realitza un dab
    {
        if (Player.curr_hp > 0)//Si tenim vida fem dab
        {
            agent.isStopped = true;
            ForceUpdatePosition();
            CastAtack ca = new CastAtack();
            ca.playerid = Player.id;
            SendToAllNearby(AllNetworkPackets.Dab, packt);
            clientConnection.Send(AllNetworkPackets.Dab, packt);
            Invoke("MoveAgain", 0.5f);
        }
    }

    void MoveAgain()//Ens permet tornar a moure
    {
        if (Player.curr_hp>0)
            agent.isStopped = false;
    }

    private void OnDestroy()
    {
        foreach(EnemyNearbyUnits enu in nearbyEnemy)
        {
            enu.nearbyUser.Remove(this.clientConnection);
        }
        foreach(PlayerController pc in nearbyUser)
        {
            pc.nearbyUser.Remove(this);
        }
    }
}