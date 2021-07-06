using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable
public class NetworkPacket : NetworkBehaviour
{
    public virtual void FixedUpdate()
    {
        PlayerDesiredDestinationManager.Tick();
        ServerPlayerCurrentLocation.Tick();
        PlayerLoginToServer.Tick();
        SendResponseToLogin.Tick();
    }

    #region PlayerDesiredDestination
    [Serializable]
    public class PlayerDesiredDestination
    {
        public float x, y, z, yr, timestamp;
    }

    private NetworkPacketManager<PlayerDesiredDestination> _PlayerDesiredDestination;

    public NetworkPacketManager<PlayerDesiredDestination> PlayerDesiredDestinationManager
    {
        get
        {
            if (_PlayerDesiredDestination == null)
            {
                _PlayerDesiredDestination = new NetworkPacketManager<PlayerDesiredDestination>();
                if (isLocalPlayer)
                    _PlayerDesiredDestination.OnRequierePackageTransmit += TransmitPlayerDesiredDestinationToServer;
            }
            return _PlayerDesiredDestination;
        }
    }

    private void TransmitPlayerDesiredDestinationToServer(byte[] bytes)
    {
        CmdTransmitPlayerDesiredDestinationToServer(bytes);
    }

    [Command]
    void CmdTransmitPlayerDesiredDestinationToServer(byte[] data)
    {
        PlayerDesiredDestinationManager.ReceiveData(data);
    }
    #endregion

    #region PlayerCurrentLocation
    [Serializable]
    public class PlayerCurrentLocation
    {
        public float x, y, z, yr, timestamp;
    }

    private NetworkPacketManager<PlayerCurrentLocation> _ServerPlayerCurrentLocation;

    public NetworkPacketManager<PlayerCurrentLocation> ServerPlayerCurrentLocation
    {
        get
        {
            if (_ServerPlayerCurrentLocation == null)
            {
                _ServerPlayerCurrentLocation = new NetworkPacketManager<PlayerCurrentLocation>();
                if (isServer)
                    _ServerPlayerCurrentLocation.OnRequierePackageTransmit += TransmitPlayerCurrentLocationToClients;
            }
            return _ServerPlayerCurrentLocation;
        }
    }

    private void TransmitPlayerCurrentLocationToClients(byte[] bytes)
    {
        RpcRecievePlayerCurrentLocation(bytes);
    }

    [ClientRpc]
    void RpcRecievePlayerCurrentLocation(byte[] data)
    {
        ServerPlayerCurrentLocation.ReceiveData(data);
    }
    #endregion

    #region LoginToServer
    [Serializable]
    public class LoginToServer
    {
        public string username, password;
    }

    private NetworkPacketManager<LoginToServer> _PlayerLoginToServer;

    public NetworkPacketManager<LoginToServer> PlayerLoginToServer
    {
        get
        {
            if (_PlayerLoginToServer == null)
            {
                _PlayerLoginToServer = new NetworkPacketManager<LoginToServer>();
                if (isLocalPlayer)
                    _PlayerLoginToServer.OnRequierePackageTransmit += TransmitLoginToServer;
            }
            return _PlayerLoginToServer;
        }
    }

    private void TransmitLoginToServer(byte[] bytes)
    {
        CmdTransmitLoginToServer(bytes);
    }

    [Command]
    void CmdTransmitLoginToServer(byte[] data)
    {
        PlayerLoginToServer.ReceiveData(data);
    }
    #endregion

    #region ResponseToLogin
    [Serializable]
    public class ResponseToLogin
    {
        public string username, password;
    }

    private NetworkPacketManager<LoginToServer> _ResponseToLogin;

    public NetworkPacketManager<LoginToServer> SendResponseToLogin
    {
        get
        {
            if (_ResponseToLogin == null)
            {
                _ResponseToLogin = new NetworkPacketManager<LoginToServer>();
                if (isServer)
                    _ResponseToLogin.OnRequierePackageTransmit += TransmitResponseToLogin;
            }
            return _ResponseToLogin;
        }
    }

    private void TransmitResponseToLogin(byte[] bytes)
    {
        CmdTransmitResponseToLogin(bytes);
    }

    [Command]
    void CmdTransmitResponseToLogin(byte[] data)
    {
        PlayerLoginToServer.ReceiveData(data);
    }
    #endregion

}
