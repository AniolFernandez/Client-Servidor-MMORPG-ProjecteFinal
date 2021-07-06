using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
#pragma warning disable CS0618
public class NearbyUnitsDetector : MonoBehaviour
{
    PlayerController pcParent;
    private bool ReeCheck = true;

    private void Start()
    {
        pcParent = gameObject.GetComponentInParent<PlayerController>();
        Invoke("StopReCheck", 15f);
    }

    private void StopReCheck()
    {
        ReeCheck = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (ReeCheck)
        {
            if (other.tag == "Player")
            {//afegeix el jugador i li envia la direcció cap a on està anant
                PlayerController otherConn = other.gameObject.GetComponent<PlayerController>();
                if (pcParent.nearbyUser.Find(x => x.Player.id == otherConn.Player.id) == null)
                {
                    Debug.Log(pcParent.Player.playername + " and " + otherConn.Player.playername + " added by recheck");
                    ForceAdd(otherConn);
                    otherConn.GetComponentInChildren<NearbyUnitsDetector>().ForceAdd(pcParent);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {//afegeix el jugador i li envia la direcció cap a on està anant
            PlayerController otherConn = other.gameObject.GetComponent<PlayerController>();
            if(pcParent.nearbyUser.Find(x => x.Player.id == otherConn.Player.id) == null)
            {
                ForceAdd(otherConn);
                otherConn.GetComponentInChildren<NearbyUnitsDetector>().ForceAdd(pcParent);
            }
        }
        else if(other.tag == "Enemy")
        {//Instanciació als clients
            EnemyNearbyUnits enu = other.GetComponentInChildren<EnemyNearbyUnits>();
            pcParent.clientConnection.Send(AllNetworkPackets.InstantiateNPC, new InstantiateNPC { npc_id = enu.npc.thisNPC.id, npc_type = enu.npc.thisNPC.type, x = enu.npc.gameObject.transform.position.x, y = enu.npc.gameObject.transform.position.y, z = enu.npc.gameObject.transform.position.z });
            pcParent.clientConnection.Send(AllNetworkPackets.MoveNPC, new MoveNPC { npc_id = enu.npc.thisNPC.id, npc_type = enu.npc.thisNPC.type, x = enu.npc.lastDir.x, y = enu.npc.lastDir.y, z = enu.npc.lastDir.z });
            enu.nearbyUser.Add(pcParent.clientConnection);
            pcParent.nearbyEnemy.Add(enu);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {//Envia que s'ha desconectat a els clients. aixi desaparexen de la seva vista
            PlayerController otherConn = other.gameObject.GetComponent<PlayerController>();
            pcParent.clientConnection.Send(AllNetworkPackets.PlayerDisconnected, new PlayerDisconnected { playerId = otherConn.Player.id });
            pcParent.nearbyUser.Remove(otherConn);
        }
        else if (other.tag == "Enemy")
        {
            EnemyNearbyUnits enu = other.GetComponentInChildren<EnemyNearbyUnits>();
            Hitter hitter = enu.npc.hitters.Find(x => x.hitter.Player.id == pcParent.Player.id);
            if (hitter != null)//Si ens allunyem i l'haviem picat, perdem dret a l'experiencia que haviem aconseguit, però no la donem a ningú. és perd
                enu.npc.hitters.Remove(hitter);
            pcParent.clientConnection.Send(AllNetworkPackets.DisposeNPC, new DisposeNPC { npc_id = enu.npc.thisNPC.id, npc_type = 1 });
            enu.nearbyUser.Remove(pcParent.clientConnection);
            pcParent.nearbyEnemy.Remove(enu);
        }
    }

    public void ForceAdd(PlayerController pc)
    {
        pcParent.notAddeds.Add(pc.Player.id);
        pcParent.clientConnection.Send(AllNetworkPackets.PlayerConnected, new PlayerConnected { playerId = pc.Player.id, playername = pc.Player.playername, x = pc.gameObject.transform.position.x, y = pc.gameObject.transform.position.y, z = pc.gameObject.transform.position.z, classid = pc.Player.class_id });
        if (pc.destPosition != new Vector3(0, 0, 0))
            pcParent.clientConnection.Send(AllNetworkPackets.MoveTo, new MoveTo { playerId = pc.Player.id, x = pc.destPosition.x, y = pc.destPosition.y, z = pc.destPosition.z });
        pcParent.nearbyUser.Add(pc);
    }
}
