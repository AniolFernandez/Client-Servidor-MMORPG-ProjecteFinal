using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendComponent : MonoBehaviour
{

    private bool _online = false;
    public bool Online//L'accesor s'encarrega de canviar l'estat del text i el color d'online
    {
        get
        {
            return _online;
        }
        set
        {
            if (value)
            {
                playerName.fontStyle = FontStyle.Bold;
                status.sprite = online;
                Color a = playerName.color;
                a.a = 0.7f;
                playerName.color = a;
            }
            else
            {
                playerName.fontStyle = FontStyle.Normal;
                status.sprite = offline;
                Color a = playerName.color;
                a.a = 1f;
                playerName.color = a;
            }
            _online = value;
        }
    }
    public Text playerName;
    public Image status;
    public Sprite online;
    public Sprite offline;

    public int id;
    public string playername;
    public void SetFriend(int id, string name, bool online)//Cridat per a afegir un nou amic.
    {
        this.id = id;
        this.playername = name;
        playerName.text = "   " + playername;
        Online = online;
    }

    public void RemoveFriend()//Cridat des del botó d'eliminar amics. Salta el popup de si realment volem eliminar-lo
    {
        LocalPlayer.Instance.DoNotMove();
        ConnectToServer.instance.MessageBox("Do you really wish to end with your relationship?", () => Remove(), ()=>Destroy(ConnectToServer.instance.msgbxInstance));
    }

    public void Remove()//Al fer click a OK en el pop up d'eliminar, eliminem l'amic
    {
        ConnectToServer.client.Send(AllNetworkPackets.RemoveFriend, new RemoveFriend { id = id });
        Destroy(ConnectToServer.instance.msgbxInstance);
    }

    public void Whisper()//Si cliquem en el nom de l'amic i l'amic està online, obrim el chat amb el whisper
    {
        if (Online)
        {
            Chat.instance.ActiveChat('/' + playername + ' ');
            LocalPlayer.Instance.DoNotMove();
        }

    }
}
