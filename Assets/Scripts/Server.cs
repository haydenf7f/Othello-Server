using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server
{
    // Properties for the server's max players and port number
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }
    
    // Dictionary to store connected clients
    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

    // Delegate for packet handling methods
    public delegate void PacketHandler(int _fromClient, Packet _packet);
    // Dictionary to store packet handler methods
    public static Dictionary<int, PacketHandler> packetHandlers;

    // TCP listener for accepting incoming connections
    private static TcpListener tcpListener;

    // Start the server with the specified max players and port number
    public static void Start(int _maxPlayers, int _port)
    {
        MaxPlayers = _maxPlayers;
        Port = _port;

        Debug.Log("Starting server...");
        InitializeServerData();

        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        // Begin accepting incoming TCP connections asynchronously
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

        Debug.Log($"Server started on port {Port}.");
    }

    // Callback for when a new client connects
    private static void TCPConnectCallback(IAsyncResult _result)
    {
        TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
        // Continue listening for new connections
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

        Debug.Log($"Incoming connection from {_client.Client.RemoteEndPoint}...");

        // Iterate through the client slots and assign the new client to an available slot
        for (int i = 1; i <= MaxPlayers; i++)
        {
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(_client);
                return;
            }
        }

        // If no slots are available, inform that the server is full
        Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
    }

    // Initialize server data and packet handlers
    private static void InitializeServerData()
    {
        // Create client instances for each player slot
        for (int i = 1; i <= MaxPlayers; i++)
        {
            clients.Add(i, new Client(i));
        }

        // Initialize packet handler methods
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ClientPackets.WelcomeReceived, ServerHandle.WelcomeReceived },
            { (int)ClientPackets.PlayerMove, ServerHandle.PlayerMove},
        };

        Debug.Log("Initialized packets.");
    }
}
