using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GoldBag : MonoBehaviour
{
    public float id;
    public TextMeshPro text;

    private void Start()
    {
        Invoke("DestroyThis", 60f);
    }

    private void Update()
    {
        RotateGoldName();
    }
    public void SetBagOfGold(float goldAmount, float id)
    {
        this.id = id;
        text.text = goldAmount + " gold";
    }

    /// <summary>
    /// Gira el nom de la bosa d'or per a que sempre estigui mirant cap a camera
    /// </summary>
    private void RotateGoldName()
    {
        Vector3 v = Camera.main.transform.position - text.transform.position;
        v.x = v.z = 0.0f;
        text.transform.LookAt(Camera.main.transform.position - v);
        text.transform.Rotate(0, 180, 0);
    }

    public void DestroyThis()
    {
        if (this.gameObject != null)
        {
            ConnectToServer.instance.gold.Remove(this);
            Destroy(this.gameObject);
        }
            
    }
}
