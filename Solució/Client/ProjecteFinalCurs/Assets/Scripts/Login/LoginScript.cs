using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#pragma warning disable CS0618
public class LoginScript : MonoBehaviour
{
    public GameObject messageBox;
    public Text registerText;

    public Color hover;
    public Color defaultColor;
    public Text serverStatus;

    public Sprite notSelected;
    public Sprite seleced;

    public GameObject RegisterGameObject;
    public InputField register_username;
    public InputField register_password1;
    public InputField register_password2;
    public InputField register_email;

    public GameObject LoginGameObject;
    public InputField login_username;
    public InputField login_password;

    public GameObject SelectCharacter1;
    public Text SelectCharacter1Text;
    public GameObject SelectCharacter2;
    public Text SelectCharacter2Text;
    public GameObject SelectCharacter3;
    public Text SelectCharacter3Text;
    public GameObject exitBtn;
    public GameObject deleteBtn;

    public GameObject swordsman;
    public GameObject swordsmanSelector;
    public GameObject archer;
    public GameObject archerSelector;
    public GameObject magician;
    public GameObject magicianSelector;
    public InputField charname;
    public GameObject cancelCreation;
    private int selectedClass = 1;
    private int selectedSlot = 0;
    private CharsAvailable charsLocal;
    private GameObject msgbxInstance=null;

