using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618
public class NetworkServerManager : MonoBehaviour
{
    private string DataBaseName = "mmorpg";
    private ServerConfig config;
    public GameObject PlayerPrefab;
    public GameObject Enemy1Prefab;
    public static List<ConnectedUser> connectedUsers = new List<ConnectedUser>();
    public static List<NPCInstance> allNPCs = new List<NPCInstance>();
    public static List<GoldBag> gBag = new List<GoldBag>();
    private List<SendFriendRequest> friendRequests = new List<SendFriendRequest>();

    private void Start()
    {
        DontDestroyOnLoad(this);
        Debug.Log("MMORPG SERVER - Aniol Fernàndez Cano");
        Debug.Log("Connecting to db...");
        if (ConnectToDB())
        {
            Debug.Log("Database connected!");
            InstantiateNPCS(NPCManagement.GetNPCInstances());
            Debug.Log("Setting up server...");
            /*
            ConnectionConfig Config = new ConnectionConfig();
            Config.AddChannel(QosType.Reliable);
            HostTopology Topology = new HostTopology(Config, 4096);
            NetworkServer.Configure(Topology);*/
            NetworkServer.Listen(config.ServerPort);
            NetworkServer.RegisterHandler(AllNetworkPackets.LoginMsg, LoginAccount);
            NetworkServer.RegisterHandler(AllNetworkPackets.MoveTo, MoveTo);
            NetworkServer.RegisterHandler(AllNetworkPackets.PlayerDisconnected, DisconnectPlayer);
            NetworkServer.RegisterHandler(AllNetworkPackets.RegisterAccount, RegisterAccount);
            NetworkServer.RegisterHandler(AllNetworkPackets.CreatePlayer, createPlayer);
            NetworkServer.RegisterHandler(AllNetworkPackets.DeletePlayer, DeleteCharacter);
            NetworkServer.RegisterHandler(AllNetworkPackets.ReqLoginCharacter, RequestLogin);
            NetworkServer.RegisterHandler(AllNetworkPackets.SendChatMessage, SendGeneralChat);
            NetworkServer.RegisterHandler(AllNetworkPackets.SendPrivateMessage, SendPrivateChat);
            NetworkServer.RegisterHandler(AllNetworkPackets.KeepAlive,  KeepAlive);
            NetworkServer.RegisterHandler(AllNetworkPackets.GetInfoOfPlayer, AddInfoListener);
            NetworkServer.RegisterHandler(AllNetworkPackets.StopGettingInfoOfPlayer, RemoveInfoListener);
            NetworkServer.RegisterHandler(AllNetworkPackets.GetInfoOfNPC, AddInfoListenerNPC);
            NetworkServer.RegisterHandler(AllNetworkPackets.StopGettingInfoOfNPC, RemoveInfoListenerNPC);
            NetworkServer.RegisterHandler(AllNetworkPackets.Atack, atack);
            NetworkServer.RegisterHandler(AllNetworkPackets.Dab, dab);
            NetworkServer.RegisterHandler(AllNetworkPackets.SendFriendRequest, SendFriendRequest);
            NetworkServer.RegisterHandler(AllNetworkPackets.AcceptRequest, AcceptRequest);
            NetworkServer.RegisterHandler(AllNetworkPackets.RemoveFriend, RemoveFriend);
            NetworkServer.RegisterHandler(AllNetworkPackets.CatchGold, CatchGold);
            NetworkServer.RegisterHandler(AllNetworkPackets.GotPlayer, GotPlayer);
            NetworkServer.RegisterHandler(AllNetworkPackets.ChangeFriendStatus, ChangeFriendStatus);
            InvokeRepeating("CheckConnectedClients", 0, 5);
            Debug.Log("SERVER STARTED SUCCESFULLY!");
        }
        else
        {
            Debug.Log("Failed to connect to databaes");
            Debug.Log("Exit on 5s");
            new WaitForSeconds(5);
            Application.Quit();
        }


    }

