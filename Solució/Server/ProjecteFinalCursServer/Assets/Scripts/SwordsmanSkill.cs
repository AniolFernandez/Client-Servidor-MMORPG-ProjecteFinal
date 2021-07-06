using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordsmanSkill : MonoBehaviour
{
    public PlayerController player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            other.gameObject.GetComponent<EnemyController>().DoDamage(player.Player.damage,player);
        }
    }
}
