using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    public SimpleHealthBar healthBar;
    public SimpleHealthBar manaBar;
    public SimpleHealthBar xpBar;
    public Text playerDef;
    public int maxHP;
    public int maxMANA;
       
    private void Start()
    {
        GameObject.Find("NetworkManager").GetComponent<ConnectToServer>().playerHUD = this;
    }
    public void UpdateCharDef(int lv, string name, string char_class)//Actualització de la definició del nostre jugador
    {
        playerDef.text = "Lv " + lv + ", " + name + ", " + char_class;
    }
    public void UpdateHP_MANA(int currHP, int currMANA)//Actualització de la nostre vida/mana
    {
        healthBar.UpdateBar(currHP, maxHP);
        manaBar.UpdateBar(currMANA, maxMANA);
    }

    public void UpdateXP(int curr_xp)//Actualització de la nostre xp
    {
        xpBar.UpdateBar(curr_xp, 100);
    }
}
