using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
#pragma warning disable CS0618
public class NPCManagement : MonoBehaviour
{
    /// <summary>
    /// Obté una llista amb tots els NPCs que hi ha a la BD per a que el servidor els pugui instanciar
    /// </summary>
    /// <returns>Llista d'NPCInstance</returns>
    public static List<NPCInstance> GetNPCInstances()
    {
        List<NPCInstance> npcs = new List<NPCInstance>();
        DatabaseConnection dbCon = DatabaseConnection.Instance();
        lock (dbCon)
        {
            try
            {
                if (dbCon.IsConnect())
                {
                    string query = "SELECT n.npc_name, n.npc_level, n.max_hp, n.max_mana, i.npc_instance_id, i.position_x, i.position_y, i.position_z, i.NPC_npc_id FROM mmorpg.npc n, mmorpg.npc_instance i WHERE i.NPC_npc_id = n.npc_id;";
                    var cmd = new MySqlCommand(query, dbCon.Connection);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        NPCInstance tmp = new NPCInstance();
                        tmp.name = reader.GetString(0);
                        tmp.level = reader.GetInt32(1);
                        tmp.max_hp = reader.GetInt32(2);
                        tmp.max_mana = reader.GetInt32(3);
                        tmp.id = reader.GetInt32(4);
                        tmp.x = reader.GetFloat(5);
                        tmp.y = reader.GetFloat(6);
                        tmp.z = reader.GetFloat(7);
                        tmp.type = reader.GetInt32(8);
                        npcs.Add(tmp);
                    }
                }
            }
            catch (Exception e) { Debug.Log(e.Message); npcs = null; }
            dbCon.Close();
        }
        return npcs;
    }
}

public class NPCInstance
{
    public string name;
    public int level;
    public int max_hp;
    public int curr_hp;
    public int max_mana;
    public int curr_mana;
    public int id;
    public float x;
    public float y;
    public float z;
    public int type;
    public List<NetworkConnection> infoListeners = new List<NetworkConnection>();
    public GameObject npc_go;
    public EnemyController controller;
}
