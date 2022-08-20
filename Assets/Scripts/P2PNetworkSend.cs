using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;

public class P2PNetworkSend : MonoBehaviour
{
    
    public static void SendToTarget(SteamId target, byte[] data)
    {
        //if (target == SteamManager.Instance.PlayerSteamId)
        //{
        //    P2PNetworkReceive.Instance.HandlePacket(target, data);
        //    return;
        //}
        Debug.Log("Sending data to: " + target.ToString());
        bool sent = SteamNetworking.SendP2PPacket(target, data);
        if (!sent)
        {
            Debug.Log("Packet to send to " + target.ToString() + " is dropped");
        }
    }

    public static void SendToAllLobby(Lobby lobby, byte[] data)
    {
        foreach (Friend member in lobby.Members)
        {
            SendToTarget(member.Id, data);
        }
    }

}
