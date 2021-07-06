using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618
public class AllNetworkPackets : MonoBehaviour
{
    public const short LoginMsg = 100;
    public const short LoginAccepted = 101;
    public const short MoveTo = 102;
    public const short CurrentPosition = 103;
    public const short PlayerConnected = 104;
    public const short PlayerDisconnected = 105;
    public const short RegisterAccount = 106;
    public const short RegisterResponse = 107;
    public const short BadLogin = 108;
    public const short CreatePlayer = 109;
    public const short CreatePlayerResp = 110;
    public const short DeletePlayer = 111;
    public const short LoginCharacter = 112;
    public const short ReqLoginCharacter = 113;
    public const short SendChatMessage = 114;
    public const short SendPrivateMessage = 115;
    public const short KeepAlive = 116;
    public const short GetInfoOfPlayer = 117;
    public const short UpdatePlayerDef = 118;
    public const short StopGettingInfoOfPlayer = 119;
    public const short InstantiateNPC = 120;
    public const short DisposeNPC = 121;
    public const short MyPlayerUpdate = 122;
    public const short MoveNPC = 123;
    public const short AtackNPC = 124;
    public const short KillPlayer = 125;
    public const short UpdateCurrentPosNPC = 126;
    public const short GetInfoOfNPC = 127;
    public const short UpdateNPCDef = 128;
    public const short StopGettingInfoOfNPC = 129;
    public const short Atack = 130;
    public const short CastAtack = 131;
    public const short RemoveDebuffs = 132;
    public const short KillNPC = 133;
    public const short Dab = 134;
    public const short SendFriendRequest = 135;
    public const short AddFriendToList = 136;
    public const short RemoveFriend = 137;
    public const short ChangeFriendStatus = 138;
    public const short AcceptRequest = 139;
    public const short OkeyFriend = 140;
    public const short AddGoldBag = 141;
    public const short RemoveGoldBag = 142;
    public const short CatchGold = 143;
    public const short CatchGoldResp = 144;
    public const short CurrentGold = 145;
    public const short GotPlayer = 146;
}
//100
public class LoginMsg : MessageBase
{
    public string username;
    public string password;
}
//101
public class LoginAccepted : MessageBase
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
//102
public class MoveTo : MessageBase
{
    public int playerId;
    public float x, y, z;
}
//103
public class CurrentPosition : MessageBase
{
    public int playerId;
    public float x, y, z, yr;
}
//104
public class PlayerConnected : MessageBase
{
    public int playerId;
    public int classid;
    public string playername;
    public float x, y, z;
}
//105
public class PlayerDisconnected : MessageBase
{
    public int playerId;
}
//106
public class RegisterAccount : MessageBase
{
    public string username;
    public string password;
    public string email;
}
//107
public class RegisterResponse : MessageBase
{
    public short result;
}
//108
public class BadLogin : MessageBase
{
    public bool bad=true;
}
//109
public class CreatePlayer : MessageBase
{
    public string char_name;
    public int char_class;
}
//110
public class CreatePlayerResp : MessageBase
{
    public short result;
}
//111
public class DeletePlayer : MessageBase
{
    public string playername;
}
//112
public class LoginCharacter : MessageBase
{
    public int id;
    public string playername;
    public int level;
    public int xp;
    public int class_id;
    public int max_hp;
    public int max_mana;
    public int curr_hp;
    public int curr_mana;
    public float x;
    public float y;
    public float z;
    public float gold;
}
//113
public class RequestLoginCharacter : MessageBase
{
    public string playername;
}
//114
public class SendChatMessage : MessageBase
{
    public string message;
}
//115
public class SendPrivateMessage : MessageBase
{
    public string message;
    public string destinatary;
}
//116
public class KeepAlive : MessageBase
{
    bool keepAlive;
}
//117
public class GetInfoOfPlayer : MessageBase
{
    public int playerID;
}
//118
public class UpdatePlayerDef : MessageBase
{
    public int playerID;
    public int curr_hp;
    public int curr_mana;
    public int max_hp;
    public int max_mana;
    public int lvl;
    public int classid;
    public string name;
}
//119
public class StopGettingInfoOfPlayer : MessageBase
{
    public int playerID;
}
//120
public class InstantiateNPC : MessageBase
{
    public int npc_id;
    public int npc_type;
    public float x, y, z;
}
//121
public class DisposeNPC : MessageBase
{
    public int npc_id;
    public int npc_type;
}
//122
public class MyPlayerUpdate : MessageBase
{
    public int hp;
    public int hp_max;
    public int mana;
    public int mana_max;
    public int xp;
    public int level;
}
//123
public class MoveNPC : MessageBase
{
    public int npc_id;
    public int npc_type;
    public float x, y, z;
}
//124
public class AtackNPC : MessageBase
{
    public int npc_id;
    public int npc_type;
}
//125
public class KillPlayer : MessageBase
{
    public int playerID;
}
//126
public class UpdateCurrentPosNPC : MessageBase
{
    public int npc_id;
    public int npc_type;
    public float x, y, z, yr;
}
//127
public class GetInfoOfNPC : MessageBase
{
    public int npc_id;
    public int npc_type;
}
//128
public class UpdateNPCDef : MessageBase
{
    public int npc_id;
    public int curr_hp;
    public int curr_mana;
    public int max_hp;
    public int max_mana;
    public int lvl;
    public int type;
    public string name;
}
//129
public class StopGettingInfoOfNPC : MessageBase
{
    public int npc_id;
    public int npc_type;
}
//130
public class Atack : MessageBase
{
    public int npc_id;
    public int npc_type;
}
//131
public class CastAtack : MessageBase
{
    public int playerid;
    public int npc_id;
    public int npc_type;
}
//132
public class RemoveDebuffs : MessageBase
{
    public int playerid;
}
//133
public class KillNPC : MessageBase
{
    public int npc_id;
    public int npc_type;
}
//134
public class Dab : MessageBase
{
    public int playerID;
}
//135
public class SendFriendRequest : MessageBase
{
    public int id_to;
    public int id_from;
    public string name_from;
}
//136
public class AddFriendToList : MessageBase
{
    public int id;
    public string name;
    public bool online;
}
//137
public class RemoveFriend : MessageBase
{
    public int id;
}
//138
public class ChangeFriendStatus : MessageBase
{
    public int id;
    public bool online;
}
//139
public class AcceptRequest : MessageBase
{
    public int id_to;
    public int id_from;
    public bool accepted;
}
//140
public class OkeyFriend : MessageBase
{
    public bool accepted;
}
//141
public class AddGoldBag : MessageBase
{
    public float amount;
    public float id;
    public float x, y, z;
}
//142
public class RemoveGoldBag : MessageBase
{
    public float id;
}
//143
public class CatchGold : MessageBase
{
    public float id;
}
//144
public class CatchGoldResp : MessageBase
{
    public float amount;
    public bool catched;
}
//145
public class CurrentGold : MessageBase
{
    public float amount;
}
//146
public class GotPlayer : MessageBase
{
    public int id;
}