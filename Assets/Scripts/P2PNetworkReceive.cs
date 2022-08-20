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

    Dictionary<SteamId, GameObject> Players;

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
            Debug.Log("A Packet is available");
            var packet = SteamNetworking.ReadP2PPacket();
            if (packet.HasValue)
            {
                Debug.Log("Packet received from " + packet.Value.SteamId.ToString());
                HandlePacket(packet.Value.SteamId, packet.Value.Data);
            }
        }
    }

    public void HandlePacket(SteamId from, byte[] packet)
    {
        if (packet == null)
            return;

        Debug.Log("Handling the packet");
        // Handle Packet
        List<byte> bytes = new List<byte>(packet);

        int offset = 0;
        ushort packetType = BitConverter.ToUInt16(bytes.GetRange(offset, sizeof(ushort)).ToArray());
        offset += sizeof(ushort);

        // Instantiate Player
        if (packetType == 0)
        {
            Players[from] = InstantiatePlayer(from);
        } else if (packetType == 1)
        {
            // No key pressed do nothing
        } else if (packetType == 2)
        {
            float x = BitConverter.ToSingle(bytes.GetRange(offset, sizeof(float)).ToArray());
            offset += sizeof(float);
            float y = BitConverter.ToSingle(bytes.GetRange(offset, sizeof(float)).ToArray());
            offset += sizeof(float);
            float z = BitConverter.ToSingle(bytes.GetRange(offset, sizeof(float)).ToArray());
            offset += sizeof(float);

            Vector3 hitPoint = new Vector3(x, y, z);

            Players[from].GetComponent<PlayerMouvement>().Move(hitPoint);

            Debug.Log(hitPoint);
        }
    }

    private GameObject InstantiatePlayer(SteamId id)
    {
        Debug.Log("Instantiating a player");
        GameObject playerObj = Instantiate(player, GameObject.Find("SpawnPoint").transform);
        Debug.Log(playerObj.transform);

        if (id == SteamManager.Instance.PlayerSteamId)
        {
            playerObj.AddComponent<InputManager>();
        }

        return playerObj;
    }
}
