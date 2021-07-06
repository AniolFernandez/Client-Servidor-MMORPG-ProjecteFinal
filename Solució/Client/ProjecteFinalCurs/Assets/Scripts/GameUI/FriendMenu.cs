using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendMenu : MonoBehaviour
{
    private Vector3 startPosition;
    private RectTransform pos;

    public void Close()//Tanca el menú d'amics
    {
        this.gameObject.SetActive(false);
        LocalPlayer.Instance.DoNotMove();
        this.transform.position = startPosition;
    }

    void Start()
    {
        pos = this.gameObject.GetComponent<RectTransform>();
        startPosition = pos.position;
        this.gameObject.SetActive(false);
    }

    public void SetStartPos()//Retorna el menú a la posició original (cridat al tancar)
    {
        pos.position = startPosition;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || (Input.GetKeyDown(KeyCode.N) && !Chat.chatOpen))//A l'apretar 'ESC' o 'N' tenquem la llista d'amics
            Close();
    }
}
