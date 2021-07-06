using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public InventoryManager inv;
    public Image itemSprite;
    private ItemInstance item;
    public bool AddItem(ItemInstance item) {//Afegeix item a l'slot de l'inventari, segons el tipus d'item que tingui a la posicio d'inventorymanager
        bool ok = true;
        if (itemSprite == null)
        {
            try
            {
                itemSprite.sprite = inv.items[item.item_type];
                this.item = item;
                itemSprite.enabled = true;
            }
            catch
            {
                ok = false;
            }
        }
        else
            ok = false;
        return ok;
    }

    public void RemoveItem()//Elimina l'item de l'slot
    {
        itemSprite.sprite = null;
        this.item = null;
        itemSprite.enabled = false;
    }
}
