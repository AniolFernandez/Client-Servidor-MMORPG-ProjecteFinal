using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;

#pragma warning disable CS0618
public class ConnectToServer : MonoBehaviour
{
    public string ip= "127.0.0.1";
    public int port = 7777;

    public GameObject messageBox;
    public GameObject msgbxInstance;
    public GameObject playerPrefab;
    public GameObject otherplayerPrefab;
    public GameObject damageToPlayer;
    public GameObject Enemy1Prefab;
    public GameObject goldBag;

    private List<EnemyController> enemys1 = new List<EnemyController>();//Llista d'npcs del tipus 1
    private static List<Player> allPlayers = new List<Player>();//Llista de tots els jugadors connectats
    public List<GoldBag> gold = new List<GoldBag>();
    public static Player myPlayer;//El nostre jugador
    public static string charc = "";
    public TimeToRespawn timeRspwn;
    public Chat chat;//Chat
    public PlayerHUD playerHUD;//HUD del nostre jugador
    public OtherInfoHUD otherHUD;//HUD del target

    private bool allowQuitting = false;
    public static NetworkClient client = new NetworkClient();
    public static ConnectToServer instance;
    private Queue<AddFriendToList> tmp = new Queue<AddFriendToList>();//Cua per als paquets d'afegir amic. Ja que ens arriben abans de haver inicialitzat la llista
    void Start()
    {
        instance = this;
        GameObject[] objs = GameObject.FindGameObjectsWithTag("NetworkManager");
        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
        InitializeClient();
    }

    private void Update()
    {
        if (FriendContainer.instance != null && tmp.Count>0)//Si tenmi paquets afegits a la cua els processem
        {
            AddFriendToList pkt = tmp.Dequeue();
            FriendContainer.instance.AddFriendToList(pkt.id, pkt.name, pkt.online);
        }      
    }

    #region Networking

    void InitializeClient()//Inicialització del servidor. Ens connectem i registrem tots els paquets que hem de rebre
    {
        /*ConnectionConfig Config = new ConnectionConfig();
        Config.AddChannel(QosType.Reliable);
        HostTopology Topology = new HostTopology(Config, 4096);
        client.Configure(Topology);*/
        client.Connect(ip, port);
        client.RegisterHandler(AllNetworkPackets.LoginAccepted, LogginSucces);
        client.RegisterHandler(AllNetworkPackets.BadLogin, badLogin);
        client.RegisterHandler(AllNetworkPackets.PlayerConnected, otherPlayerConnected);
        client.RegisterHandler(AllNetworkPackets.CurrentPosition, updateCurrentPosition);
        client.RegisterHandler(AllNetworkPackets.MoveTo, updateDestPosition);
        client.RegisterHandler(AllNetworkPackets.PlayerDisconnected, otherPlayerDisconnected);
        client.RegisterHandler(AllNetworkPackets.RegisterResponse, registerResponse);
        client.RegisterHandler(AllNetworkPackets.CreatePlayerResp, createResponse);
        client.RegisterHandler(AllNetworkPackets.LoginCharacter, loginCharacter);
        client.RegisterHandler(AllNetworkPackets.SendChatMessage, SendMessageToChat);
        client.RegisterHandler(AllNetworkPackets.SendPrivateMessage, SendPrivateMesssage);
        client.RegisterHandler(AllNetworkPackets.UpdatePlayerDef, UpdatePlayerDef);
        client.RegisterHandler(AllNetworkPackets.InstantiateNPC, InstantiateNPC);
        client.RegisterHandler(AllNetworkPackets.DisposeNPC, DisposeNPC);
        client.RegisterHandler(AllNetworkPackets.MyPlayerUpdate, MyPlayerUpdate);
        client.RegisterHandler(AllNetworkPackets.MoveNPC, MoveNPC);
        client.RegisterHandler(AllNetworkPackets.AtackNPC, AtackNPC);
        client.RegisterHandler(AllNetworkPackets.KillPlayer, KillPlayer);
        client.RegisterHandler(AllNetworkPackets.UpdateCurrentPosNPC, updateCurrentPosNPC);
        client.RegisterHandler(AllNetworkPackets.UpdateNPCDef, UpdateNPCDef);
        client.RegisterHandler(AllNetworkPackets.CastAtack, castAtack);
        client.RegisterHandler(AllNetworkPackets.RemoveDebuffs, removeDebuffs);
        client.RegisterHandler(AllNetworkPackets.KillNPC, KillNPC);
        client.RegisterHandler(AllNetworkPackets.Dab, dab);
        client.RegisterHandler(AllNetworkPackets.SendFriendRequest, SendFriendRequest);
        client.RegisterHandler(AllNetworkPackets.OkeyFriend, FriendRequestResponse);
        client.RegisterHandler(AllNetworkPackets.AddFriendToList, AddFriendToList);
        client.RegisterHandler(AllNetworkPackets.ChangeFriendStatus, ChangeFriendStatus);
        client.RegisterHandler(AllNetworkPackets.RemoveFriend, RemoveFriend);
        client.RegisterHandler(AllNetworkPackets.AddGoldBag, AddGoldBag);
        client.RegisterHandler(AllNetworkPackets.RemoveGoldBag, RemoveGoldBag);
        client.RegisterHandler(AllNetworkPackets.CatchGoldResp, CatchGoldResp);
        client.RegisterHandler(AllNetworkPackets.CurrentGold, CurrentGold);
        InvokeRepeating("CheckConnected", 0.5f, 1f);
    }

