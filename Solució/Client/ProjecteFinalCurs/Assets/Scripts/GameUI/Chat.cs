using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Chat : MonoBehaviour
{
    private List<Message> messageList = new List<Message>();//Llista de missatges que té el xat
    public GameObject chatPanel, textBox;
    public InputField chatInput;
    public Image chatBg;
    public static Chat instance;
    public static bool chatOpen = false;
    private void Start()
    {
        instance = this;
        GameObject.Find("NetworkManager").GetComponent<ConnectToServer>().chat = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))//Si premem enter s'obra el chat. Si ja està obert, s'envia el msg
        {
            if (!chatInput.IsActive())
            {
                ActiveChat(); 
            }
            if (chatInput.text != "")
            {
                //Standard input
                if (chatInput.text.Length <= 50)//Només deixem enviar fins a 50 caràcters
                {
                    if (chatInput.text.StartsWith("/"))//Si el text comença pel caràcter '/' l'entrada es una comanda de whisper
                    {
                        string[] tmp=chatInput.text.Split(' ');
                        SendPrivateMessage prv = new SendPrivateMessage();
                        prv.destinatary = tmp[0].Remove(0,1);
                        prv.message = CommandToText(tmp);
                        ConnectToServer.client.Send(AllNetworkPackets.SendPrivateMessage, prv);//Envia al servidor el msg privat
                    }
                    else//És un missatge ordinari
                        ConnectToServer.client.Send(AllNetworkPackets.SendChatMessage, new SendChatMessage { message = chatInput.text });
                }    
                chatInput.text = "";
                DisableChat();
            }
        }
        
    }

    private string CommandToText(string[] txt)//Transforma l'input del text en una comanda 
    {
        string msg = "";
        for(int i = 1; i < txt.Length; i++)
        {
            msg += txt[i] + " ";
        }
        return msg;
    }

    public void ActiveChat()//Criat per a activar el chat
    {
        chatInput.gameObject.SetActive(true);
        chatInput.Select();
        chatInput.ActivateInputField();
        Color a = chatBg.color;
        a.a = 0.3f;
        chatBg.color = a;
        chatOpen = true;
    }

    public void ActiveChat(string input)//Cridat per a activar el chat amb un text ja escrit (Utilitzat pel whisper)
    {
        chatInput.text = input;
        ActiveChat();
        chatInput.MoveTextEnd(true);//Al utilitzar aquesta funció, el text queda "seleccionat", he intentat treure-ho però no hi ha manera
        chatOpen = true;
    }

    public void DisableChat()//Cridat per a desseleccionar el chat. 
    {
        chatInput.gameObject.SetActive(false);
        chatInput.Select();
        Color a = chatBg.color;
        a.a = 0.2f;
        chatBg.color = a;
        chatInput.OnDeselect(new BaseEventData(EventSystem.current));
        chatOpen = false;
    }

    public void SendMessageToChat(string text, Color chatColor)//Afegeix un missatge al chat.
    {
        if (messageList.Count > 100)//Si arribem al limit de msg, eliminem el primer que hagi arribat
        {
            Destroy(messageList[0].textObject.gameObject);
            messageList.RemoveAt(0);
        }
            
        Message newMessage = new Message();
        newMessage.text = text;
        GameObject newChatText = Instantiate(textBox, chatPanel.transform);
        newMessage.textObject = newChatText.GetComponent<Text>();
        newMessage.textObject.text = newMessage.text;
        newMessage.textObject.color = chatColor;
        messageList.Add(newMessage);
    }
}

public class Message//Missatge del chat
{
    public string text;
    public Text textObject;
}