    /// <summary>
    /// Connexió a la DB
    /// </summary>
    private bool ConnectToDB()
    {
        config = ServerConfig.Load();
        bool connected = false;
        var dbCon = DatabaseConnection.Instance();
        dbCon.DB_IP = config.DataBaseIP;
        dbCon.DatabaseName = DataBaseName;
        dbCon.User = config.DataBaseUser;
        dbCon.Password = config.DataBasePassword;
        if (dbCon.IsConnect())
        {
            connected = true;
            dbCon.Close();
        }
        return connected;
    }

    /// <summary>
    /// Instancia tots els npcs de la bd al món
    /// </summary>
    private void InstantiateNPCS(List<NPCInstance> npcs)
    {
        Debug.Log("Instantiating NPCS...");
        foreach(NPCInstance npc in npcs)
        {
            npc.curr_hp = npc.max_hp;
            npc.curr_mana = npc.max_mana;
            switch (npc.type)
            {
                case 1://Nailer
                    npc.npc_go = Instantiate(Enemy1Prefab, new Vector3(npc.x, npc.y, npc.z), Quaternion.identity);
                    npc.controller = npc.npc_go.GetComponent<EnemyController>();
                    npc.controller.thisNPC = npc;
                    break;
            }
        }
        allNPCs = npcs;
        Debug.Log("NPCS Loaded");
    }

    /// <summary>
    /// Envia a TOTS els jugadors connectats algún missatge
    /// </summary>
    public static void SendToAll(short msgType, MessageBase msg)
    {
        foreach(ConnectedUser cp in connectedUsers)
        {
            cp.playerConnection.Send(msgType, msg);
        }
        
    }

    /// <summary>
    /// Comprova si algún jugador ha perdut la connexió i ha de ser eliminat
    /// </summary>
    void CheckConnectedClients()
    {
        List<ConnectedUser> toDc = new List<ConnectedUser>();
        foreach (ConnectedUser nc in connectedUsers)
        {
            try
            {
                if (Time.time - nc.playerConnection.lastMessageTime > 7f)
                    toDc.Add(nc);
            }
            catch (Exception e) { Debug.Log(e.Message); }
        }
        foreach(ConnectedUser nc in toDc)
            DisconnectPlayer(nc);
    }

    //Tots els paquets que executen una "ordre" sobre un jugador/compte, com per exemple moure's es comprova que qui envia el paquet sigui el mateix que ha iniciat la compta. Per tal d'evitar fake calls.

    #region Networking

    #region AccountManagement

    /// <summary>
    /// Paquet de ping per a mantindre l'usuari viu
    /// </summary>
    private void KeepAlive(NetworkMessage msg) { }

    /// <summary>
    /// Loggeja la comtpa si l'usuari no està ja conectat i ens entren bé les dades. Ens guardem la connexió des d'on s'ha realitzat la connexió per a tal de només donar permís a aquesta conx per a modificar la compta
    /// </summary>
    private void LoginAccount(NetworkMessage msg)
    {
        LoginMsg msglogin = msg.ReadMessage<LoginMsg>();
        if (connectedUsers.Find(x => x.account.username.Equals(msglogin.username)) == null)
        {
            ConnectedAccount tmp = AccountManagement.LoginAccount(msglogin.username, msglogin.password);
            if (tmp != null)
            {
                ConnectedUser newConnection = new ConnectedUser();
                newConnection.playerConnection = msg.conn;
                newConnection.playerCharacter = null;
                newConnection.account = tmp;
                connectedUsers.Add(newConnection);
                CharsAvailable chars = AccountManagement.GetAccChars(tmp.id);
                newConnection.playerConnection.Send(AllNetworkPackets.LoginAccepted, new LoginAccepted
                {
                    player_1_class = chars.player_1_class,
                    player_1_level = chars.player_1_level,
                    player_1_name = chars.player_1_name,
                    player_2_class = chars.player_2_class,
                    player_2_level = chars.player_2_level,
                    player_2_name = chars.player_2_name,
                    player_3_class = chars.player_3_class,
                    player_3_level = chars.player_3_level,
                    player_3_name = chars.player_3_name,
                });
            }
            else
            {
                msg.conn.Send(AllNetworkPackets.BadLogin, new BadLogin());
            }
        }
        else
        {
            msg.conn.Send(AllNetworkPackets.BadLogin, new BadLogin { bad = false });
        }
    }

