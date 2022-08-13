using System.Collections;
using System.Collections.Generic;
using Steamworks.Data;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public async void HostGame()
    {
        Debug.Log("Hosting a game ...");

        await SteamManager.Instance.CreateLobby(0);

    }

    public async void JoinGame()
    {
        Debug.Log("Joining a game");

        await SteamManager.Instance.RefreshMultiplayerLobbies();
        List<Lobby> Lobbies = SteamManager.Instance.LobbiesList;

        if(Lobbies.Count < 1)
        {
            Debug.Log("Ooops no Lobbies");
            return;
        }
        foreach(Lobby lobby in Lobbies)
        {
            Debug.Log(lobby.Owner.ToString());
        }

        await SteamManager.Instance.JoinLobby(Lobbies[0]);

    }
}
