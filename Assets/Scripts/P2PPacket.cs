using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P2PPacket
{
    
    public enum PacketType : ushort
    {
        IntantiatePlayer,
        InstantiatePlayerAtPosition,
        KeyEvent,
        PlayerLeft,
    }

    public UInt16 packetType;
    public List<byte> buffer;
    public int offset;

    public P2PPacket(PacketType type)
    {
        packetType = Convert.ToUInt16(type);
        buffer = new List<byte>();
        buffer.AddRange(BitConverter.GetBytes(packetType));
    }

    public P2PPacket(byte[] data)
    {
        buffer = new List<byte>(data);
        packetType = BitConverter.ToUInt16(buffer.GetRange(offset, sizeof(ushort)).ToArray());
        offset += sizeof(ushort);
    }

    public PacketType GetPacketType()
    {
        return (PacketType)packetType;
    }

    public void InsertUInt32(UInt32 data)
    {
        buffer.AddRange(BitConverter.GetBytes(data));
    }

    public UInt32 PopUInt32()
    {
        UInt32 data = BitConverter.ToUInt32(buffer.GetRange(offset, sizeof(UInt32)).ToArray());
        offset += sizeof(UInt32);
        return data;
    }

    public void InsertFloat(float data)
    {
        buffer.AddRange(BitConverter.GetBytes(data));
    }

    public float PopFloat()
    {
        float data = BitConverter.ToSingle(buffer.GetRange(offset, sizeof(float)).ToArray());
        offset += sizeof(float);
        return data;
    }

    public void InsertKeyPressed(InputManager.Key keyPressed)
    {
        UInt32 key = Convert.ToUInt32(keyPressed);
        InsertUInt32(key);
    }

    public InputManager.Key PopKeyPressed()
    {
        UInt32 key = PopUInt32();
        return (InputManager.Key)key;
    }

}
