using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FriendContainer : MonoBehaviour
{
    List<FriendComponent> list = new List<FriendComponent>();
    public GameObject friendPrefab;
    public static FriendContainer instance;

    private void Start()
    {
        instance = this;
    }

    public void AddFriendToList(int id, string name, bool online)//Afegeix un component FriendComponent a la llista d'amics. Cridat just al connectar-se. Un cop per cada amic
    {
        FriendComponent newFriend = Instantiate(friendPrefab, this.transform).GetComponent<FriendComponent>();
        newFriend.SetFriend(id, name, online);
        list.Add(newFriend);
        SortList();//Ordena la llista per
    }

    public bool FriendAlreadyChanged(int id, bool status)
    {
        FriendComponent friend = list.Find(x => x.id == id);
        return friend != null && friend.Online==status;

    }

    public void RemoveFriend(int id)
    {
        FriendComponent friend = list.Find(x => x.id == id);
        if (friend != null)//Si tenim l'amic l'eliminem
        {
            list.Remove(friend);
            Destroy(friend.gameObject);
            SortList();
        }
    }

    public string ChangeFriendStatus(int id, bool online)
    {
        FriendComponent friend = list.Find(x => x.id == id);
        if (friend != null)//Si tenim l'amic li canviem l'estat
        {
            friend.Online = online;
            SortList();//Al canviar l'estat hem de reordenar la llista. Onlines primer, ordenat alfabèticament
            return friend.playername;
        }
        return "";
    }

    /// <summary>
    /// Ordena la llista d'amics, ordenats alfabèticament. Primer online
    /// </summary>
    public void SortList()
    {
        // Array.Sort(list.ToArray(), (x, y) => String.Compare(x.playername, y.playername));  <- vell
        //--------------------------------------------------------------------------------------------
        //Ordenem la llista que tenim
        list = list.OrderByDescending(c => c.Online).ThenBy(c => c.playername).ToList();
        //Movem els objectes de text segons la llista ordenada
        int index = 0;
        foreach (FriendComponent t in list)
        {
            t.transform.SetSiblingIndex(index);
            index++;
        }
    }
}