    /// <summary>
    /// Crea un nou jugador. Comprova que no hagi superat el maxim/que no existeixi i retorna resultat. En cas d'afegir-lo, reenvia paquet de login per a refrescar la selecció de pj
    /// </summary>
    private void createPlayer(NetworkMessage msg)
    {
        CreatePlayer msglogin = msg.ReadMessage<CreatePlayer>();
        ConnectedUser tmp = connectedUsers.Find(x => msg.conn.connectionId == x.playerConnection.connectionId);
        if (tmp != null)
        {
            short result = AccountManagement.CreatePlayer(msglogin.char_name, msglogin.char_class, tmp.account.id);
            msg.conn.Send(AllNetworkPackets.CreatePlayerResp, new CreatePlayerResp { result = result });
            CharsAvailable chars = AccountManagement.GetAccChars(tmp.account.id);
            tmp.playerConnection.Send(AllNetworkPackets.LoginAccepted, new LoginAccepted
            {
                player_1_class = chars.player_1_class,
                player_1_level = chars.player_1_level,
                player_1_name = chars.player_1_name,
                player_2_class = chars.player_2_class,
                player_2_level = chars.player_2_level,
                player_2_name = chars.player_2_name,
                player_3_class = chars.player_3_class,
                player_3_level = chars.player_3_level,
                player_3_name = chars.player_3_name,
            });
        }
    }

    /// <summary>
    /// Paquet per a registrar nova compta. Retorna el resultat i el client ho processa
    /// </summary>
    private void RegisterAccount(NetworkMessage msg)
    {
        RegisterAccount register = msg.ReadMessage<RegisterAccount>();
        RegisterResponse response = new RegisterResponse();
        response.result = AccountManagement.RegisterAccount(register.username, register.password, register.email);
        msg.conn.Send(AllNetworkPackets.RegisterResponse, response);
    }

    /// <summary>
    /// "Elimina" el personatge que el client demani. Realment el que fem es posar-lo inactiu a la BD, reenviem un paquet de login al client per tal de que refresqui la seleccio de personatge
    /// </summary>
    private void DeleteCharacter(NetworkMessage msg)
    {

        DeletePlayer register = msg.ReadMessage<DeletePlayer>();
        ConnectedUser cu = connectedUsers.Find(x => x.playerConnection.connectionId == msg.conn.connectionId);
        if (cu != null)
        {
            AccountManagement.DeleteCharacter(register.playername, cu.account.id);
            CharsAvailable chars = AccountManagement.GetAccChars(cu.account.id);
            cu.playerConnection.Send(AllNetworkPackets.LoginAccepted, new LoginAccepted
            {
                player_1_class = chars.player_1_class,
                player_1_level = chars.player_1_level,
                player_1_name = chars.player_1_name,
                player_2_class = chars.player_2_class,
                player_2_level = chars.player_2_level,
                player_2_name = chars.player_2_name,
                player_3_class = chars.player_3_class,
                player_3_level = chars.player_3_level,
                player_3_name = chars.player_3_name,
            });
        }
    }

    #endregion

    #region Player

