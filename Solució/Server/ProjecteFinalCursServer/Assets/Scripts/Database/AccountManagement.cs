using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccountManagement
{
    #region Register
    public static short RegisterAccount(string username, string password, string email)//Registra la compta a la DB
    {//RETURNS 0 = INSERT OK, 1=USER ALREADY EXISTS, 2=FAILED TO INSERT, 3=BAD INPUTS
        try
        {
            int filesAfectades = 0;
            if (username != "" && password.Length == 64 && IsValidEmail(email))
            {
                if (!UserAlreadyExist(username))
                {
                    DatabaseConnection dbCon = DatabaseConnection.Instance();
                    lock (dbCon)
                    {
                        try
                        {
                            if (dbCon.IsConnect())
                            {
                                string insert = "INSERT INTO user (username, email, password, last_login, total_time_played, USER_AUTHORITY_usr_auth_id) VALUES (@username, @email, @password, 0, 0, 1);";
                                var cmd = new MySqlCommand(insert, dbCon.Connection);
                                cmd.Parameters.AddWithValue("@username", username);
                                cmd.Parameters.AddWithValue("@email", email);
                                cmd.Parameters.AddWithValue("@password", password);
                                filesAfectades = cmd.ExecuteNonQuery();

                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e.Message);
                        }
                        dbCon.Close();
                    }    
                    if (filesAfectades == 1)
                        return 0;
                    else
                        return 2;
                }
                return 1;
            }
            else
                return 3;
        }
        catch (Exception e) { Debug.Log(e.Message); return 2; }

    }

    private static bool UserAlreadyExist(string username)//Comprova si l'usuari ja existeix
    {
        bool existeix = false;
        DatabaseConnection dbCon = DatabaseConnection.Instance();
        lock (dbCon)
        {
            try
            {

                if (dbCon.IsConnect())
                {
                    string query = "SELECT user_id FROM user WHERE username = @username;";
                    var cmd = new MySqlCommand(query, dbCon.Connection);
                    cmd.Parameters.AddWithValue("@username", username);
                    var reader = cmd.ExecuteReader();
                    existeix = reader.Read();
                }
            }
            catch (Exception e) { Debug.Log(e.Message); existeix= false; }
            dbCon.Close();
        }
       
        return existeix;
    }

    private static bool IsValidEmail(string email)//Comprova si és un email vaild
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    #endregion

    #region Login
    /// <summary>
    /// Logueja la compta entrada
    /// </summary>
    /// <param name="username">usuari</param>
    /// <param name="password">password en sha256</param>
    /// <returns>Retorna usuari per a fer la connexió o null si és erroni</returns>
    public static ConnectedAccount LoginAccount(string username, string password)
    {
        ConnectedAccount acc=null;
        DatabaseConnection dbCon = DatabaseConnection.Instance();
        lock (dbCon)
        {
            try
            {
                if (dbCon.IsConnect())
                {
                    string query = "SELECT * FROM user WHERE username = @username AND password = @password;";
                    var cmd = new MySqlCommand(query, dbCon.Connection);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        acc = new ConnectedAccount();
                        acc.id = reader.GetInt32(0);
                        acc.username = reader.GetString(1);
                        acc.last_login = DateTime.Now;//reader.GetMySqlDateTime(4);
                        acc.total_time_played = reader.GetInt32(5);
                        acc.authority = reader.GetInt32(6);
                    }  
                }
            }
            catch (Exception e) { Debug.Log(e.Message); acc= null; }
            dbCon.Close();
        }
        return acc;
    }

    /// <summary>
    /// Obté els personatge de la compta
    /// </summary>
    /// <param name="uid">user_id</param>
    /// <returns>Tots els personatges de la compta</returns>
    public static CharsAvailable GetAccChars(int uid)
    {
        CharsAvailable chars = new CharsAvailable();
        DatabaseConnection dbCon = DatabaseConnection.Instance();
        lock (dbCon)
        {
            try
            {
                if (dbCon.IsConnect())
                {
                    string query = "SELECT character_name, character_level, CLASS_class_id FROM mmorpg.character WHERE USER_user_id = @userid AND inactive = false;";
                    var cmd = new MySqlCommand(query, dbCon.Connection);
                    cmd.Parameters.AddWithValue("@userid", uid);
                    var reader = cmd.ExecuteReader();
                    int charCounter = 0;
                    while (reader.Read())
                    {
                        charCounter++;
                        if (charCounter == 1)
                        {
                            chars.player_1_name = reader.GetString(0);
                            chars.player_1_level = reader.GetInt32(1);
                            chars.player_1_class = reader.GetInt32(2);
                        }
                        else if (charCounter == 2)
                        {
                            chars.player_2_name = reader.GetString(0);
                            chars.player_2_level = reader.GetInt32(1);
                            chars.player_2_class = reader.GetInt32(2);
                        }
                        if (charCounter == 3)
                        {
                            chars.player_3_name = reader.GetString(0);
                            chars.player_3_level = reader.GetInt32(1);
                            chars.player_3_class = reader.GetInt32(2);
                        }
                    }
                }
            }
            catch (Exception e) { Debug.Log(e.Message); }
            dbCon.Close();
        }
        
        return chars;
    }

    /// <summary>
    /// Obté les dades del pesonatge que es vol connectar
    /// </summary>
    /// <param name="playername">nom del pj</param>
    /// <param name="uid">user_id</param>
    /// <returns>Jugador loggejat</returns>
    public static PlayerStructure loginCharacter(string playername, int uid, out float x, out float y, out float z)
    {
        x = 0; y = 0; z = 0;
        PlayerStructure ps = null;
        DatabaseConnection dbCon = DatabaseConnection.Instance();
        lock (dbCon)
        {
            try
            {
                if (dbCon.IsConnect())
                {
                    string query = "SELECT character_id, character_name, character_level, character_current_xp, char_last_pos_x, char_last_pos_y, char_last_pos_z, char_max_hp, char_max_mana, char_basic_damage, char_basic_defense, gold, CLASS_class_id, char_curr_hp, char_curr_mana FROM mmorpg.character WHERE USER_user_id = @uid AND character_name = @playername;";
                    var cmd = new MySqlCommand(query, dbCon.Connection);
                    cmd.Parameters.AddWithValue("@playername", playername);
                    cmd.Parameters.AddWithValue("@uid", uid);
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        ps = new PlayerStructure();
                        ps.id = reader.GetInt32(0);
                        ps.playername = reader.GetString(1);
                        ps.level = reader.GetInt32(2);
                        ps.xp = reader.GetInt32(3);
                        x = reader.GetFloat(4);
                        y = reader.GetFloat(5);
                        z = reader.GetFloat(6);
                        ps.max_hp = reader.GetInt32(7);
                        ps.max_mana = reader.GetInt32(8);
                        ps.damage = reader.GetInt32(9);
                        ps.defense = reader.GetInt32(10);
                        ps.gold = reader.GetFloat(11);
                        ps.class_id = reader.GetInt32(12);
                        ps.curr_hp = reader.GetInt32(13);
                        ps.curr_mana = reader.GetInt32(14);
                        ps.friends = new List<Friend>();
                    }
                }
            }
            catch (Exception e) { Debug.Log(e.Message); ps = null; }
            dbCon.Close();
            if (ps != null)
                ps.friends = GetPlayerFriends(ps.id);
        }
        return ps;
    }

    /// <summary>
    /// Actualitza l'ultim login de la compta (cridat al fer la desconnexió). és suma el temps jugat
    /// </summary>
    public static bool UpdateLastLogin(ConnectedAccount ca)
    {
        bool result = false;
        DatabaseConnection dbCon = DatabaseConnection.Instance();
        lock (dbCon)
        {
            try
            {
                if (dbCon.IsConnect())
                {
                    string query = "UPDATE mmorpg.user SET last_login = @last_login, total_time_played = @total_time_played WHERE user_id = @user_id;";
                    var cmd = new MySqlCommand(query, dbCon.Connection);
                    cmd.Parameters.AddWithValue("@last_login", ca.last_login.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@total_time_played", (DateTime.Now - ca.last_login).Seconds);
                    cmd.Parameters.AddWithValue("@user_id", ca.id);
                    result = cmd.ExecuteNonQuery() == 1;
                }
            }
            catch (Exception e) { Debug.Log(e.Message); result = true; }
            dbCon.Close();
        }
        return result;
    }
    #endregion

    #region PlayerManagment
    /// <summary>
    /// Crea un nou personatge per a la compta indicada de la classe indicada i hi afegeix els stats base
    /// </summary>
    /// <param name="playerName">Nom del nou jugador</param>
    /// <param name="char_class">Tipus de classe</param>
    /// <param name="uid">User_id</param>
    /// <returns>0 = ok, 1= bad, 2 = already exist, 3 = max</returns>
    public static short CreatePlayer(string playerName, int char_class, int uid)
    {// 0 = ok, 1=bad, 2 = already exist, 3=max
        short result = 1;
        if (countAccountChars(uid) < 3)
        {
            if (!playerAlreadyExist(playerName))
            {
                DatabaseConnection dbCon = DatabaseConnection.Instance();
                lock (dbCon)
                {
                    try
                    {
                        if (dbCon.IsConnect())
                        {
                            string insert = "INSERT INTO mmorpg.character (character_name, character_level, character_current_xp, char_last_pos_x, char_last_pos_y, char_last_pos_z, char_max_hp, char_max_mana, char_basic_damage, char_basic_defense, gold, USER_user_id, CLASS_class_id, HAS_GUILD_id, char_curr_hp, char_curr_mana) VALUES (@name, 1, 0, 0, 0, 0, 100, 100, @dmg, @def, 0, @uid, @cid, null, 100, 100);";
                            var cmd = new MySqlCommand(insert, dbCon.Connection);
                            cmd.Parameters.AddWithValue("@name", playerName);
                            cmd.Parameters.AddWithValue("@cid", char_class);
                            cmd.Parameters.AddWithValue("@uid", uid);
                            switch (char_class)
                            {
                                case 1:
                                    cmd.Parameters.AddWithValue("@dmg", 20);
                                    cmd.Parameters.AddWithValue("@def", 10);
                                    break;
                                case 2:
                                    cmd.Parameters.AddWithValue("@dmg", 12);
                                    cmd.Parameters.AddWithValue("@def", 8);
                                    break;
                                case 3:
                                    cmd.Parameters.AddWithValue("@dmg", 25);
                                    cmd.Parameters.AddWithValue("@def", 9);
                                    break;
                            }
                            if (cmd.ExecuteNonQuery() == 1)
                                result = 0;
                            else
                                result = 1;
                        }
                    }
                    catch (Exception e) { Debug.Log(e.Message); result = 1; }
                    dbCon.Close();
                }
            }
            else
                result = 2;
        }
        else
            result = 3;
        

        return result;
    }

    /// <summary>
    /// Compta els personatges actius que te la commpta
    /// </summary>
    /// <param name="uid">suer_id</param>
    /// <returns>nº pjs</returns>
    private static int countAccountChars(int uid)
    {
        int result=3;
        DatabaseConnection dbCon = DatabaseConnection.Instance();
        lock (dbCon)
        {
            try
            {
                if (dbCon.IsConnect())
                {
                    string query = "SELECT count(*) FROM mmorpg.character WHERE USER_user_id = @userid AND inactive = false;";
                    var cmd = new MySqlCommand(query, dbCon.Connection);
                    cmd.Parameters.AddWithValue("@userid", uid);
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        result = reader.GetInt32(0);
                    }
                }
            }
            catch (Exception e) { Debug.Log(e.Message); result = 3; }
            dbCon.Close();
        }
        return result;
    }

    /// <summary>
    /// Comprova si el nom del jugador ja existeix
    /// </summary>
    /// <param name="name">nom del jugador</param>
    /// <returns>true=existeix / false=no existeix</returns>
    private static bool playerAlreadyExist(string name)
    {
        bool result = true;
        DatabaseConnection dbCon = DatabaseConnection.Instance();
        lock (dbCon)
        {
            try
            {
                if (dbCon.IsConnect())
                {
                    string query = "SELECT count(*) FROM mmorpg.character WHERE character_name = @name;";
                    var cmd = new MySqlCommand(query, dbCon.Connection);
                    cmd.Parameters.AddWithValue("@name", name);
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        result = reader.GetInt32(0) == 1;//Existeix =1
                    }
                }
            }
            catch (Exception e) { Debug.Log(e.Message); result = true; }
            dbCon.Close();
        }
        return result;
    }

    /// <summary>
    /// "Elimina" el jugador pasat per parametre. Realment no s'elimina, es posa en inactiu
    /// </summary>
    /// <param name="playername">Nom del jugador</param>
    /// <param name="uid">user_id</param>
    /// <returns>true si ha anat bé</returns>
    public static bool DeleteCharacter(string playername, int uid)
    {
        bool result = false;
        DatabaseConnection dbCon = DatabaseConnection.Instance();
        lock (dbCon)
        {
            try
            {
                if (dbCon.IsConnect())
                {
                    string query = "UPDATE mmorpg.character SET inactive = true WHERE character_name = @name AND USER_user_id = @userid;";
                    var cmd = new MySqlCommand(query, dbCon.Connection);
                    cmd.Parameters.AddWithValue("@name", playername);
                    cmd.Parameters.AddWithValue("@userid", uid);
                    result = cmd.ExecuteNonQuery()==1;
                }
            }
            catch (Exception e) { Debug.Log(e.Message); result = true; }
            dbCon.Close();
        }
        return result;
    }

    /// <summary>
    /// Guarda els stats actuals del jugador
    /// </summary>
    public static bool SavePlayerStats(PlayerStructure ps, float x, float y, float z)
    {
        bool result = false;
        DatabaseConnection dbCon = DatabaseConnection.Instance();
        lock (dbCon)
        {
            try
            {
                if (dbCon.IsConnect())
                {
                    string query = "UPDATE mmorpg.character SET character_level = @character_level, character_current_xp = @character_current_xp, char_last_pos_x = @char_last_pos_x, char_last_pos_y = @char_last_pos_y, char_last_pos_z = @char_last_pos_z, char_max_hp = @char_max_hp, char_max_mana = @char_max_mana, char_basic_damage = @char_basic_damage, char_basic_defense = @char_basic_defense, gold = @gold, char_curr_hp = @char_curr_hp, char_curr_mana = @char_curr_mana WHERE character_id = @character_id;";
                    var cmd = new MySqlCommand(query, dbCon.Connection);
                    cmd.Parameters.AddWithValue("@character_id", ps.id);
                    cmd.Parameters.AddWithValue("@character_level", ps.level);
                    cmd.Parameters.AddWithValue("@character_current_xp", ps.xp);
                    cmd.Parameters.AddWithValue("@char_last_pos_x", x);
                    cmd.Parameters.AddWithValue("@char_last_pos_y", y);
                    cmd.Parameters.AddWithValue("@char_last_pos_z", z);
                    cmd.Parameters.AddWithValue("@char_max_hp", ps.max_hp);
                    cmd.Parameters.AddWithValue("@char_max_mana", ps.max_mana);
                    cmd.Parameters.AddWithValue("@char_basic_damage", ps.damage);
                    cmd.Parameters.AddWithValue("@char_basic_defense", ps.defense);
                    cmd.Parameters.AddWithValue("@gold", ps.gold);
                    cmd.Parameters.AddWithValue("@char_curr_hp", ps.curr_hp);
                    cmd.Parameters.AddWithValue("@char_curr_mana", ps.curr_mana);
                    result = cmd.ExecuteNonQuery() == 1;
                }
            }
            catch (Exception e) { Debug.Log(e.Message); result = true; }
            dbCon.Close();
        }
        return result;
    }
    #endregion

    #region Friends
    /// <summary>
    /// Afegeix els ids de pj passat per paràmetre com a amics
    /// </summary>
    /// <returns>RETURNS 0 = INSERT OK, 1=JA SON AMICS, 2=FAILED TO INSERT</returns>
    public static short AddFriend(int id_1, int id_2)
    {//RETURNS 0 = INSERT OK, 1=JA SON AMICS, 2=FAILED TO INSERT
        short resposta = 2;
        if (!AlreadyFriends(id_1, id_2))
        {
            int filesAfectades = 0;
            DatabaseConnection dbCon = DatabaseConnection.Instance();
            lock (dbCon)
            {
                try
                {
                    if (dbCon.IsConnect())
                    {
                        string insert = "INSERT INTO mmorpg.character_has_character VALUES (@id1, @id2);";
                        var cmd = new MySqlCommand(insert, dbCon.Connection);
                        cmd.Parameters.AddWithValue("@id1", id_1);
                        cmd.Parameters.AddWithValue("@id2", id_2);
                        filesAfectades = cmd.ExecuteNonQuery();
                        if (filesAfectades == 1)
                            resposta = 0;
                        else
                            resposta = 2;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
                dbCon.Close();
            }
        }
        else
            resposta = 1;
        return resposta;
        
    }

    /// <summary>
    /// Comprova si dos pj són amics
    /// </summary>
    /// <param name="id_1">pj1_id</param>
    /// <param name="id_2">pj2_id</param>
    /// <returns>true = són amics / false = no ho son</returns>
    public static bool AlreadyFriends(int id_1, int id_2)
    {
        bool result = true;
        DatabaseConnection dbCon = DatabaseConnection.Instance();
        lock (dbCon)
        {
            try
            {
                if (dbCon.IsConnect())
                {
                    string query = "SELECT count(*) FROM mmorpg.character_has_character WHERE (CHARACTER_character_id = @id1 AND CHARACTER_character_id2 = @id2) OR (CHARACTER_character_id = @id2 AND CHARACTER_character_id2 = @id1);";
                    var cmd = new MySqlCommand(query, dbCon.Connection);
                    cmd.Parameters.AddWithValue("@id1", id_1);
                    cmd.Parameters.AddWithValue("@id2", id_2);
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        result = reader.GetInt32(0) == 1;//Existeix =1
                    }
                }
            }
            catch (Exception e) { Debug.Log(e.Message); result = true; }
            dbCon.Close();
        }
        return result;
    }
    
    /// <summary>
    /// Obté una llista amb els amics del jugador
    /// </summary>
    /// <param name="player_id">id del jugador a buscar amics</param>
    /// <returns>Llista d'amics</returns>
    public static List<Friend> GetPlayerFriends(int player_id)
    {
        List<Friend> friends = new List<Friend>();
        DatabaseConnection dbCon = DatabaseConnection.Instance();
        lock (dbCon)
        {
            try
            {
                if (dbCon.IsConnect())
                {
                    string query = "SELECT c.character_id, c.character_name FROM mmorpg.character_has_character f, mmorpg.character c WHERE (f.CHARACTER_character_id = @id OR f.CHARACTER_character_id2 = @id) AND c.character_id != @id AND (c.character_id = f.CHARACTER_character_id2 OR c.character_id = f.CHARACTER_character_id);";
                    var cmd = new MySqlCommand(query, dbCon.Connection);
                    cmd.Parameters.AddWithValue("@id", player_id);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Friend friend = new Friend();
                        friend.id = reader.GetInt32(0);
                        friend.name = reader.GetString(1);
                        friends.Add(friend);
                    }
                }
            }
            catch (Exception e) { Debug.Log(e.Message); }
            dbCon.Close();
        }
        return friends;
    }

    /// <summary>
    /// Acaba amb la relació d'amistat de dos jugadors
    /// </summary>
    /// <param name="id_1">pj1_id</param>
    /// <param name="id_2">pj2_id</param>
    /// <returns>true si tot correcte</returns>
    public static bool RemoveFriends(int id_1, int id_2)
    {
        bool result = true;
        DatabaseConnection dbCon = DatabaseConnection.Instance();
        lock (dbCon)
        {
            try
            {
                if (dbCon.IsConnect())
                {
                    string query = "DELETE FROM mmorpg.character_has_character WHERE (CHARACTER_character_id = @id1 AND CHARACTER_character_id2 = @id2) OR (CHARACTER_character_id = @id2 AND CHARACTER_character_id2 = @id1);";
                    var cmd = new MySqlCommand(query, dbCon.Connection);
                    cmd.Parameters.AddWithValue("@id1", id_1);
                    cmd.Parameters.AddWithValue("@id2", id_2);
                    result = cmd.ExecuteNonQuery()==1;//1 s'ha esborrat bé
                }
            }
            catch (Exception e) { Debug.Log(e.Message); result = true; }
            dbCon.Close();
        }
        return result;
    }
    #endregion
}
public struct CharsAvailable
{
    public string player_1_name;
    public int player_1_level;
    public int player_1_class;
    public string player_2_name;
    public int player_2_level;
    public int player_2_class;
    public string player_3_name;
    public int player_3_level;
    public int player_3_class;
}
