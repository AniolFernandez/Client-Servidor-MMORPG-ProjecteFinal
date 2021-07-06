using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBoxController : MonoBehaviour
{
    public Text text;
    Action ok;
    Action cancel;
    public Button okButton;
    public Button cancelButton;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))//al premer enter acceptem el msg box
            ok();
    }
    public void SetMessageBox(string msg, Action OK, Action Cancel)//Funció per a definir el msgBox. accions delegades
    {
        text.text = msg;
        ok = OK;
        cancel = Cancel;
    }

    // Start is called before the first frame update
    void Start()
    {
        okButton.onClick.AddListener(() => ok());
        cancelButton.onClick.AddListener(() => cancel());
    }
}