    /// <summary>
    /// Paquet enviat per el client en petició a iniciar un personatge. Instancia el jugador iniciat a el servidor i envia a el client les dades necs. per que ho fagi ell
    /// </summary>
    private void RequestLogin(NetworkMessage msg)
    {
        RequestLoginCharacter msgreq = msg.ReadMessage<RequestLoginCharacter>();
        ConnectedUser cu = connectedUsers.Find(x => msg.conn.connectionId == x.playerConnection.connectionId);
        if (cu != null)
        {
            float x, y, z;
            PlayerStructure newP = AccountManagement.loginCharacter(msgreq.playername, cu.account.id, out x, out y, out z);
            if (newP != null)
            {
                cu.playerCharacter = Instantiate(PlayerPrefab, new Vector3(x, y, z), Quaternion.identity);
                cu.playerController.clientConnection = msg.conn;
                cu.playerController.Player = newP;
                if (cu.playerController.Player.curr_hp == 0)
                    cu.playerController.Player.curr_hp = 1;
                //Afegeix jugador al client especifit
                cu.playerController.clientConnection.Send(AllNetworkPackets.LoginCharacter, new LoginCharacter
                {
                    class_id = newP.class_id,
                    id = newP.id,
                    level = newP.level,
                    playername = newP.playername,
                    xp = newP.xp,
                    x = x,
                    y = y,
                    z = z,
                    curr_hp = newP.curr_hp,
                    curr_mana = newP.curr_mana,
                    max_hp = newP.max_hp,
                    max_mana = newP.max_mana,
                    gold = newP.gold
                });
                //Envia els paquets d'afegir els amics a la llista
                foreach (Friend f in cu.playerController.Player.friends)
                {
                    ConnectedUser friend = connectedUsers.Find(c => c.playerController.Player.id == f.id);
                    f.online = friend != null;
                    if (f.online)
                    {
                        ChangeFriendStatus csf = new ChangeFriendStatus { id = cu.playerController.Player.id, online = true };
                        friend.playerConnection.Send(AllNetworkPackets.ChangeFriendStatus, csf);
                        cu.playerController.csn.Add(new ChangeStateNeeded { destinatary = friend.playerController, cfs = csf });
                    }
                    cu.playerConnection.Send(AllNetworkPackets.AddFriendToList, new AddFriendToList { id = f.id, name = f.name, online = f.online });
                }

            }
        }
    }

    /// <summary>
    /// Per asegurar-nos de que el client inicialitza l'usuari
    /// </summary>
    private void GotPlayer(NetworkMessage msg)
    {
        GotPlayer gp = msg.ReadMessage<GotPlayer>();
        ConnectedUser cu = connectedUsers.Find(x => x.playerConnection == msg.conn);
        if (cu != null)
        {
            cu.playerController.notAddeds.Remove(gp.id);
        }
    }

    /// <summary>
    /// Rep paquet de desconexió esperada. I desconecta el personatge (si està instanciat) i desconecta la compta
    /// </summary>
    private void DisconnectPlayer(NetworkMessage msg)
    {
        ConnectedUser ctmp = connectedUsers.Find(x => x.playerConnection.connectionId == msg.conn.connectionId);
        if (ctmp != null)
        {
            DisconnectPlayer(ctmp);
        }

    }

    /// <summary>
    /// Input de moviment d'un jugador. Realitzem els calculs al servidor i en els clients per a que no experimentin lag. Reenviem el paquet a tots els jugadors propers
    /// </summary>
    private void MoveTo(NetworkMessage msg)
    {
        MoveTo msglogin = msg.ReadMessage<MoveTo>();
        ConnectedUser ctmp = connectedUsers.Find(x => x.playerConnection.connectionId == msg.conn.connectionId);
        if (ctmp != null)
        {
            ctmp.playerController.pkt = msglogin;
        }
    }

