using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Coordinates : MonoBehaviour
{
    public Transform t;
    public Text coords;

    private void Start()
    {
        coords = this.GetComponent<Text>();
    }
    // Update is called once per frame
    void Update()
    {
        if (t != null)
            SetCords();
        else if (ConnectToServer.myPlayer!=null)
            t = ConnectToServer.myPlayer.playerCharacter.transform;
    }

    void SetCords()
    {
        coords.text = "X: " + t.transform.position.x.ToString("0.00") + " Y: " + t.transform.position.y.ToString("0.00") + " Z: " + t.transform.position.z.ToString("0.00");
    }
}
