using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTargetDetector : MonoBehaviour
{
    public EnemyController main;
    private List<Transform> possibleTargets = new List<Transform>();
    private PlayerController pc = null;

    private void Update()
    {
        if (pc != null && pc.Player.curr_hp == 0)
            SetNewNearestTarget();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && other.transform != null)
        {//afegeix el jugador i li envia la direcció cap a on està anant
            if (main.target == null)
            {
                main.SetTarget(other.transform);
                pc = other.GetComponent<PlayerController>();
            }
                
            possibleTargets.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player" && other.transform != null)
        {//Treu el jugador de possibles targets i comprova si es el target actual, si ho és, busca un altre target, si no n'hi ha torna al a base
            possibleTargets.Remove(other.transform);
            if (main.target == other.transform)
            {
                if(possibleTargets.Count >= 1)
                {
                    main.SetTarget(possibleTargets[0]);
                    pc = possibleTargets[0].GetComponent<PlayerController>();
                }
                else
                {
                    main.SetTarget(null);
                    pc = null;
                }
                    
            }
            
        }
    }

    public void SetNewNearestTarget()//Perdem el target actual(deu haver mort) fem que el seguent target sigui el mes proper o null i torni a la base
    {
        possibleTargets.Remove(main.target);
        Transform nearest = null;
        float minDist = float.MaxValue;
        foreach(Transform t in possibleTargets)
        {
            if (t!=null && Vector3.Distance(this.transform.position, t.position) < minDist)
                nearest = t;
        }
        if (nearest != null)
            pc = nearest.GetComponent<PlayerController>();
        else
            pc = null;
        main.SetTarget(nearest);
    }
}