    /// <summary>
    /// Afegeix listener de la info
    /// </summary>
    private void AddInfoListener(NetworkMessage msg)
    {
        GetInfoOfPlayer giop = msg.ReadMessage<GetInfoOfPlayer>();
        ConnectedUser cu = connectedUsers.Find(x => x.playerController.Player.id == giop.playerID);
        if (cu != null)
        {
            UpdatePlayerDef def = new UpdatePlayerDef();
            def.curr_hp = cu.playerController.Player.curr_hp;
            def.curr_mana = cu.playerController.Player.curr_mana;
            def.lvl = cu.playerController.Player.level;
            def.name = cu.playerController.Player.playername;
            def.classid = cu.playerController.Player.class_id;
            def.max_hp = cu.playerController.Player.max_hp;
            def.max_mana = cu.playerController.Player.max_mana;
            def.playerID = cu.playerController.Player.id;
            msg.conn.Send(AllNetworkPackets.UpdatePlayerDef, def);
            cu.playerController.infoListeners.Add(msg.conn);
        }
    }

    /// <summary>
    /// Treu listener de la info
    /// </summary>
    private void RemoveInfoListener(NetworkMessage msg)
    {
        StopGettingInfoOfPlayer giop = msg.ReadMessage<StopGettingInfoOfPlayer>();
        ConnectedUser cu = connectedUsers.Find(x => x.playerController.Player.id == giop.playerID);
        if (cu != null)
        {
            cu.playerController.infoListeners.Remove(msg.conn);
        }
    }

    /// <summary>
    /// Petició d'un usuari per a realitzar el seu atac
    /// </summary>
    private void atack(NetworkMessage msg)
    {
        Atack msgch = msg.ReadMessage<Atack>();
        ConnectedUser sender = connectedUsers.Find(x => x.playerConnection == msg.conn);
        NPCInstance npc = allNPCs.Find(x => x.id == msgch.npc_id && x.type == msgch.npc_type);
        if (sender != null && (npc != null || sender.playerController.Player.class_id == 1))
        {
            switch (sender.playerController.Player.class_id)//Depenent de la classe, fem un atac o un altre
            {
                case 3://Si es mag, fem l'atack de mag
                    sender.playerController.TryMagicianAtack(npc);
                    break;
                case 2://Arquer
                    sender.playerController.TryArcherAtack(npc);
                    break;
                case 1://Espadachin
                    sender.playerController.TrySwordsmanAtack();
                    break;
            }
        }
    }

    /// <summary>
    /// Petició d'un usuari per a realitzar el dab
    /// </summary>
    private void dab(NetworkMessage msg)
    {
        Dab msgch = msg.ReadMessage<Dab>();
        ConnectedUser sender = connectedUsers.Find(x => x.playerConnection == msg.conn);
        if (sender != null)
        {
            msgch.playerID = sender.playerController.Player.id;
            sender.playerController.dab(msgch);
        }
    }

    #endregion

    #region Chat
    /// <summary>
    /// Missatge provinent del chat general s'envia a tots els jugadors propers al jugador que l'ha enviat
    /// </summary>
    private void SendGeneralChat(NetworkMessage msg)
    {
        SendChatMessage msgch = msg.ReadMessage<SendChatMessage>();
        if (msgch.message.Trim() != "")
        {
            ConnectedUser sender = connectedUsers.Find(x => x.playerConnection == msg.conn);
            if (sender != null)
            {
                msgch.message = "<" + sender.playerController.Player.playername + ">: " + msgch.message;
                foreach (PlayerController pc in sender.playerController.nearbyUser)
                    pc.clientConnection.Send(AllNetworkPackets.SendChatMessage, msgch);
                msg.conn.Send(AllNetworkPackets.SendChatMessage, msgch);
            }
        }
    }

