using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ServerHandle
{
    public static void WelcomeReceived(int _fromClient, Packet _packet)
    {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} ({_username}) connected successfully and is now player {_fromClient}.");

        // Check that the Client has claimed the correct ID
        if (_fromClient != _clientIdCheck)
        {
            Debug.Log($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
        }
        Server.clients[_fromClient].Matchmaking(_fromClient);
    }

    public static void PlayerMove(int _fromClient, Packet _packet)
    {
        int row = _packet.ReadInt();
        int column = _packet.ReadInt();
        Position position = new Position(row, column);
        int player = _packet.ReadInt();
        int outflankedCount = _packet.ReadInt();
        List<Position> outflanked = new List<Position>();
        for (int i = 0; i < outflankedCount; i++)
        {
            int outflankedRow = _packet.ReadInt();
            int outflankedColumn = _packet.ReadInt();
            outflanked.Add(new Position(outflankedRow, outflankedColumn));
        }
        
        MoveInfo moveInfo = new MoveInfo(player, position, outflanked);
        ServerSend.GameUpdate(moveInfo);
    }
}