    #region LoginRegister

    private void badLogin(NetworkMessage msg)//A l'intentar fer un loggin erroni
    {
        BadLogin msglogin = msg.ReadMessage<BadLogin>();
        if(msglogin.bad)
            MessageBox("Wrong username or password.", () => Destroy(msgbxInstance), () => Destroy(msgbxInstance));
        else
            MessageBox("User already connected.", () => Destroy(msgbxInstance), () => Destroy(msgbxInstance));
    }

    private void loginCharacter(NetworkMessage msg)//Paquet per a loggejar el nostre personatje principal
    {
        LoginCharacter msglogin = msg.ReadMessage<LoginCharacter>();
        Player tmp = new Player();
        tmp.playerCharacter = Instantiate(playerPrefab, new Vector3(msglogin.x, msglogin.y, msglogin.z), Quaternion.identity);
        tmp.playerController.pData.player_id = msglogin.id;
        tmp.playerController.pData.playername = msglogin.playername;
        tmp.playerController.pData.level = msglogin.level;
        tmp.playerController.pData.xp = msglogin.xp;
        tmp.playerController.pData.class_id = msglogin.class_id;
        tmp.playerController.playerName.text = msglogin.playername;
        tmp.playerController.pData.curr_hp = msglogin.curr_hp;
        tmp.playerController.pData.curr_mana = msglogin.curr_mana;
        tmp.playerController.pData.gold = msglogin.gold;
        myPlayer = tmp;
        playerHUD.maxHP = msglogin.max_hp;
        playerHUD.maxMANA = msglogin.max_mana;
        playerHUD.UpdateXP(msglogin.xp);
        playerHUD.UpdateCharDef(msglogin.level, msglogin.playername, LoginScript.GetClass(tmp.playerController.pData.class_id));
        playerHUD.UpdateHP_MANA(msglogin.curr_hp, msglogin.curr_mana);
        InventoryManager.instance.UpdateGold(tmp.playerController.pData.gold);
    }

    private void LogginSucces(NetworkMessage msg)//Paquet per a loggejar la compta 
    {
        LoginAccepted msglogin = msg.ReadMessage<LoginAccepted>();
        CharsAvailable chars = new CharsAvailable();
        chars.player_1_class = msglogin.player_1_class;
        chars.player_1_level = msglogin.player_1_level;
        chars.player_1_name = msglogin.player_1_name;
        chars.player_2_class = msglogin.player_2_class;
        chars.player_2_level = msglogin.player_2_level;
        chars.player_2_name = msglogin.player_2_name;
        chars.player_3_class = msglogin.player_3_class;
        chars.player_3_level = msglogin.player_3_level;
        chars.player_3_name = msglogin.player_3_name;
        GameObject.Find("Canvas").GetComponent<LoginScript>().LoggedIn(chars);
    }

