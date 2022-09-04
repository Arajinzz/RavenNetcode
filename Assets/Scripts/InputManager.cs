using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public enum Key : UInt32
    {
        None = 0,
        LeftMouseDown = 1,
        RightMouseDown = 2,
        W = 4,
        S = 8,
        A = 16,
        D = 32,
        SPACE = 64,
    }

    public Key KeyPressed;
    public Key LastKeyPressed;
    bool stopSendingKeys = false;

    // Update is called once per frame
    void Update()
    {
        int tick = 0;
        if (GameManager.Instance)
        {
            tick = GameManager.Instance.currentTick;
        }
        HandleTick(tick);
    }

    private void HandleTick(int tick)
    {
        LastKeyPressed = KeyPressed;

        if (Input.GetMouseButtonDown(0))
        {
            KeyPressed |= Key.LeftMouseDown;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            KeyPressed &= ~Key.LeftMouseDown;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            KeyPressed |= Key.W;
        }
        else if (Input.GetKeyUp(KeyCode.W))
        {
            KeyPressed &= ~Key.W;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            KeyPressed |= Key.S;
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            KeyPressed &= ~Key.S;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            KeyPressed |= Key.A;
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            KeyPressed &= ~Key.A;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            KeyPressed |= Key.D;
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            KeyPressed &= ~Key.D;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            KeyPressed |= Key.SPACE;
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            KeyPressed &= ~Key.SPACE;
        }

        if (KeyPressed == 0 && !stopSendingKeys)
        {
            // Send Packet
            P2PPacket packet = new P2PPacket(P2PPacket.PacketType.KeyEvent);
            packet.InsertUInt32((UInt32)tick % 60);
            packet.InsertKeyPressed(KeyPressed);
            P2PNetworkSend.SendToAllLobby(SteamLobbyManager.Instance.CurrentLobby, packet.buffer.ToArray());
            stopSendingKeys = true;
        }
        else
        {
            // Send Packet
            P2PPacket packet = new P2PPacket(P2PPacket.PacketType.KeyEvent);
            packet.InsertUInt32((UInt32)tick % 60);
            packet.InsertKeyPressed(KeyPressed);
            P2PNetworkSend.SendToAllLobby(SteamLobbyManager.Instance.CurrentLobby, packet.buffer.ToArray());
            stopSendingKeys = false;
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
            KeyPressed = 0;
    }

    public bool isKeyPressed(Key key)
    {
        return (KeyPressed & key) != 0;
    }

    public static bool CompareKey(Key key1, Key key2)
    {
        return (key1 & key2) != 0;
    }

}
