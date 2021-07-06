using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowSelector : MonoBehaviour
{
    public Transform toFollow;

    // Update is called once per frame
    void Update()
    {
        if(toFollow!=null)//Fa que la fletxa de seleccionar segueixi el seu target
        {
            if (toFollow.tag == "otherPlayer")//Si és un jugador modifiquem la posició de la fletxa amb el seu offset, ja que el jugador està una mica descolocat
                this.transform.position = new Vector3(toFollow.transform.position.x, toFollow.transform.position.y + 8f, toFollow.transform.position.z + 2f);
            else//Esta seleccionant enemics
                this.transform.position = new Vector3(toFollow.transform.position.x, toFollow.transform.position.y + 6f, toFollow.transform.position.z);
        }
        else
        {
            Destroy(this.gameObject);
        }
            
    }
}
