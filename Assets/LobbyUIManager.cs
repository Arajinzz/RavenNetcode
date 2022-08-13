using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyUIManager : MonoBehaviour
{
    public void LeaveLobby()
    {
        Debug.Log("Leaving Lobby ...");
        SteamManager.Instance.LeaveLobby();
    }
}