    // Start is called before the first frame update
    void Start()
    {
        registerText.color = Color.HSVToRGB(defaultColor.r, defaultColor.g, defaultColor.b);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && LoginGameObject.active)//Si apretem enter i estem al login, intenta fer el login
            Login();
        else if (Input.GetKeyDown(KeyCode.Return) && RegisterGameObject.active)//Igual amb el registre
            Register();
    }

    #region CharCreation
    public void CreateNewChar()//Obrim el menú de creació de pj, desactivem tot lo altre
    {
        SelectCharacter1.SetActive(false);
        SelectCharacter2.SetActive(false);
        SelectCharacter3.SetActive(false);
        exitBtn.SetActive(false);
        deleteBtn.SetActive(false);
        swordsman.SetActive(true);
        magician.SetActive(true);
        archer.SetActive(true);
        charname.gameObject.SetActive(true);
        swordsmanSelector.SetActive(true);
        selectedClass = 1;
        cancelCreation.SetActive(true);
    }

    public void Create()//Botó de cració del pj
    {
        if (charname.text != "")
        {//Enviem al servidor la petició de crear el jugador
            ConnectToServer.client.Send(AllNetworkPackets.CreatePlayer, new CreatePlayer { char_name = charname.text, char_class = selectedClass });
            charname.text = "";
        }
        else
        {
            MessageBox("Character name can't be empty!", () => Destroy(msgbxInstance), () => Destroy(msgbxInstance));
        }
    }

    public void SelectSwordsman()//Seleccionem com a classe espadachin
    {
        selectedClass = 1;
        swordsmanSelector.SetActive(true);
        magicianSelector.SetActive(false);
        archerSelector.SetActive(false);
    }

    public void SelectArcher()//Seleccionem com a classe arquer
    {
        selectedClass = 2;
        swordsmanSelector.SetActive(false);
        magicianSelector.SetActive(false);
        archerSelector.SetActive(true);
    }

    public void SelectMagician()//Seleccionem com a classe mag
    {
        selectedClass = 3;
        swordsmanSelector.SetActive(false);
        magicianSelector.SetActive(true);
        archerSelector.SetActive(false);
    }

    public void ExitCreation()//Al sortir de la creació de personatge, resetejem la pantalla de selecció ja que tornarem a rebre els paquets dels pj que tenim
    {
        SelectCharacter1.SetActive(true);
        SelectCharacter2.SetActive(true);
        SelectCharacter3.SetActive(true);
        SelectCharacter1.GetComponent<Image>().sprite = notSelected;
        SelectCharacter2.GetComponent<Image>().sprite = notSelected;
        SelectCharacter3.GetComponent<Image>().sprite = notSelected;
        SelectCharacter1Text.text = "Create a new player";
        SelectCharacter2Text.text = "Create a new player";
        SelectCharacter3Text.text = "Create a new player";
        exitBtn.SetActive(true);
        deleteBtn.SetActive(true);
        swordsman.SetActive(false);
        magician.SetActive(false);
        archer.SetActive(false);
        charname.gameObject.SetActive(false);
        swordsmanSelector.SetActive(false);
        magicianSelector.SetActive(false);
        archerSelector.SetActive(false);
        cancelCreation.SetActive(false);
        selectedSlot = 0;
        LoggedIn(charsLocal);
    }
    #endregion

    #region CharSelection
    public void dcAcc()//Desconectem la compta
    {
        SelectCharacter1Text.text = "Create a new player";
        SelectCharacter2Text.text = "Create a new player";
        SelectCharacter3Text.text = "Create a new player";
        LoginGameObject.SetActive(true);
        SelectCharacter1.SetActive(false);
        SelectCharacter2.SetActive(false);
        SelectCharacter3.SetActive(false);
        exitBtn.SetActive(false);
        deleteBtn.SetActive(false);
        ConnectToServer.client.Send(AllNetworkPackets.PlayerDisconnected, new PlayerDisconnected());
    }

    public static string GetClass(int id)//Retorna el nom de la classe a partir de l'id
    {
        string cls = "";
        switch (id)
        {
            case 1:
                cls = "Swordsman";
                break;
            case 2:
                cls = "Archer";
                break;
            case 3:
                cls = "Magician";
                break;
        }
        return cls;
    }

    public void ClickOnSlot1()
    {
        if (selectedSlot==1)
        {//Primer marcar el slot amb la flexa, o si ja esta marcat entrar. Afegir boto d'eliminar player
            ConnectToServer.charc = charsLocal.player_1_name;
            SceneManager.LoadScene(1);
        }
        else if(charsLocal.player_1_name != null && charsLocal.player_1_name != "")//Create new player
        {
            SelectCharacter1.GetComponent<Image>().sprite = seleced;
            SelectCharacter2.GetComponent<Image>().sprite = notSelected;
            SelectCharacter3.GetComponent<Image>().sprite = notSelected;
            selectedSlot = 1;
        }
        else
        {
            CreateNewChar();
        }
    }

    public void ClickOnSlot2()
    {
        if (selectedSlot == 2)
        {//Primer marcar el slot amb la flexa, o si ja esta marcat entrar. Afegir boto d'eliminar player
            ConnectToServer.charc = charsLocal.player_2_name;
            SceneManager.LoadScene(1);
        }
        else if (charsLocal.player_2_name != null && charsLocal.player_2_name != "")//Create new player
        {
            SelectCharacter1.GetComponent<Image>().sprite = notSelected;
            SelectCharacter2.GetComponent<Image>().sprite = seleced;
            SelectCharacter3.GetComponent<Image>().sprite = notSelected;
            selectedSlot = 2;
        }
        else
        {
            CreateNewChar();
        }
    }

    public void ClickOnSlot3()
    {
        if (selectedSlot == 3)
        {//Primer marcar el slot amb la flexa, o si ja esta marcat entrar. Afegir boto d'eliminar player
            ConnectToServer.charc = charsLocal.player_3_name;
            SceneManager.LoadScene(1);
        }
        else if (charsLocal.player_3_name != null && charsLocal.player_3_name != "")//Create new player
        {
            SelectCharacter1.GetComponent<Image>().sprite = notSelected;
            SelectCharacter2.GetComponent<Image>().sprite = notSelected;
            SelectCharacter3.GetComponent<Image>().sprite = seleced;
            selectedSlot = 3;
        }
        else
        {
            CreateNewChar();
        }
    }

    public void DeleteSelectedChar()
    {
        if (selectedSlot != 0)//Obrim popup de si realment volem esborrar el pj
        {
            MessageBox("Do you really wish to delete the selected character? This action is irreversible.", () => DeleteChar(), () => Destroy(msgbxInstance));
        }
    }

    public void DeleteChar()//Si realment el volem esborrar enviem el paquet
    {
        DeletePlayer dp = new DeletePlayer();
        switch (selectedSlot)//Enviem el nom del pj seleccionat
        {
            case 1:
                dp.playername = charsLocal.player_1_name;
                break;
            case 2:
                dp.playername = charsLocal.player_2_name;
                break;
            case 3:
                dp.playername = charsLocal.player_3_name;
                break;
        }
        ConnectToServer.client.Send(AllNetworkPackets.DeletePlayer, dp);//Envia el paquet
        //Resetejem la vista
        Destroy(msgbxInstance);
        SelectCharacter1.SetActive(true);
        SelectCharacter2.SetActive(true);
        SelectCharacter3.SetActive(true);
        SelectCharacter1.GetComponent<Image>().sprite = notSelected;
        SelectCharacter2.GetComponent<Image>().sprite = notSelected;
        SelectCharacter3.GetComponent<Image>().sprite = notSelected;
        SelectCharacter1Text.text = "Create a new player";
        SelectCharacter2Text.text = "Create a new player";
        SelectCharacter3Text.text = "Create a new player";
        exitBtn.SetActive(true);
        deleteBtn.SetActive(true);
        selectedSlot = 0;
    }
    #endregion

    #region LoginBoxMethods
    public void LoggedIn(CharsAvailable chars)//Logguejem la compta amb els jugadors
    {
        charsLocal = chars;
        LoginGameObject.SetActive(false);
        SelectCharacter1.SetActive(true);
        SelectCharacter2.SetActive(true);
        SelectCharacter3.SetActive(true);
        exitBtn.SetActive(true);
        deleteBtn.SetActive(true);
        if (chars.player_1_name != "")
        {
            SelectCharacter1Text.text = "Lv " + chars.player_1_level + ", " + chars.player_1_name + ", " + GetClass(chars.player_1_class);
        }
        if (chars.player_2_name != "")
        {
            SelectCharacter2Text.text = "Lv " + chars.player_2_level + ", " + chars.player_2_name + ", " + GetClass(chars.player_2_class);
        }
        if (chars.player_3_name != "")
        {
            SelectCharacter3Text.text = "Lv " + chars.player_3_level + ", " + chars.player_3_name + ", " + GetClass(chars.player_3_class);
        }
    }

    public void Login()//Envia petició de login
    {
        if (ConnectToServer.client.isConnected)
        {
            if (login_password.text != "" && login_username.text != "")
            {//Enviem paquet de login
                ConnectToServer.client.Send(AllNetworkPackets.LoginMsg, new LoginMsg { username = login_username.text, password = ComputeSha256Hash(login_password.text) });
                ClearLogin();
            }
        }
        else
        {
            MessageBox("Can't connect to server. Server may be offline, in maintenance or you don't have connection.", () => Destroy(msgbxInstance), () => Destroy(msgbxInstance));
        }
            
    }

    public void ClearLogin()//Esborra la contrasenya
    {
        login_password.text = "";
    }

    public void RegisterLink()//Botó per a registrar-se. Activa el menú de registre
    {
        ClearLogin();
        LoginGameObject.SetActive(false);
        RegisterGameObject.SetActive(true);
    }

    public void registerHOver()//Al passar per sobre del registre canviem el seu color
    {
        registerText.color = Color.HSVToRGB(hover.r,hover.g,hover.b);
    }

    public void registerHOverExit()//Al sortir de sobre del registre el tornem al color original
    {
        registerText.color = Color.HSVToRGB(defaultColor.r, defaultColor.g, defaultColor.b);
    }

    public void Exit()//Botó sortir de l'applicació
    {
        Application.Quit();
    }

    #endregion

    #region RegisterBoxMethods

    public void ClearRegister()//Neteja els camps de registre
    {
        register_email.text = "";
        register_password1.text = "";
        register_password2.text = "";
        register_username.text = "";
    }

    public void CancelRegister()//Cancelem la creació de la compta
    {
        ClearRegister();
        RegisterGameObject.SetActive(false);
        LoginGameObject.SetActive(true);
    }

    public void Register()
    {
        //Comprobem cada camp i registrem (Ho comprobem localment, pero igualment en el servidor es torna a comprobar)
        if(register_username.text.Length > 0 && register_username.text.Length < 45)
        {
            if(register_password1.text.Length > 0 && register_password2.text.Length > 0)
            {
                if (IsValidEmail(register_email.text))
                {
                    if (register_password1.text.Equals(register_password2.text))//Si tot és correcte enviem paquet de registre
                    {
                        ConnectToServer.client.Send(AllNetworkPackets.RegisterAccount, new RegisterAccount { username = register_username.text, password = ComputeSha256Hash(register_password1.text), email = register_email.text });
                        ClearRegister();
                    }
                    else
                        MessageBox("Passwords do not match", () => Destroy(msgbxInstance), () => Destroy(msgbxInstance));
                }
                else
                {
                    MessageBox("You must provide a valid email", () => Destroy(msgbxInstance), () => Destroy(msgbxInstance));
                }
            }
            else
            {
                MessageBox("You must provide a password", () => Destroy(msgbxInstance), () => Destroy(msgbxInstance));
            }
        }
        else
        {
            MessageBox("You must provide an username", () => Destroy(msgbxInstance), () => Destroy(msgbxInstance));
        }
    }

    private string ComputeSha256Hash(string rawData)//Realitza l'encriptat en sha256 de la contrasenya
    {
 
        using (SHA256 sha256Hash = SHA256.Create())
        {

            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));


            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    private void MessageBox(string msg, Action onOK, Action onCancel)//MessageBox
    {
        if (msgbxInstance == null)
        {
            msgbxInstance = Instantiate(messageBox, new Vector3(0, 0, 0), Quaternion.identity);
            msgbxInstance.GetComponent<RectTransform>().SetParent(this.transform, false);
            msgbxInstance.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 130);
            msgbxInstance.GetComponent<MessageBoxController>().SetMessageBox(msg, onOK, onCancel);
        }
    }

    bool IsValidEmail(string email)//Comprova si l'email es vàlid
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    #endregion
}

public struct CharsAvailable
{
    public string player_1_name;
    public int player_1_level;
    public int player_1_class;
    public string player_2_name;
    public int player_2_level;
    public int player_2_class;
    public string player_3_name;
    public int player_3_level;
    public int player_3_class;
}