    private void registerResponse(NetworkMessage msg)//Paquet de resposta amb el resultat del registre
    {
        RegisterResponse tmpmsg = msg.ReadMessage<RegisterResponse>();
        if (msgbxInstance != null)
            Destroy(msgbxInstance);
        switch (tmpmsg.result)
        {
            case 0:
                MessageBox("Registered successfully!", () => Destroy(msgbxInstance), () => Destroy(msgbxInstance));
                LoginScript tmpRef = GameObject.Find("Canvas").GetComponent<LoginScript>();
                tmpRef.RegisterGameObject.SetActive(false);
                tmpRef.LoginGameObject.SetActive(true);
                break;
            case 1:
                MessageBox("The username already exists.", () => Destroy(msgbxInstance), () => Destroy(msgbxInstance));
                break;
            case 2:
                MessageBox("Something went wrong...", () => Destroy(msgbxInstance), () => Destroy(msgbxInstance));
                break;
            case 3:
                MessageBox("Some input value is wrong", () => Destroy(msgbxInstance), () => Destroy(msgbxInstance));
                break;
        }

    }

    private void createResponse(NetworkMessage msg)//Paquet de resposta a la creació del personatge
    {
        CreatePlayerResp tmpmsg = msg.ReadMessage<CreatePlayerResp>();
        if (msgbxInstance != null)
            Destroy(msgbxInstance);
        switch (tmpmsg.result)
        {
            case 0:
                MessageBox("Player created successfully!", () => Destroy(msgbxInstance), () => Destroy(msgbxInstance));
                GameObject.Find("Canvas").GetComponent<LoginScript>().ExitCreation();
                break;
            case 1:
                MessageBox("Something went wrong...", () => Destroy(msgbxInstance), () => Destroy(msgbxInstance));
                GameObject.Find("Canvas").GetComponent<LoginScript>().ExitCreation();
                break;
            case 2:
                MessageBox("Playername already exists!", () => Destroy(msgbxInstance), () => Destroy(msgbxInstance));
                GameObject.Find("Canvas").GetComponent<LoginScript>().ExitCreation();
                break;
            case 3:
                MessageBox("You've reached your max characters", () => Destroy(msgbxInstance), () => Destroy(msgbxInstance));
                GameObject.Find("Canvas").GetComponent<LoginScript>().ExitCreation();
                break;
        }

    }

    #endregion

    #region Players

    private void otherPlayerConnected(NetworkMessage msg)//Quan un altre jugador entra al nostre camp, revem el seu paquet de connexió i el fem apareixre
    {
        PlayerConnected msglogin = msg.ReadMessage<PlayerConnected>();
        if(allPlayers.Find(x=>x.playerController.pData.player_id == msglogin.playerId) == null)
        {
            Player tmp = new Player();
            tmp.playerCharacter = Instantiate(otherplayerPrefab, new Vector3(msglogin.x, msglogin.y, msglogin.z), Quaternion.identity);
            tmp.playerController.pData.player_id = msglogin.playerId;
            tmp.playerController.pData.class_id = msglogin.classid;
            tmp.playerController.playerName.text = msglogin.playername;
            tmp.playerController.pData.curr_hp = 1;
            allPlayers.Add(tmp);
            client.Send(AllNetworkPackets.GotPlayer, new GotPlayer { id = msglogin.playerId });
        }
    }

    private void otherPlayerDisconnected(NetworkMessage msg)//Quan un altre jugador surt del nostre camp, revem el seu paquet i el fem desapareixre
    {
        PlayerDisconnected tmpmsg = msg.ReadMessage<PlayerDisconnected>();
        Player other = allPlayers.Find(x => x.playerController.pData.player_id == tmpmsg.playerId);
        if (other != null)
        {
            allPlayers.Remove(other);
            Destroy(other.playerCharacter);
        }

    }

