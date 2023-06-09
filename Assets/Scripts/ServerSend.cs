using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ServerSend
{
    /// <summary>Sends a packet to a client via TCP.</summary>
    /// <param name="_toClient">The client to send the packet the packet to.</param>
    /// <param name="_packet">The packet to send to the client.</param>
    private static void SendTCPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].tcp.SendData(_packet);
    }

    /// <summary>Sends a packet to all clients via TCP.</summary>
    /// <param name="_packet">The packet to send.</param>
    private static void SendTCPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(_packet);
        }
    }

    /// <summary>Sends a packet to all clients except one via TCP.</summary>
    /// <param name="_exceptClient">The client to NOT send the data to.</param>
    /// <param name="_packet">The packet to send.</param>
    private static void SendTCPDataToAllExcept(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
    }

    #region  Packets
    /// <summary>Sends a welcome message to the given client.</summary>
    /// <param name="_toClient">The client to send the packet to.</param>
    /// <param name="_msg">The message to send.</param>
    public static void Welcome(int _toClient, string _msg)
    {
        using (Packet _packet = new Packet((int)ServerPackets.Welcome))
        {
            _packet.Write(_msg);
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
    }

    public static void ServerMessage(int _toClient, string _msg)
    {
        using (Packet _packet = new Packet((int)ServerPackets.ServerMessage))
        {
            _packet.Write(_msg);
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>Sends a message to the given client.</summary>
    /// <param name="_toClient">The client to send the packet to.</param>
    /// <param name="_msg">The message to send.</param>
    public static void StartGame(int _toClient, string _msg)
    {
        using (Packet _packet = new Packet((int)ServerPackets.StartGame))
        {
            _packet.Write(_msg);
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
    }

    public static void GameUpdate(MoveInfo moveInfo)
    {
        using (Packet _packet = new Packet((int)ServerPackets.GameUpdate))
        {
            _packet.Write(moveInfo.Position.Row);
            _packet.Write(moveInfo.Position.Column);
            _packet.Write(moveInfo.Player);
            _packet.Write(moveInfo.Outflanked.Count);
            foreach (Position position in moveInfo.Outflanked)
            {
                _packet.Write(position.Row);
                _packet.Write(position.Column);
            }

            SendTCPDataToAll(_packet);
        }

        Debug.Log($"Sent game update to all clients.\n\n");
    }

    public static void PlayerDisconnected(string _msg) {
        using (Packet _packet = new Packet((int)ServerPackets.PlayerDisconnected))
        {
            _packet.Write(_msg);
            SendTCPDataToAll(_packet);
        }
    }

    #endregion
}
