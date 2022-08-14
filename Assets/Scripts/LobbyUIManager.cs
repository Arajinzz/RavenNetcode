using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField]
    GameObject PlayerListUI;

    [SerializeField]
    GameObject PlayerProfileUI;

    private Dictionary<SteamId, SteamPlayerObject> Players;

    private void Start()
    {
        // Get Players in lobby
        Players = SteamLobbyManager.Instance.PlayersInLobby;
    }

    private void Update()
    {
        
        if (Players != null)
        {
            foreach (SteamId id in Players.Keys)
            {
                if (!Players[id].ProfileUI)
                {
                    InstantiatePlayerUI(id);
                }
            }
        }

    }

    public async void InstantiatePlayerUI(SteamId id)
    {
        Image? avatar = await SteamLobbyManager.GetAvatar(id);
        Texture2D image = SteamLobbyManager.Covert(avatar.Value);
        GameObject obj = Instantiate(PlayerProfileUI, PlayerListUI.transform);
        obj.GetComponentInChildren<UnityEngine.UI.RawImage>().texture = image;
        obj.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = Players[id].PlayerName;
        Players[id].ProfileUI = obj;
    }

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