    /// <summary>
    /// Missatge privat. Només el rep el destinatari, estigui o no aprop. Ha d'estar connectat
    /// </summary>
    private void SendPrivateChat(NetworkMessage msg)
    {
        SendPrivateMessage msgch = msg.ReadMessage<SendPrivateMessage>();
        if (msgch.message.Trim() != "")
        {
            ConnectedUser sender = connectedUsers.Find(x => x.playerConnection == msg.conn);
            if (sender != null)
            {
                ConnectedUser destinatary = connectedUsers.Find(x => x.playerController.Player.playername == msgch.destinatary);
                if (destinatary != null)
                {
                    msgch.message = "<" + sender.playerController.Player.playername + ">: " + msgch.message;
                    msg.conn.Send(AllNetworkPackets.SendPrivateMessage, msgch);
                    destinatary.playerConnection.Send(AllNetworkPackets.SendPrivateMessage, msgch);
                }
                else
                {
                    msgch.message = "Destinatary not connected!";
                    msg.conn.Send(AllNetworkPackets.SendPrivateMessage, msgch);
                }


            }
        }
    }
    #endregion

    #region NPC

    /// <summary>
    /// Afegeix listener de la info
    /// </summary>
    private void AddInfoListenerNPC(NetworkMessage msg)
    {
        GetInfoOfNPC gion = msg.ReadMessage<GetInfoOfNPC>();
        NPCInstance npc = allNPCs.Find(x => x.id == gion.npc_id && x.type == gion.npc_type);
        if (npc != null)
        {
            UpdateNPCDef def = new UpdateNPCDef();
            def.npc_id = npc.id;
            def.curr_hp = npc.curr_hp;
            def.curr_mana = npc.curr_mana;
            def.lvl = npc.level;
            def.name = npc.name;
            def.type = npc.type;
            def.max_hp = npc.max_hp;
            def.max_mana = npc.max_mana;
            msg.conn.Send(AllNetworkPackets.UpdateNPCDef, def);
            npc.infoListeners.Add(msg.conn);
        }
    }

    /// <summary>
    /// Treu listener de la info
    /// </summary>
    private void RemoveInfoListenerNPC(NetworkMessage msg)
    {
        StopGettingInfoOfNPC gion = msg.ReadMessage<StopGettingInfoOfNPC>();
        NPCInstance npc = allNPCs.Find(x => x.id == gion.npc_id && x.type == gion.npc_type);
        if (npc != null)
        {
            npc.infoListeners.Remove(msg.conn);
        }
    }

    #endregion

    #region Friendzone
    private void ChangeFriendStatus(NetworkMessage msg)
    {
        ChangeFriendStatus cfs = msg.ReadMessage<ChangeFriendStatus>();
        ConnectedUser cu = connectedUsers.Find(x => x.playerController.Player.id == cfs.id);
        ConnectedUser cuDest = connectedUsers.Find(x => x.playerConnection == msg.conn);
        if (cuDest != null && cu != null)//Elimina paquet de m'haig de connectar
        {
            cu.playerController.csn.RemoveAll(x => x.destinatary.Player.id == cuDest.playerController.Player.id);
        }
        else if (cuDest != null && cu == null && !cfs.online)//Elimina paquet de m'haig de desconectar
        {
            cuDest.playerController.csn.RemoveAll(x => x.destinatary.Player.id == cuDest.playerController.Player.id);
        }
    }

    /// <summary>
    /// Envia petició d'amistat
    /// </summary>
    private void SendFriendRequest(NetworkMessage msg)
    {
        SendFriendRequest msgc = msg.ReadMessage<SendFriendRequest>();
        //Primera petició
        ConnectedUser sender = connectedUsers.Find(x => x.playerConnection == msg.conn);
        ConnectedUser dest = connectedUsers.Find(x => x.playerController.Player.id == msgc.id_to);
        if (sender != null && dest != null && !AccountManagement.AlreadyFriends(sender.playerController.Player.id, dest.playerController.Player.id))
        {
            msgc.name_from = sender.playerController.Player.playername;
            msgc.id_from = sender.playerController.Player.id;
            dest.playerConnection.Send(AllNetworkPackets.SendFriendRequest, msgc);
            friendRequests.Add(msgc);
        }
        
    }

