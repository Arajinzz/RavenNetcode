using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class PlayerGameState
{
    public SteamId id;
    public Vector3 playerPosition;

    public PlayerGameState(SteamId id, Vector3 position)
    {
        this.id = id;
        this.playerPosition = position;
    }
}

public class GameState
{
    public Dictionary<SteamId, PlayerGameState> Players;

    public GameState()
    {
        Players = new Dictionary<SteamId, PlayerGameState>();

        // Get All players
        foreach (SteamId key in P2PNetworkReceive.Instance.Players.Keys)
        {
            // Get player gameobject
            GameObject player = P2PNetworkReceive.Instance.Players[key];

            // Save positions
            Players[key] = new PlayerGameState(key, player.transform.position);
        }
    }
}
