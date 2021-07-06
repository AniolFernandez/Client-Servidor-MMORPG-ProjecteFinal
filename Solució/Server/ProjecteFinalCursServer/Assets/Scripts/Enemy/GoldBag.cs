using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
#pragma warning disable CS0618
public class GoldBag : MonoBehaviour
{
    public float id;
    public float amount;
    public PlayerController pcAllowed;
    List<NetworkConnection> players;
    private void Start()
    {
        Invoke("DestroyThis", 60f);
    }

    /// <summary>
    /// Inicialitzem la bossa d'or
    /// </summary>
    public void SetBagOfGold(float goldAmount, float id, PlayerController pc, List<NetworkConnection> players)
    {
        this.id = id;
        this.amount = goldAmount;
        pcAllowed = pc;
        AddGoldBag agb = new AddGoldBag();
        agb.id = id;
        this.players = players;
        agb.amount = goldAmount;
        agb.x = this.transform.transform.position.x;
        agb.y = this.transform.transform.position.y;
        agb.z = this.transform.transform.position.z;
        foreach (NetworkConnection p in players)
        {
            p.Send(AllNetworkPackets.AddGoldBag, agb);
        }
        NetworkServerManager.gBag.Add(this);
    }

    /// <summary>
    /// Afegim l'or a qui ha matat l'enemic si l'agafa
    /// </summary>
    public void CatchGold()
    {
        pcAllowed.Player.gold += amount;
        foreach (NetworkConnection p in players)
        {
            p.Send(AllNetworkPackets.RemoveGoldBag, new RemoveGoldBag { id = id });
        }
        DestroyThis();
    }


    private void DestroyThis()
    {
        if (this.gameObject != null)
        {
            NetworkServerManager.gBag.Remove(this);
            Destroy(this.gameObject);
        }
  
    }
}