    private void updateCurrentPosition(NetworkMessage msg)//Actualització de l'actual posició real dels personatges
    {
        CurrentPosition tmpmsg = msg.ReadMessage<CurrentPosition>();
        Vector3 posicioReal = new Vector3(tmpmsg.x, tmpmsg.y, tmpmsg.z);
        if (myPlayer != null && myPlayer.playerController.pData.player_id == tmpmsg.playerId)
        {
            //Si estem a més de 3 unitats de la posició real, forçem el canvi de lloc. Si no fem això notariem petits lagazos.. i aquesta opció hi és per corretgir errors heavys, no centesimes de posició
            if (Vector3.Distance(posicioReal, myPlayer.playerCharacter.transform.position) > 3)
            {
                myPlayer.playerController.gameObject.transform.position = posicioReal;
            }
            myPlayer.playerController.gameObject.transform.rotation = Quaternion.Euler(0, tmpmsg.yr, 0);//La rotació la canviem si o si(pels atacs)
        }
        else
        {
            Player dest = allPlayers.Find(x => x.playerController.pData.player_id == tmpmsg.playerId);
            if (dest != null && Vector3.Distance(posicioReal, dest.playerCharacter.transform.position) > 3)
            {
                dest.playerController.gameObject.transform.position = posicioReal;     
            }
            dest.playerController.gameObject.transform.rotation = Quaternion.Euler(0, tmpmsg.yr, 0);

        }
    }

    private void updateDestPosition(NetworkMessage msg)//Actualització de la posició de destí dels personatges
    {
        MoveTo tmpmsg = msg.ReadMessage<MoveTo>();
        Player dest = allPlayers.Find(x => x.playerController.pData.player_id == tmpmsg.playerId);
        if (dest != null)
            dest.playerController.moveTo = tmpmsg;
    }

    private void SendMessageToChat(NetworkMessage msg)//Paquet per a afegir un missatge al chat general
    {
        SendChatMessage msgchat = msg.ReadMessage<SendChatMessage>();
        chat.SendMessageToChat(msgchat.message, Color.white);
    }

    private void SendPrivateMesssage(NetworkMessage msg)//Paquet que ens envia un jugador per a afegir com a msg privat
    {
        SendPrivateMessage msgchat = msg.ReadMessage<SendPrivateMessage>();
        chat.SendMessageToChat(msgchat.message, Color.magenta);
    }

    private void UpdatePlayerDef(NetworkMessage msg)//Actualització de la info del personatge al que tenim com a target
    {
        UpdatePlayerDef msgdef = msg.ReadMessage<UpdatePlayerDef>();
        if(LocalPlayer.Instance.target_type == 0 && LocalPlayer.Instance.id_target == msgdef.playerID)
        {
            otherHUD.id = msgdef.playerID;
            otherHUD.type = 0;
            otherHUD.maxHP = msgdef.max_hp;
            otherHUD.maxMANA = msgdef.max_mana;
            otherHUD.UpdateHP_MANA(msgdef.curr_hp, msgdef.curr_mana);
            otherHUD.UpdateCharDef(msgdef.lvl, msgdef.name, LoginScript.GetClass(msgdef.classid));
            otherHUD.gameObject.SetActive(true);
        }
    }

    private void MyPlayerUpdate(NetworkMessage msg)//Acctualizació de la info del nostre jugador
    {
        MyPlayerUpdate msgdef = msg.ReadMessage<MyPlayerUpdate>();
        TextMeshPro tmp;
        int amount = msgdef.hp - myPlayer.playerController.pData.curr_hp;
        //Fa apareixre els numeros de + o - vida
        if (msgdef.hp < myPlayer.playerController.pData.curr_hp && amount!=0)
        {//Fan mal
            tmp = Instantiate(damageToPlayer, myPlayer.playerCharacter.transform.position, Quaternion.identity).GetComponentInChildren<TextMeshPro>();
            tmp.text =  amount+ "";
            tmp.color = Color.red;
        }
        else if(amount != 0){//Es cura
            tmp = Instantiate(damageToPlayer, myPlayer.playerCharacter.transform.position, Quaternion.identity).GetComponentInChildren<TextMeshPro>();
            tmp.text = amount + "";
            tmp.color = Color.green;
        }
        //Numeros de curació o gast de mana
        int amountMana = msgdef.mana - myPlayer.playerController.pData.curr_mana;
        //Fa apareixre els numeros de + o - mana
        if (msgdef.mana < myPlayer.playerController.pData.curr_mana && amount != 0)
        {//Gastem mana
            tmp = Instantiate(damageToPlayer, myPlayer.playerCharacter.transform.position, Quaternion.identity).GetComponentInChildren<TextMeshPro>();
            tmp.text = amountMana + "";
            tmp.color = Color.blue;
            tmp.fontSize = tmp.fontSize - 8;
            tmp.transform.position = new Vector3(tmp.transform.position.x + 2, tmp.transform.position.y, tmp.transform.position.z);
        }
        else if (amountMana != 0)
        {//Es cura
            tmp = Instantiate(damageToPlayer, myPlayer.playerCharacter.transform.position, Quaternion.identity).GetComponentInChildren<TextMeshPro>();
            tmp.text = amountMana + "";
            tmp.color = Color.cyan;
            tmp.fontSize = tmp.fontSize - 8;
            tmp.transform.position = new Vector3(tmp.transform.position.x + 2, tmp.transform.position.y, tmp.transform.position.z);
        }
        //Realitza l'actualització
        playerHUD.maxHP = msgdef.hp_max;
        playerHUD.maxMANA = msgdef.mana_max;
        playerHUD.UpdateHP_MANA(msgdef.hp, msgdef.mana);
        playerHUD.UpdateXP(msgdef.xp);
        playerHUD.UpdateCharDef(msgdef.level,myPlayer.playerController.pData.playername,LoginScript.GetClass(myPlayer.playerController.pData.class_id));
        myPlayer.playerController.pData.curr_hp = msgdef.hp;
        myPlayer.playerController.pData.curr_mana = msgdef.mana;
    }

