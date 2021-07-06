using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class NetworkPacket
{
    public abstract byte[] GetBytes();
    public abstract int Type();

    public class PacketType
    {
        public const int LoginPacket = 0;
        public const int LoginAccept = 1;
        public const int AddClient = 2;
        public const int MovePlayer = 3;
    }

    public class LoginPacket : NetworkPacket
    {
        public int username, password_md5;

        public static LoginPacket Parse(byte[] bytes)
        {
            LoginPacket tmp = new LoginPacket();
            tmp.username = BitConverter.ToInt32(bytes, 4);
            tmp.password_md5 = BitConverter.ToInt32(bytes, 8);
            return tmp;
        }

        public override byte[] GetBytes()
        {
            byte[] tmp = new byte[TotalBytes()];
            byte[] type = BitConverter.GetBytes(PacketType.LoginPacket);
            byte[] xbr = BitConverter.GetBytes(username);
            byte[] ybr = BitConverter.GetBytes(password_md5);

            for (int i = 0; i < tmp.Length; i++)
            {
                if (i < 4)
                    tmp[i] = type[i];
                else if (i < 8)
                    tmp[i] = xbr[i - 4];
                else
                    tmp[i] = ybr[i - 8];
            }
            return tmp;
        }

        public override int Type()
        {
            return PacketType.LoginPacket;
        }

        public static int TotalBytes() { return sizeof(int) + sizeof(int) + sizeof(int); }
    }

    public class LoginAccept : NetworkPacket
    {
        public long playerID;
        public float x, y, z;

        public LoginAccept() { }

        public LoginAccept(long playerID, float x, float y, float z)
        {
            this.playerID = playerID;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static LoginAccept Parse(byte[] bytes)
        {
            LoginAccept tmp = new LoginAccept();
            tmp.playerID = BitConverter.ToInt64(bytes, 4);
            tmp.x = BitConverter.ToSingle(bytes, 12);
            tmp.y = BitConverter.ToSingle(bytes, 16);
            tmp.z = BitConverter.ToSingle(bytes, 20);
            return tmp;
        }

        public override byte[] GetBytes()
        {
            byte[] tmp = new byte[TotalBytes()];
            byte[] type = BitConverter.GetBytes(PacketType.LoginAccept);
            byte[] id = BitConverter.GetBytes(playerID);
            byte[] xbr = BitConverter.GetBytes(x);
            byte[] ybr = BitConverter.GetBytes(y);
            byte[] zbr = BitConverter.GetBytes(z);

            for (int i = 0; i < tmp.Length; i++)
            {
                if (i < 4)
                    tmp[i] = type[i];//Primers 4 bytes tipus
                else if (i < 12)
                    tmp[i] = id[i - 4];//Seguents 8 bytes id
                else if (i < 16)
                    tmp[i] = xbr[i - 12];//Seguents 4 bytes x
                else if (i < 20)
                    tmp[i] = ybr[i - 16];//Seguents 4 bytes y
                else if (i < 24)
                    tmp[i] = zbr[i - 20];//Seguents 4 bytes z
            }
            return tmp;
        }

        public override int Type()
        {
            return PacketType.LoginAccept;
        }

        public static int TotalBytes() { return sizeof(long) + sizeof(int) + sizeof(int) + sizeof(int) + sizeof(int); }
    }

    public class AddClient : NetworkPacket
    {
        public long playerID;
        public float x, y, z;

        public AddClient() { }

        public AddClient(long playerID, float x, float y, float z)
        {
            this.playerID = playerID;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static AddClient Parse(byte[] bytes)
        {
            AddClient tmp = new AddClient();
            tmp.playerID = BitConverter.ToInt64(bytes, 4);
            tmp.x = BitConverter.ToSingle(bytes, 12);
            tmp.y = BitConverter.ToSingle(bytes, 16);
            tmp.z = BitConverter.ToSingle(bytes, 20);
            return tmp;
        }

        public override byte[] GetBytes()
        {
            byte[] tmp = new byte[TotalBytes()];
            byte[] type = BitConverter.GetBytes(PacketType.AddClient);
            byte[] id = BitConverter.GetBytes(playerID);
            byte[] xbr = BitConverter.GetBytes(x);
            byte[] ybr = BitConverter.GetBytes(y);
            byte[] zbr = BitConverter.GetBytes(z);

            for (int i = 0; i < tmp.Length; i++)
            {
                if (i < 4)
                    tmp[i] = type[i];//Primers 4 bytes tipus
                else if (i < 12)
                    tmp[i] = id[i - 4];//Seguents 8 bytes id
                else if (i < 16)
                    tmp[i] = xbr[i - 12];//Seguents 4 bytes x
                else if (i < 20)
                    tmp[i] = ybr[i - 16];//Seguents 4 bytes y
                else if (i < 24)
                    tmp[i] = zbr[i - 20];//Seguents 4 bytes z
            }
            return tmp;
        }

        public override int Type()
        {
            return PacketType.AddClient;
        }

        public static int TotalBytes() { return sizeof(long) + sizeof(int) + sizeof(int) + sizeof(int) + sizeof(int); }
    }

    public class MovePlayer : NetworkPacket
    {
        public long playerID;
        public float destinationX, destinationY, destinationZ;
        public float timestamp;

        public MovePlayer() { }

        public MovePlayer(long playerID, float x, float y, float z, float timestamp)
        {
            this.playerID = playerID;
            this.destinationX = x;
            this.destinationY = y;
            this.destinationZ = z;
            this.timestamp = timestamp;
        }

        public static MovePlayer Parse(byte[] bytes)
        {
            MovePlayer tmp = new MovePlayer();
            tmp.playerID = BitConverter.ToInt64(bytes, 4);
            tmp.destinationX = BitConverter.ToSingle(bytes, 12);
            tmp.destinationY = BitConverter.ToSingle(bytes, 16);
            tmp.destinationZ = BitConverter.ToSingle(bytes, 20);
            tmp.timestamp = BitConverter.ToSingle(bytes, 24);
            return tmp;
        }

        public override byte[] GetBytes()
        {
            byte[] tmp = new byte[TotalBytes()];
            byte[] type = BitConverter.GetBytes(PacketType.MovePlayer);
            byte[] id = BitConverter.GetBytes(playerID);
            byte[] xbr = BitConverter.GetBytes(destinationX);
            byte[] ybr = BitConverter.GetBytes(destinationY);
            byte[] zbr = BitConverter.GetBytes(destinationZ);
            byte[] time = BitConverter.GetBytes(timestamp);

            for (int i = 0; i < tmp.Length; i++)
            {
                if (i < 4)
                    tmp[i] = type[i];//Primers 4 bytes tipus
                else if (i < 12)
                    tmp[i] = id[i - 4];//Seguents 8 bytes id
                else if (i < 16)
                    tmp[i] = xbr[i - 12];//Seguents 4 bytes x
                else if (i < 20)
                    tmp[i] = ybr[i - 16];//Seguents 4 bytes y
                else if (i < 24)
                    tmp[i] = zbr[i - 20];//Seguents 4 bytes z
                else
                    tmp[i] = time[i - 24];//Seguents 4 bytes temps
            }
            return tmp;
        }

        public override int Type()
        {
            return PacketType.MovePlayer;
        }

        public static int TotalBytes() { return sizeof(int) + sizeof(long) + sizeof(float) + sizeof(float) + sizeof(float) + sizeof(float); }
    }

}
