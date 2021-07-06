using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamageApplyer : MonoBehaviour
{
    public EnemyTargetDetector etd;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {//afegeix el jugador i li envia la direcció cap a on està anant
            PlayerController otherConn = other.gameObject.GetComponent<PlayerController>();
            if (otherConn.ApplyDamage(20))//si matem al jugador, fem que l'enemic vagi cap al seguent jugador més proper
            {
                etd.SetNewNearestTarget();
            }
        }
    }
}
