using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;

public class P2PNetworkReceive : MonoBehaviour
{
    public static P2PNetworkReceive Instance;

    [SerializeField]
    GameObject player;

    public Dictionary<SteamId, GameObject> Players;

    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(this);
            Instance = this;
            Players = new Dictionary<SteamId, GameObject>();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        

        // Check every 0.05 seconds for new packets
        InvokeRepeating("ReceivePacket", 0f, 0.05f);
    }

    private void ReceivePacket()
    {
        while (SteamNetworking.IsP2PPacketAvailable())
        {
            var packet = SteamNetworking.ReadP2PPacket();
            if (packet.HasValue)
            {
                HandlePacket(packet.Value.SteamId, packet.Value.Data);
            }
        }
    }

    public void HandlePacket(SteamId from, byte[] packet)
    {
        if (packet == null)
            return;

        // Handle Packet
        P2PPacket data = new P2PPacket(packet);
        P2PPacket.PacketType packetType = data.GetPacketType();

        // Instantiate Player
        if (packetType == P2PPacket.PacketType.IntantiatePlayer)
        {
            Players[from] = InstantiatePlayer(from);
        } else if (packetType == P2PPacket.PacketType.KeyEvent)
        {
            // Simulate key
            InputManager.Key KeyPressed = data.PopKeyPressed();
            FPSMouvements Mouvements = Players[from].GetComponent<FPSMouvements>();
            if (InputManager.CompareKey(KeyPressed, InputManager.Key.W) ||
                InputManager.CompareKey(KeyPressed, InputManager.Key.S) ||
                InputManager.CompareKey(KeyPressed, InputManager.Key.A) ||
                InputManager.CompareKey(KeyPressed, InputManager.Key.D))
            {
                Mouvements.HandleMouvement();
            }
        } else if (packetType == P2PPacket.PacketType.PlayerLeft)
        {
            // Player left
            if (Players.ContainsKey(from)) {
                Destroy(Players[from]);
            }
        } else if (packetType == P2PPacket.PacketType.InstantiatePlayerAtPosition)
        {
            Vector3 position = new Vector3(data.PopFloat(), data.PopFloat(), data.PopFloat());
            Quaternion rotation = new Quaternion(data.PopFloat(), data.PopFloat(), data.PopFloat(), data.PopFloat());
            Players[from] = InstantiatePlayer(from);
            Players[from].transform.position = position;
            Players[from].transform.rotation = rotation;
        }
    }

    private GameObject InstantiatePlayer(SteamId id)
    {
        GameObject playerObj = Instantiate(player, GameObject.Find("SpawnPoint").transform);
        if (id == SteamManager.Instance.PlayerSteamId)
        {
            playerObj.AddComponent<InputManager>();
        }

        return playerObj;
    }
}