    private void KillPlayer(NetworkMessage msg)//Un jugador ha mort, se l'ha d'animar
    {
        KillPlayer msgkll = msg.ReadMessage<KillPlayer>();
        if(myPlayer.playerController.pData.player_id == msgkll.playerID)//Han matat el nostre jugador
        {//Animem la nostre mort
            int segons = 10 + myPlayer.playerController.pData.level;
            MessageBox("You've died! You will respawn in "+segons+" seconds", ()=>Destroy(msgbxInstance), () => Destroy(msgbxInstance));
            myPlayer.playerController.Stop();
            myPlayer.playerController.anim.SetBool("alive", false);
            myPlayer.playerController.anim.Play("Player_die");
            timeRspwn.SetRespawn(segons);
        }
        else
        {//Animem la mort de l'altre jugador
            Player p = allPlayers.Find(x => x.playerController.pData.player_id == msgkll.playerID);
            if(p!=null){
                p.playerController.pData.curr_hp = 0;
                p.playerController.Stop();
                p.playerController.anim.SetBool("alive", false);
                p.playerController.anim.Play("Player_die");
            }
        }
    }

    private void removeDebuffs(NetworkMessage msg)//Paquet que rebem quan el nostre jugador torna a estar viu despres de la mort
    {//Tornem a deixar moure
        RemoveDebuffs tmpmsg = msg.ReadMessage<RemoveDebuffs>();
        PlayerController src = null;
        //Busquem l'atacant
        if (tmpmsg.playerid == myPlayer.playerController.pData.player_id)
        {
            src = myPlayer.playerController;
        }
        else
        {
            Player p = allPlayers.Find(x => x.playerController.pData.player_id == tmpmsg.playerid);
            if (p != null)
            {
                src = p.playerController;
                src.pData.curr_hp = 1;
            }
                
        }
        if (src != null)
        {
            src.agent.isStopped = false;
            src.anim.SetBool("alive", true);
            src.agent.SetDestination(src.gameObject.transform.position);
        }
       
    }

    private void castAtack(NetworkMessage msg)//Fa que el jugador indicat castegi el seu atac
    {//Castegem l'atac on toqui
        CastAtack tmpmsg = msg.ReadMessage<CastAtack>();
        PlayerController src = null;
        //Busquem l'atacant
        if(tmpmsg.playerid == myPlayer.playerController.pData.player_id)
        {
            src = myPlayer.playerController;
        }
        else
        {
            Player p = allPlayers.Find(x => x.playerController.pData.player_id == tmpmsg.playerid);
            if (p != null)
                src = p.playerController;
        }
        if (src != null)//si tenim atacant, fem la seva habilitat
        {
            switch (src.pData.class_id)
            {
                case 3://Magician
                    src.MagicianAtack();
                    break;
                case 2://Arch
                    Transform target = null;
                    switch (tmpmsg.npc_type)
                    {
                        case 1:
                            EnemyController e = enemys1.Find(x => x.enemyID == tmpmsg.npc_id);
                            if (e != null)
                                target = e.gameObject.transform;
                            break; 
                    }
                    if(target!=null)
                    src.ArcherAtack(target);
                    break;
                case 1://Swordsman
                    src.SwordsmanAtack();
                    break;
            }
        }
    }

