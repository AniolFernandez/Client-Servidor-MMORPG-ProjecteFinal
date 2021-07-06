using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
#pragma warning disable CS0618
public class EnemyNearbyUnits : MonoBehaviour
{
    public List<NetworkConnection> nearbyUser = new List<NetworkConnection>();
    public EnemyController npc;

    private void Start()
    {
        npc = transform.parent.GetComponent<EnemyController>();
    }
    /*
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {//Instanciació als clients
            PlayerController otherConn = other.gameObject.GetComponent<PlayerController>();
            otherConn.clientConnection.Send(AllNetworkPackets.InstantiateNPC, new InstantiateNPC { npc_id = npc.thisNPC.id, npc_type = npc.thisNPC.type, x = npc.gameObject.transform.position.x, y = npc.gameObject.transform.position.y, z = npc.gameObject.transform.position.z });
            otherConn.clientConnection.Send(AllNetworkPackets.MoveNPC, new MoveNPC { npc_id = npc.thisNPC.id, npc_type = npc.thisNPC.type, x = npc.lastDir.x, y = npc.lastDir.y, z = npc.lastDir.z });
            nearbyUser.Add(otherConn.clientConnection);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {//Dispose als clients
            PlayerController otherConn = other.gameObject.GetComponent<PlayerController>();
            Hitter hitter= npc.hitters.Find(x => x.hitter.Player.id == otherConn.Player.id);
            if (hitter != null)//Si ens allunyem i l'haviem picat, perdem dret a l'experiencia que haviem aconseguit, però no la donem a ningú. és perd
                npc.hitters.Remove(hitter);
            otherConn.clientConnection.Send(AllNetworkPackets.DisposeNPC, new DisposeNPC { npc_id = npc.thisNPC.id, npc_type = 1 });
            nearbyUser.Remove(otherConn.clientConnection);
        }
    }*/
}
