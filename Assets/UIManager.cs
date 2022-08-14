using System.Collections;
using System.Collections.Generic;
using Steamworks.Data;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public async void HostGame()
    {
        await SteamLobbyManager.Instance.CreateLobby(2, "JustTesting");
    }

    public async void JoinGame()
    {
        await SteamLobbyManager.Instance.SearchLobbies("JustTesting");

        List<Lobby> lobbiesFound = SteamLobbyManager.Instance.LobbiesResult;

        if(lobbiesFound != null && lobbiesFound.Count > 0)
        {
            await SteamLobbyManager.Instance.JoinLobby(lobbiesFound[0]);
        }
    }
}
