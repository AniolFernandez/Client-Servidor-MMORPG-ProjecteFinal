using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapFollower : MonoBehaviour
{
    private Transform player;

    // Start is called before the first frame update
    void Start()
    {
        if(ConnectToServer.myPlayer != null && ConnectToServer.myPlayer.playerCharacter)
            player = ConnectToServer.myPlayer.playerCharacter.transform;
    }

    // Update is called once per frame
    void Update()
    {
        //Fa que la camera del minimapa segueixi al jugador
        if(player==null && ConnectToServer.myPlayer != null && ConnectToServer.myPlayer.playerCharacter)
            player = ConnectToServer.myPlayer.playerCharacter.transform;
        else if(player!=null)
            this.transform.position = new Vector3(player.transform.position.x, this.transform.position.y, player.transform.position.z);
    }
}
