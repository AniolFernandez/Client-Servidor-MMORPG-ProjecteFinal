using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStructure
{
    public int id;
    public string playername;
    public int level;
    public int xp;
    public int max_hp;
    public int max_mana;
    public int damage;
    public int defense;
    public float gold;
    public int class_id;
    public int curr_hp;
    public int curr_mana;
    public List<Friend> friends;
}
public class Friend
{
    public int id;
    public string name;
    public bool online;
}