    /// <summary>
    /// Accepta o declina la petició d'amistat
    /// </summary>
    private void AcceptRequest(NetworkMessage msg)
    {
        AcceptRequest msgc = msg.ReadMessage<AcceptRequest>();
        SendFriendRequest fr = friendRequests.Find(x => x.id_from == msgc.id_from && x.id_to == msgc.id_to);
        if (fr != null)
        {
            friendRequests.Remove(fr);
            OkeyFriend ok = new OkeyFriend();
            if (msgc.accepted)//Ens ha acceptat la solicitud
            {
                if (AccountManagement.AddFriend(msgc.id_from, msgc.id_to) == 0)
                    ok.accepted = true;
                else
                    ok = null;
            }
            else
                ok.accepted = false;
            //Si estan conectats els hi enviem resposta
            if (ok != null)
            {
                ConnectedUser user1 = connectedUsers.Find(x => x.playerController.Player.id == msgc.id_from);
                ConnectedUser user2 = connectedUsers.Find(x => x.playerController.Player.id == msgc.id_to);
                if (user1 != null && user2 != null)
                {//Envia l'estat i afegeix a la llista el nou amic
                    //Envia el resultat
                    user1.playerConnection.Send(AllNetworkPackets.OkeyFriend, ok);
                    user2.playerConnection.Send(AllNetworkPackets.OkeyFriend, ok);
                    if (ok.accepted)
                    {
                        Friend newFriend = new Friend();
                        newFriend.id = user2.playerController.Player.id;
                        newFriend.name = user2.playerController.Player.playername;
                        newFriend.online = true;
                        user1.playerConnection.Send(AllNetworkPackets.AddFriendToList, new AddFriendToList { id = user2.playerController.Player.id, name = user2.playerController.Player.playername, online = true });
                        user1.playerController.Player.friends.Add(newFriend);//Afegeix l'amic a la instancia del servidor
                        //Usuari2
                        newFriend = new Friend();
                        newFriend.id = user1.playerController.Player.id;
                        newFriend.name = user1.playerController.Player.playername;
                        newFriend.online = true;
                        user2.playerConnection.Send(AllNetworkPackets.AddFriendToList, new AddFriendToList { id = user1.playerController.Player.id, name = user1.playerController.Player.playername, online = true });
                        user2.playerController.Player.friends.Add(newFriend);//Afegeix l'amic a la instancia del servidor
                    }
                }  
            }
        }    
    }

    /// <summary>
    /// Elimina la relació d'amistat
    /// </summary>
    private void RemoveFriend(NetworkMessage msg)
    {
        RemoveFriend rf = msg.ReadMessage<RemoveFriend>();
        ConnectedUser cu = connectedUsers.Find(x => x.playerConnection == msg.conn);
        if (cu != null)
        {
            Friend removed = cu.playerController.Player.friends.Find(x => x.id == rf.id);
            if (removed != null)
            {//Si hi ha per eliminar, eliminem les dades a la BD i si els jugadors estan connectats els hi notifiquem que han perdut un amic
                AccountManagement.RemoveFriends(cu.playerController.Player.id, rf.id);
                cu.playerController.Player.friends.Remove(removed);
                cu.playerConnection.Send(AllNetworkPackets.RemoveFriend, rf);
                ConnectedUser other = connectedUsers.Find(x => x.playerController.Player.id == rf.id);
                if (other != null)
                {
                    Friend toRmv = other.playerController.Player.friends.Find(x => x.id == cu.playerController.Player.id);
                    if(toRmv!=null)
                    {
                        other.playerController.Player.friends.Remove(toRmv);
                        other.playerConnection.Send(AllNetworkPackets.RemoveFriend, new RemoveFriend { id = cu.playerController.Player.id });
                    }  
                }           
            }     
        }
        
    }
    #endregion