    private void dab(NetworkMessage msg)//Animació de dab
    {//Castejem animació dab
        Dab tmpmsg = msg.ReadMessage<Dab>();
        PlayerController src = null;
        if (tmpmsg.playerID == myPlayer.playerController.pData.player_id)
        {
            src = myPlayer.playerController;
        }
        else
        {
            Player p = allPlayers.Find(x => x.playerController.pData.player_id == tmpmsg.playerID);
            if (p != null)
                src = p.playerController;
        }
        if (src != null)//si tenim atacant, fem la seva habilitat
        {
            src.Dab();
        }
    }

    #endregion

    #region NPC

    private void InstantiateNPC(NetworkMessage msg)//Inicialització d'un NPC proper
    {
        InstantiateNPC msgdef = msg.ReadMessage<InstantiateNPC>();
        switch (msgdef.npc_type)
        {
            case 1:
                EnemyController tmp = Instantiate(Enemy1Prefab, new Vector3(msgdef.x, msgdef.y, msgdef.z), Quaternion.identity).GetComponent<EnemyController>();
                tmp.enemyID = msgdef.npc_id;
                enemys1.Add(tmp);
                break;
        }
    }

    private void DisposeNPC(NetworkMessage msg)//Dispose d'un NPC que ens allunyem
    {
        DisposeNPC msgdef = msg.ReadMessage<DisposeNPC>();
        switch (msgdef.npc_type)
        {
            case 1:
                EnemyController tmp = enemys1.Find(x => x.enemyID == msgdef.npc_id);
                if (tmp != null)
                {
                    enemys1.Remove(tmp);
                    Destroy(tmp.gameObject);
                }
                break;
        }
    }

    private void MoveNPC(NetworkMessage msg)//Moviment d'un NPC. Canvia la seva posició de destií
    {
        MoveNPC msgmv = msg.ReadMessage<MoveNPC>();
        switch (msgmv.npc_type)
        {
            case 1:
                EnemyController tmp = enemys1.Find(x => x.enemyID == msgmv.npc_id);
                if (tmp != null)
                {
                    tmp.moveTo = msgmv;
                }
                break;
        }
    }

    private void updateCurrentPosNPC(NetworkMessage msg)//Actualizació de la posició real de l'NPC
    {//Igual que amb els jugadors, si calculem malament la posició l'hem de rectificar
        UpdateCurrentPosNPC tmpmsg = msg.ReadMessage<UpdateCurrentPosNPC>();
        Vector3 posicioReal = new Vector3(tmpmsg.x, tmpmsg.y, tmpmsg.z);
        switch (tmpmsg.npc_type)
        {
            case 1:
                EnemyController tmp = enemys1.Find(x => x.enemyID == tmpmsg.npc_id);
                if (tmp != null && Vector3.Distance(posicioReal, tmp.gameObject.transform.position) > 3)
                    tmp.gameObject.transform.position = posicioReal;
                break;
        }
    }

    private void UpdateNPCDef(NetworkMessage msg)//Actualització de la info de l'NPC al que tenim com a target. Disposa d'ajudes gràfices per a veure el mal que li hem fet
    {
        UpdateNPCDef msgdef = msg.ReadMessage<UpdateNPCDef>();
        if(LocalPlayer.Instance.target_type == msgdef.type && LocalPlayer.Instance.id_target == msgdef.npc_id)
        {
            TextMeshPro tmp;
            int amount = msgdef.curr_hp - otherHUD.lastHP;
            //Fa apareixre els numeros de + o - vida
            if (msgdef.curr_hp < otherHUD.lastHP && amount != 0)
            {//Fan mal
                tmp = Instantiate(damageToPlayer, otherHUD.target.transform.position, Quaternion.identity).GetComponentInChildren<TextMeshPro>();
                tmp.text = amount + "";
                tmp.color = Color.white;
            }
            LocalPlayer.Instance.id_target = msgdef.npc_id;
            LocalPlayer.Instance.target_type = msgdef.type;
            otherHUD.id = msgdef.npc_id;
            otherHUD.lastHP = msgdef.curr_hp;
            otherHUD.type = msgdef.type;
            otherHUD.maxHP = msgdef.max_hp;
            otherHUD.maxMANA = msgdef.max_mana;
            otherHUD.UpdateHP_MANA(msgdef.curr_hp, msgdef.curr_mana);
            otherHUD.UpdateCharDef(msgdef.lvl, msgdef.name, "NPC");
            otherHUD.gameObject.SetActive(true);
        }
        

    }

