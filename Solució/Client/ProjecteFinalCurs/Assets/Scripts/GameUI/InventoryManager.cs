using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public List<Sprite> items;//Llista d'items que conté l'inventari (No implementat encara)
    public Text gold;
    private Vector3 startPosition;
    private RectTransform pos;
    public static InventoryManager instance;
    void Start()
    {
        instance = this;
        pos = this.gameObject.GetComponent<RectTransform>();
        startPosition = pos.position;
        this.gameObject.SetActive(false);
    }

    public void Close()//Tanca l'inventari
    {
        this.gameObject.SetActive(false);
        LocalPlayer.Instance.DoNotMove();
        this.transform.position = startPosition;
    }

    public void SetStartPos()//Retorna l'inventari a la posició original
    {  
        pos.position = startPosition;
    }

    public void UpdateGold(float currentGold)
    {
        gold.text = currentGold + " gold";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))//Tanca l'inventari al apretar 'ESC'
            Close();
    }
}
