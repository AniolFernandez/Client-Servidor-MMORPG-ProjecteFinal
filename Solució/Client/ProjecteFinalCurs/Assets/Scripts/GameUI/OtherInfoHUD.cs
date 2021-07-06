using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OtherInfoHUD : MonoBehaviour
{
    public SimpleHealthBar healthBar;//Vida target
    public SimpleHealthBar manaBar;//Mana target
    public Text playerDef;
    public int maxHP;
    public int maxMANA;
    private string othname;
    public Chat chat;
    public int id=0;
    public int type=0;
    public int lastHP=0;
    public GameObject whispBtn;
    public GameObject friendBtn;
    public GameObject target;
    public static OtherInfoHUD instance;
    private void Start()
    {
        GameObject.Find("NetworkManager").GetComponent<ConnectToServer>().otherHUD = this;
        this.gameObject.SetActive(false);
        instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))//Si apretem 'ESC' tanquem la info
            CloseInfo();
    }
    public void UpdateCharDef(int lv, string name, string char_class)
    {
        if (type == 0)//Si és un jugador, hem d'activar els botons d'afegir amic i whisper
        {
            friendBtn.SetActive(true);
            whispBtn.SetActive(true);
        }
        else//Si és un NPC, no s'han d'activar
        {
            friendBtn.SetActive(false);
            whispBtn.SetActive(false);
        }
        this.othname = name;
        playerDef.text = "Lv " + lv + ", " + name + ", " + char_class;
    }
    public void UpdateHP_MANA(int currHP, int currMANA)
    {
        healthBar.UpdateBar(currHP, maxHP);
        manaBar.UpdateBar(currMANA, maxMANA);
    }

    public void Whisper()//Obrim chat amb whisper preparat
    {
        chat.ActiveChat('/' + othname + ' ');
        LocalPlayer.Instance.DoNotMove();
    }

    public void AddFriend()//Enviem paquet d'afegir a amics
    {
        ConnectToServer.client.Send(AllNetworkPackets.SendFriendRequest, new SendFriendRequest { id_to = LocalPlayer.Instance.id_target });
        ConnectToServer.instance.MessageBox("Friend request sent.", () => Destroy(ConnectToServer.instance.msgbxInstance), () => Destroy(ConnectToServer.instance.msgbxInstance));
        LocalPlayer.Instance.DoNotMove();
    }

    public void CloseInfo()//Tanquem la info i perdem el target
    {
        this.gameObject.SetActive(false);
        switch (type)
        {
            case 0://Enviem al sv que volem deixar de rebre info del jugador
                ConnectToServer.client.Send(AllNetworkPackets.StopGettingInfoOfPlayer, new StopGettingInfoOfPlayer { playerID = LocalPlayer.Instance.id_target });
                break;
            case 1://Enviem al sv que volem deixar de rebre info de l'npc
                ConnectToServer.client.Send(AllNetworkPackets.StopGettingInfoOfNPC, new StopGettingInfoOfNPC { npc_id = id, npc_type = type });
                break;
        }
        LocalPlayer.Instance.id_target = 0;
        LocalPlayer.Instance.target_type = 0;
        LocalPlayer.Instance.DoNotMove();
        id =0;
        type=0;
        lastHP=0;
    }
}
