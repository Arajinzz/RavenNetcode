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
    }

    private void Update()
    {
        ReceivePacket();
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
            // Key frame
            UInt32 frame = data.PopUInt32();

            if (GameManager.Instance && frame > GameManager.Instance.currentTick)
            {
                GameManager.Instance.Rollback((int)frame);
                Debug.Log("Rollback");
            }

            // Simulate key
            InputManager.Key KeyPressed = data.PopKeyPressed();
            FPSMouvements Mouvements = Players[from].GetComponent<FPSMouvements>();
            Mouvements.SetKeyPressed(KeyPressed);
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
        } else if (packetType == P2PPacket.PacketType.PlayerRotated)
        {
            float mouseX = data.PopFloat();
            FPSMouvements Mouvements = Players[from].GetComponent<FPSMouvements>();
            Mouvements.RotatePlayer(mouseX);
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