    private void AtackNPC(NetworkMessage msg)//NPC Ataca
    {
        AtackNPC msgmv = msg.ReadMessage<AtackNPC>();
        switch (msgmv.npc_type)
        {
            case 1:
                EnemyController tmp = enemys1.Find(x => x.enemyID == msgmv.npc_id);
                if (tmp != null)
                {
                    tmp.DoAtack();
                }
                break;
        }
    }

    private void KillNPC(NetworkMessage msg)//L'npc indicat ha mort
    {
        KillNPC msgkll = msg.ReadMessage<KillNPC>();
        switch (msgkll.npc_type)
        {
            case 1:
                EnemyController p = enemys1.Find(x => x.enemyID == msgkll.npc_id);
                if (p != null)
                {
                    p.KillThis();
                    enemys1.Remove(p);
                }
                break;
        }

    }

    #endregion

    #region Friendzone
    private void SendFriendRequest(NetworkMessage msg)//Rebem paquet de que algú vol ser amic nostre
    {
        SendFriendRequest msgf = msg.ReadMessage<SendFriendRequest>();
        MessageBox(msgf.name_from + " wants to be your friend.\nAccept?", () => AcceptFriendReq(msgf), () => DeclineFriendReq(msgf));
    }

    private void FriendRequestResponse(NetworkMessage msg)//Resposta a la petició d'amistat
    {
        OkeyFriend msgf = msg.ReadMessage<OkeyFriend>();
        if(msgf.accepted)
            MessageBox("Added to friendlist!", () => Destroy(msgbxInstance), () => Destroy(msgbxInstance));
        else
            MessageBox("Declined!", () => Destroy(msgbxInstance), () => Destroy(msgbxInstance));
    }

    private void AddFriendToList(NetworkMessage msg)//Paquet per a afegir amics a la llista d'amics
    {
        AddFriendToList aftl = msg.ReadMessage<AddFriendToList>();
        if (FriendContainer.instance != null)
            FriendContainer.instance.AddFriendToList(aftl.id, aftl.name, aftl.online);
        else
            tmp.Enqueue(aftl);
    }

    private void ChangeFriendStatus(NetworkMessage msg)//Canvia l'estat de l'amic online/offline
    {
        ChangeFriendStatus chg = msg.ReadMessage<ChangeFriendStatus>();
        if (!FriendContainer.instance.FriendAlreadyChanged(chg.id, chg.online))
        {
            string name = FriendContainer.instance.ChangeFriendStatus(chg.id, chg.online);
            if (chg.online)
                chat.SendMessageToChat("Your friend " + name + " just connected!", Color.yellow);
            else
                chat.SendMessageToChat("Your friend " + name + " has disconnected.", Color.yellow);
            client.Send(AllNetworkPackets.ChangeFriendStatus, chg);
        }
    }

    private void RemoveFriend(NetworkMessage msg)//Elimina l'amic de la llista d'amics
    {
        RemoveFriend chg = msg.ReadMessage<RemoveFriend>();
        FriendContainer.instance.RemoveFriend(chg.id);
        chat.SendMessageToChat("You've lost a friend...", Color.yellow);
    }
    #endregion

    #region Items
    /// <summary>
    /// Afegeix la bossa d'or al terre
    /// </summary>
    private void AddGoldBag(NetworkMessage msg)//Elimina l'amic de la llista d'amics
    {
        AddGoldBag chg = msg.ReadMessage<AddGoldBag>();
        GameObject goldB = Instantiate(goldBag, new Vector3(chg.x, chg.y, chg.z), Quaternion.identity);
        GoldBag g = goldB.GetComponent<GoldBag>();
        g.SetBagOfGold(chg.amount, chg.id);
        gold.Add(g);
    }

