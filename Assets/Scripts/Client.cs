using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client
{
    // Static buffer size for data transmission
    public static int dataBufferSize = 4096;

    // Client properties
    public int id;
    public TCP tcp;
    public Player player;

    // Client constructor
    public Client(int _clientId)
    {
        id = _clientId;
        tcp = new TCP(id);
        player = null;
    }

    // TCP class for handling network communication
    public class TCP
    {
        // TCP properties
        public TcpClient socket;

        private readonly int id;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        // TCP constructor
        public TCP(int _id)
        {
            id = _id;
        }

        // Connect and set up the TcpClient
        public void Connect(TcpClient _socket)
        {
            socket = _socket;
            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;

            stream = socket.GetStream();

            receivedData = new Packet();
            receiveBuffer = new byte[dataBufferSize];

            // Begin asynchronous read from the stream
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

            // Send a welcome message to the client
            ServerSend.Welcome(id, "Welcome to the server!");
        }

        // Send data to the client
        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    // Begin asynchronous write to the stream
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error sending data to player {id} via TCP: {_ex}");
            }
        }

        // Receive data from the client
        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    Server.clients[id].Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLength];

                // Copy received data to a new byte array
                Array.Copy(receiveBuffer, _data, _byteLength);

                // Reset received data and begin another read
                receivedData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error receiving TCP data: {_ex}");
                Server.clients[id].Disconnect();

            }
        }

        // Process received data
        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;

            receivedData.SetBytes(_data);

            // Check if received data contains more than 4 unread bytes (size of int)
            if (receivedData.UnreadLength() >= 4)
            {
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0)
                {
                    return true;
                }
            }

            // Read and handle packets in the received data
            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetID = _packet.ReadInt();
                        // Execute the appropriate packet handler based on the packet ID
                        Server.packetHandlers[_packetID](id, _packet);
                    }
                });

                _packetLength = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (_packetLength <= 1)
            {
                return true;
            }

            return false;
        }

        public void Disconnect()
        {
            socket.Close();
            stream = null;
            receiveBuffer = null;
            receivedData = null;
            socket = null;
        }
    }

    public void Matchmaking(int _fromClient)
    {
        
        int _playerCount = 0;

        foreach (Client _client in Server.clients.Values)
        {
            try {
                if (_client.tcp.socket.Client.RemoteEndPoint != null)
                {
                    _playerCount++;
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"One of the clients is null: {_ex}");
            }
        }

        Debug.Log($"Number of clients: {_playerCount}");

        if (_playerCount == 1)
        {
            ServerSend.ServerMessage(_fromClient, "Waiting for another player to join...");
        }
        else if (_playerCount == 2)
        {
            foreach (Client _client in Server.clients.Values) 
            {
                ServerSend.StartGame(_client.id, $"Starting a game for Client {_client.id}!");
            }
        }
    }

    private void Disconnect()
    {
        Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");

        player = null;

        tcp.Disconnect();
    }
}
