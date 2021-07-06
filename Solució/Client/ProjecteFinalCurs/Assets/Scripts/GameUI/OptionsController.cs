using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsController : MonoBehaviour
{
    //Inventari
    public GameObject Inventari;
    private InventoryManager invref;
    //Llista d'amics
    public GameObject Amics;
    private FriendMenu amicref;
    //MessageBox
    public GameObject messageBox;
    GameObject msgbxInstance;

    private void Start()
    {
        invref = Inventari.GetComponent<InventoryManager>();
        amicref = Amics.GetComponent<FriendMenu>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && !Chat.chatOpen)//Si el chat no està obert, al apretar I obrim/tanquem inventari
            ObrirInventari();
        if (Input.GetKeyDown(KeyCode.N) && !Chat.chatOpen)//Si el chat no està obert, al apretar N obrim/tanquem llista d'amics
            ObrirLlistaAmics();
    }
    public void ObrirInventari()//Tanca/obra inventari
    {
        if (Inventari.activeSelf)
        {
            invref.Close();
        }
        else
        {
            Inventari.SetActive(true);  
        }
        LocalPlayer.Instance.DoNotMove();
    }

    public void ObrirLlistaAmics()//Tanca/obra llista d'amics
    {
        if (Amics.activeSelf)
        {
            amicref.Close();
        }
        else
        {
            Amics.SetActive(true);
        }
        LocalPlayer.Instance.DoNotMove();
    }

    public void Sortir()//Per al boto de sortir, cridem al popup de si realment vol sortir
    {
        MessageBox("Do you really want to exit?", () => Application.Quit(), () => Destroy(msgbxInstance));
        LocalPlayer.Instance.DoNotMove();
    }
    private void MessageBox(string msg, Action onOK, Action onCancel)
    {
        
        msgbxInstance = Instantiate(messageBox, new Vector3(0, 0, 0), Quaternion.identity);
        msgbxInstance.GetComponent<RectTransform>().SetParent(GameObject.Find("Canvas").transform, false);
        msgbxInstance.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 130);
        msgbxInstance.GetComponent<MessageBoxController>().SetMessageBox(msg, onOK, onCancel);
    }
}
