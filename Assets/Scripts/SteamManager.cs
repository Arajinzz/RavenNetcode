using System;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
    public static SteamManager Instance;
    private static uint AppId = 480u;

    public string PlayerName { get; set; }
    public SteamId PlayerSteamId { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(this);
            Instance = this;

            try
            {

                SteamClient.RestartAppIfNecessary(AppId);
                SteamClient.Init(AppId, true);

                if (!SteamClient.IsValid)
                {
                    throw new Exception("Steam client not valid");
                }

                PlayerName = SteamClient.Name;
                PlayerSteamId = SteamClient.SteamId;

                // Because we're using the relay network
                SteamNetworkingUtils.InitRelayNetworkAccess();
            } catch (Exception e)
            {
                QuickLog.Instance.Log("Error connecting to steam");
                Debug.Log(e);
            }

        } else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        SteamClient.RunCallbacks();
    }

    private void OnApplicationQuit()
    {
        GameCleanup();
    }

    public void AcceptP2P(SteamId user)
    {
        try
        {
            // For two players to send P2P packets to each other, they each must call this on the other player
            SteamNetworking.AcceptP2PSessionWithUser(user);
        }
        catch
        {
            QuickLog.Instance.Log("Unable to accept P2P Session with user");
        }
    }

    public void CloseP2P(SteamId user)
    {
        try
        {
            // For two players to send P2P packets to each other, they each must call this on the other player
            SteamNetworking.CloseP2PSessionWithUser(user);
        }
        catch
        {
            QuickLog.Instance.Log("Unable to close P2P Session with user");
        }
    }

    private void GameCleanup()
    {
        SteamClient.Shutdown();
    }
}