    #region Items
    /// <summary>
    /// Paquet d'agafar or, comprova que qui l'agafi és el propietari
    /// </summary>
    private void CatchGold(NetworkMessage msg)
    {
        try
        {
            CatchGold gldmg = msg.ReadMessage<CatchGold>();
            GoldBag g = gBag.Find(x => x.id == gldmg.id);
            ConnectedUser sender = connectedUsers.Find(x => x.playerConnection == msg.conn);
            ConnectedUser cu = connectedUsers.Find(x => x.playerController.Player.id == g.pcAllowed.Player.id);
            if (sender.playerController.Player.id == cu.playerController.Player.id)
            {
                //Si existeix l'or i el jugador està a la distancia pertinent, li afegim l'or
                if (g != null && cu != null && Vector3.Distance(g.gameObject.transform.position, cu.playerCharacter.transform.position) < 3)
                {
                    CatchGoldResp cgr = new CatchGoldResp();
                    cgr.amount = g.amount;
                    cgr.catched = true;
                    g.CatchGold();
                    cu.playerConnection.Send(AllNetworkPackets.CatchGoldResp, cgr);
                    CurrentGold cg = new CurrentGold();
                    cg.amount = cu.playerController.Player.gold;
                    cu.playerConnection.Send(AllNetworkPackets.CurrentGold, cg);
                }
            }
            else if (g != null && cu != null && Vector3.Distance(g.gameObject.transform.position, sender.playerCharacter.transform.position) < 3)//no es el propietari de l'or
            {
                CatchGoldResp cgr = new CatchGoldResp();
                cgr.catched = false;
                sender.playerConnection.Send(AllNetworkPackets.CatchGoldResp, cgr);
            }
        }
        catch { }

    }
    #endregion

    #endregion

    private void DisconnectPlayer(ConnectedUser cu)//Pot ser cridat per el checker de desconexions
    {
        PlayerDisconnected dc = new PlayerDisconnected();
        connectedUsers.Remove(cu);
        if (cu.playerController != null)
        {
            dc.playerId = cu.playerController.Player.id;
            cu.playerController.SavePlayerData();
            foreach (PlayerController pc in cu.playerController.nearbyUser)
                pc.clientConnection.Send(AllNetworkPackets.PlayerDisconnected, dc);
            Destroy(cu.playerCharacter);
            AccountManagement.UpdateLastLogin(cu.account);
            //Els hi enviem als amics que hi ha connectats que es desconecta
            ChangeFriendStatus chg = new ChangeFriendStatus();
            chg.id = cu.playerController.Player.id;
            chg.online = false;
            foreach (Friend f in cu.playerController.Player.friends)
            {
                ConnectedUser frnd = connectedUsers.Find(x => x.playerController.Player.id == f.id);
                if (frnd != null)
                {
                    frnd.playerController.Player.friends.RemoveAll(x => x.id == dc.playerId);
                    frnd.playerConnection.Send(AllNetworkPackets.ChangeFriendStatus, chg);
                    frnd.playerController.csn.Add(new ChangeStateNeeded { destinatary = frnd.playerController, cfs = chg });
                }
                    
            }
        }
        if (cu.playerCharacter != null)
        {
            cu.playerConnection.Disconnect();
            cu.playerConnection.Dispose();
        }

    }

    private void OnApplicationQuit()
    {
        foreach(ConnectedUser cu in connectedUsers)
        {
            try
            {
                cu.playerController.SavePlayerData();
            }
            catch(Exception e) { Debug.Log(e.Message); } 
        }
    }
}

public class ConnectedUser
{
    public NetworkConnection playerConnection;
    public ConnectedAccount account;
    public GameObject playerCharacter;
    private PlayerController _playerController;
    public PlayerController playerController
    {
        get {
            if (_playerController == null)
            {
                try
                {
                    _playerController = playerCharacter.GetComponent<PlayerController>();
                }
                catch
                {
                    _playerController = null;
                }
                    
            }
            return _playerController;
        }
    }
}