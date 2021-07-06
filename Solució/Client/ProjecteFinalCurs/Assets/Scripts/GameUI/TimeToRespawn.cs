using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TimeToRespawn : MonoBehaviour
{
    int seconds = 0;
    private Text scndstext;
    // Start is called before the first frame update
    void Start()
    {
        scndstext = this.GetComponent<Text>();
        GameObject.Find("NetworkManager").GetComponent<ConnectToServer>().timeRspwn = this;
        this.gameObject.SetActive(false);
    }

    public void SetRespawn(int seconds)//Activem els segons de respawn i els anem decrementant per a que poguem visualitzar quan reapareixerem
    {
        this.seconds = seconds+1;
        this.gameObject.SetActive(true);
        InvokeRepeating("Second",0f,1f);
    }

    public void Second()//Cada segon actualitzem el text 
    {
        if (seconds > 1)
        {
            seconds--;
            scndstext.text = seconds.ToString();
        }
        else//Si arribem al final desactivem l'objecte
        {
            CancelInvoke("Second");
            this.gameObject.SetActive(false);
        }
            
    }
}