    /// <summary>
    /// Elimina la bossa d'or del terre (algú l'ha agafat)
    /// </summary>
    private void RemoveGoldBag(NetworkMessage msg)
    {
        RemoveGoldBag chg = msg.ReadMessage<RemoveGoldBag>();
        GoldBag goldB = gold.Find(x => x.id == chg.id);
        if(goldB!=null)
            goldB.DestroyThis();
    }

    /// <summary>
    /// Resposta a agafar or
    /// </summary>
    private void CatchGoldResp(NetworkMessage msg)
    {
        CatchGoldResp chg = msg.ReadMessage<CatchGoldResp>();
        if (chg.catched)
        {
            chat.SendMessageToChat("You've picked up " + chg.amount + " gold", Color.yellow);
        }
        else
        {
            chat.SendMessageToChat("You don't own this item", Color.yellow);
        }
    }

    /// <summary>
    /// Actualitza l'or actual
    /// </summary>
    private void CurrentGold(NetworkMessage msg)
    {
        CurrentGold chg = msg.ReadMessage<CurrentGold>();
        InventoryManager.instance.UpdateGold(chg.amount);
    }
    #endregion

    #endregion

    #region Methods

    void CheckConnected()//Comprovem si estem connectats al servidor
    {
        if (client.isConnected)
        {
            GameObject.Find("Canvas").GetComponent<LoginScript>().serverStatus.text = "Server status: ONLINE";
            CancelInvoke("CheckConnected");
            InvokeRepeating("IKeepAlive", 0, 5f);
        }
    }

    void IKeepAlive()//Ping cada X segons per a informar al servidor de que continuem tinguent connexió
    {
        try
        {
            if (!client.Send(AllNetworkPackets.KeepAlive, new KeepAlive()))
            {
                CancelInvoke("IKeepAlive");
                if (msgbxInstance != null)
                    Destroy(msgbxInstance);
                MessageBox("Server has shutted down. Game will close.", () => Application.Quit(), () => Application.Quit());
                myPlayer.playerController.agent.isStopped = true;
            }
        }
        catch { }
    }

    void AcceptFriendReq(SendFriendRequest req)//Acceptar ser amic
    {
        Destroy(msgbxInstance);
        client.Send(AllNetworkPackets.AcceptRequest, new AcceptRequest { id_from = req.id_from, id_to = req.id_to, accepted = true });
    }

    void DeclineFriendReq(SendFriendRequest req)//Declinar petició d'amistat
    {
        Destroy(msgbxInstance);
        client.Send(AllNetworkPackets.AcceptRequest, new AcceptRequest { id_from = req.id_from, id_to = req.id_to, accepted = false });
    }

    private void OnLevelWasLoaded(int level)//Al carregar el nivell 1 logejem el personatge
    {
        if (level == 1)
        {
            client.Send(AllNetworkPackets.ReqLoginCharacter, new RequestLoginCharacter { playername = charc });
        }
    }

    IEnumerator DelayedQuit()//Al sortir del joc enviem paquet per a desconectar-nos
    {
        client.Send(AllNetworkPackets.PlayerDisconnected, new PlayerDisconnected());
        yield return new WaitForSeconds(0.1f);
        client.Disconnect();
        allowQuitting = true;
        Application.Quit();
    }

    private void OnApplicationQuit()
    {
        if (!allowQuitting)
        {
            Application.CancelQuit();
            StartCoroutine(DelayedQuit());
        }
    }

    public void MessageBox(string msg, Action onOK, Action onCancel)
    {
        if (msgbxInstance == null)
        {
            msgbxInstance = Instantiate(messageBox, new Vector3(0, 0, 0), Quaternion.identity);
            msgbxInstance.GetComponent<RectTransform>().SetParent(GameObject.Find("Canvas").transform, false);
            msgbxInstance.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 130);
            msgbxInstance.GetComponent<MessageBoxController>().SetMessageBox(msg, onOK, onCancel);
        }
    }

    #endregion
}

public class Player
{
    public GameObject playerCharacter;
    private PlayerController _playerController;
    public PlayerController playerController
    {
        get
        {
            if (_playerController == null)
                _playerController = playerCharacter.GetComponent<PlayerController>();
            return _playerController;
        }
    }
}