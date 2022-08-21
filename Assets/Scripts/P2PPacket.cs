using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P2PPacket : MonoBehaviour
{
    
    public enum PacketType : ushort
    {
        IntantiatePlayer,
        NoKeyPressed,
        LeftMouseButtonPressed,
        PlayerLeft,
        InstantiatePlayerAtPosition,
    }

    public static byte[] Compose_InstantiatePlayerPacket()
    {
        List<byte> buffer = new List<byte>();
        ushort packetType = Convert.ToUInt16(PacketType.IntantiatePlayer);
        buffer.AddRange(BitConverter.GetBytes(packetType));
        // Maybe add position

        return buffer.ToArray();
    }

    public static void Decompose_InstantiatePlayerPacket(byte[] data)
    {

    }

    public static byte[] Compose_LeftMouseButtonPressedPacket(Vector3 hitPoint)
    {
        List<byte> buffer = new List<byte>();
        ushort packetType = Convert.ToUInt16(PacketType.LeftMouseButtonPressed);
        buffer.AddRange(BitConverter.GetBytes(packetType));
        buffer.AddRange(BitConverter.GetBytes(hitPoint.x));
        buffer.AddRange(BitConverter.GetBytes(hitPoint.y));
        buffer.AddRange(BitConverter.GetBytes(hitPoint.z));
        // Maybe add position

        return buffer.ToArray();
    }

    public static byte[] Compose_NoKeyPressedPacket()
    {
        List<byte> buffer = new List<byte>();
        ushort packetType = Convert.ToUInt16(PacketType.NoKeyPressed);
        buffer.AddRange(BitConverter.GetBytes(packetType));
        // Maybe add position

        return buffer.ToArray();
    }

    public static byte[] Compose_PlayerLeftPacket()
    {
        List<byte> buffer = new List<byte>();
        ushort packetType = Convert.ToUInt16(PacketType.PlayerLeft);
        buffer.AddRange(BitConverter.GetBytes(packetType));
        // Maybe add position

        return buffer.ToArray();
    }

    public static byte[] Compose_InstantiatePlayerAtPositionPacket()
    {
        GameObject Player = P2PNetworkReceive.Instance.Players[SteamManager.Instance.PlayerSteamId];
        Vector3 position = Player.transform.position;
        Quaternion rotation = Player.transform.rotation;

        List<byte> buffer = new List<byte>();
        ushort packetType = Convert.ToUInt16(PacketType.InstantiatePlayerAtPosition);
        buffer.AddRange(BitConverter.GetBytes(packetType));
        buffer.AddRange(BitConverter.GetBytes(position.x));
        buffer.AddRange(BitConverter.GetBytes(position.y));
        buffer.AddRange(BitConverter.GetBytes(position.z));

        buffer.AddRange(BitConverter.GetBytes(rotation.x));
        buffer.AddRange(BitConverter.GetBytes(rotation.y));
        buffer.AddRange(BitConverter.GetBytes(rotation.z));
        buffer.AddRange(BitConverter.GetBytes(rotation.w));
        // Maybe add position

        return buffer.ToArray();
    }

}
