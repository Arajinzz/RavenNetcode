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
        } else if (packetType == 3)
        {
            // Player left
            if (Players.ContainsKey(from)) {
                Destroy(Players[from]);
            }
        } else if (packetType == 4)
        {
            float x = BitConverter.ToSingle(bytes.GetRange(offset, sizeof(float)).ToArray());
            offset += sizeof(float);
            float y = BitConverter.ToSingle(bytes.GetRange(offset, sizeof(float)).ToArray());
            offset += sizeof(float);
            float z = BitConverter.ToSingle(bytes.GetRange(offset, sizeof(float)).ToArray());
            offset += sizeof(float);

            Vector3 position = new Vector3(x, y, z);

            x = BitConverter.ToSingle(bytes.GetRange(offset, sizeof(float)).ToArray());
            offset += sizeof(float);
            y = BitConverter.ToSingle(bytes.GetRange(offset, sizeof(float)).ToArray());
            offset += sizeof(float);
            z = BitConverter.ToSingle(bytes.GetRange(offset, sizeof(float)).ToArray());
            offset += sizeof(float);
            float w = BitConverter.ToSingle(bytes.GetRange(offset, sizeof(float)).ToArray());
            offset += sizeof(float);

            Quaternion rotation = new Quaternion(x, y, z, w);

            Players[from] = InstantiatePlayer(from);
            Players[from].transform.position = position;
            Players[from].transform.rotation = rotation;
        }
    }

    private GameObject InstantiatePlayer(SteamId id)
    {
        GameObject playerObj = Instantiate(player, GameObject.Find("SpawnPoint").transform);
        Debug.Log(playerObj.transform);

        if (id == SteamManager.Instance.PlayerSteamId)
        {
            playerObj.AddComponent<InputManager>();
        }

        return playerObj;
    }
}
