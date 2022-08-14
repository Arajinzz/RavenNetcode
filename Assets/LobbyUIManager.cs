using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyUIManager : MonoBehaviour
{

    public void LeaveLobby()
    {
        Debug.Log("Leaving Lobby ...");
        SteamLobbyManager.Instance.LeaveLobby();
    }

    public void SendMessageToLobby()
    {
        TMPro.TMP_InputField InputField = GetComponentInChildren<TMPro.TMP_InputField>();
        SteamLobbyManager.Instance.CurrentLobby.SendChatString(InputField.text);
    }
}
