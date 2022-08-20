using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SteamPlayerObject
{
    public string PlayerName = "";
    public GameObject ProfileUI = null;

    public SteamPlayerObject(string name, GameObject obj)
    {
        PlayerName = name;
        ProfileUI = obj;
    }
}

public class SteamLobbyManager : MonoBehaviour
{
    public static SteamLobbyManager Instance;

    // Lobby currently joined
    public Lobby CurrentLobby { get; set; }

    public Dictionary<SteamId, SteamPlayerObject> PlayersInLobby { get; set; }

    // Here resides the lobby search result
    public List<Lobby> LobbiesResult;

    // The player who is hosting the game
    public SteamId HostPlayerId;

    private void Awake()
    {

        if (Instance == null)
        {
            DontDestroyOnLoad(this);
            Instance = this;
            PlayersInLobby = new Dictionary<SteamId, SteamPlayerObject>();
            LobbiesResult = new List<Lobby>();
            HostPlayerId = 0;
        } else if (Instance != this)
        {
            Destroy(gameObject);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreatedCallback;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnectedCallback;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeaveCallback;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoinedCallback;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEnteredCallback;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInviteCallback;
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreatedCallback;
        SteamMatchmaking.OnChatMessage += OnChatMessageCallback;
        SteamNetworking.OnP2PSessionRequest += OnP2PSessionRequestCallback;
        SceneManager.sceneLoaded += OnSceneLoadedCallback;
    }

    private void OnApplicationQuit()
    {
        CleanUp();
    }

    #region Help functions

    private void CleanUp()
    {
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreatedCallback;
        SteamMatchmaking.OnLobbyMemberDisconnected -= OnLobbyMemberDisconnectedCallback;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeaveCallback;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoinedCallback;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEnteredCallback;
        SteamMatchmaking.OnLobbyInvite -= OnLobbyInviteCallback;
        SteamMatchmaking.OnLobbyGameCreated -= OnLobbyGameCreatedCallback;
        SteamMatchmaking.OnChatMessage -= OnChatMessageCallback;
        SteamNetworking.OnP2PSessionRequest -= OnP2PSessionRequestCallback;
        SceneManager.sceneLoaded -= OnSceneLoadedCallback;
    }

    public static async Task<Image?> GetAvatar(SteamId id)
    {
        try
        {
            // Get Avatar using await
            return await SteamFriends.GetLargeAvatarAsync(id);
        }
        catch (System.Exception e)
        {
            // If something goes wrong, log it
            Debug.Log(e);
            return null;
        }
    }

    public static Texture2D Covert(Image image)
    {
        // Create a new Texture2D
        var avatar = new Texture2D((int)image.Width, (int)image.Height, TextureFormat.ARGB32, false);

        // Set filter type, or else its really blury
        avatar.filterMode = FilterMode.Trilinear;

        // Flip image
        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                var p = image.GetPixel(x, y);
                avatar.SetPixel(x, (int)image.Height - y, new UnityEngine.Color(p.r / 255.0f, p.g / 255.0f, p.b / 255.0f, p.a / 255.0f));
            }
        }

        avatar.Apply();
        return avatar;
    }

    // Will fil PlayerInLobby list with players that are connected to the lobby
    private void PopulatePlayerList(Lobby lobby)
    {
        CleanPlayerList();
        foreach (Friend member in lobby.Members)
        {
            AddPlayerToList(member.Id, member.Name);
        }

        if(!PlayersInLobby.ContainsKey(SteamManager.Instance.PlayerSteamId))
        {
            AddPlayerToList(SteamManager.Instance.PlayerSteamId, SteamManager.Instance.PlayerName);
        }
    }

    // Clean PlayersInLobby list this will probably get executed when we leave the lobby
    private void CleanPlayerList()
    {
        if(PlayersInLobby != null && PlayersInLobby.Count > 0)
        {
            List<SteamId> Keys = new List<SteamId>(PlayersInLobby.Keys);
            foreach (SteamId key in Keys)
            {
                RemovePlayerFromList(key);
            }
        }
    }

    // Add a player to PlayersInLobby
    private void AddPlayerToList(SteamId id, string name)
    {
        if (PlayersInLobby.ContainsKey(id)) {
            return;
        }
        
        // ProfileUI is set to null
        // Object Instantiation will be done in LobbyScene
        // Precisely in the LobbyUIManager
        PlayersInLobby.Add(id, new SteamPlayerObject(name, null));
    }

    // Remove a player from PlayersInLobby
    // And destroy its UI
    private void RemovePlayerFromList(SteamId id)
    {
        if(!PlayersInLobby.ContainsKey(id))
        {
            return;
        }

        if(PlayersInLobby[id].ProfileUI == null)
        {
            return;
        }

        Destroy(PlayersInLobby[id].ProfileUI);
        PlayersInLobby.Remove(id);
    }

    #endregion

    #region Core functions

    public async Task<bool> CreateLobby(int maxPlayers, string gameName)
    {

        try
        {
            var lobbyOut = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);
            
            if(!lobbyOut.HasValue)
            {
                throw new System.Exception("Lobby created but instantiated correctly");
            }

            lobbyOut.Value.SetPublic();
            lobbyOut.Value.SetJoinable(true);
            lobbyOut.Value.SetData("GameName", gameName);

            return true;

        } catch (System.Exception e)
        {
            QuickLog.Instance.Log("Failed to create Lobby");
            Debug.Log(e);
        }

        return false;

    }

    public async Task<bool> JoinLobby(Lobby lobby)
    {
        RoomEnter joinedLobbySuccess = await lobby.Join();
        if (joinedLobbySuccess != RoomEnter.Success)
        {
            QuickLog.Instance.Log("Failed to join lobby");
            return false;
        }
        return true;
    }

    public void LeaveLobby()
    {
        try
        {
            CurrentLobby.Leave();
        }
        catch
        {
            QuickLog.Instance.Log("Error leaving current lobby");
            return;
        }

        // Left Successfully
        // Load the desired scene
        SceneManager.LoadScene(0);
    }

    public async Task<bool> SearchLobbies(string gameName)
    {
        try
        {
            LobbiesResult.Clear();
            Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithMaxResults(20)
                                                              .WithKeyValue("GameName", gameName)
                                                              .RequestAsync();
            if (lobbies != null)
            {
                foreach (Lobby lobby in lobbies.ToList())
                {
                    LobbiesResult.Add(lobby);
                }
            }
            return true;
        }
        catch (System.Exception e)
        {
            QuickLog.Instance.Log("Error fetching multiplayer lobbies");
            Debug.Log(e);
        }
        
        return false;
    }

    #endregion

    #region Callbacks

    // Executed when a lobby is created
    private void OnLobbyCreatedCallback(Result result, Lobby lobby)
    {
        QuickLog.Instance.Log("OnLobbyCreated Callback");
        if (result == Result.OK)
        {
            CurrentLobby = lobby;
        }
    }

    // Executed only when a member Disconnected
    // Which means when i get Disconnected it will not get executed 
    private void OnLobbyMemberDisconnectedCallback(Lobby lobby, Friend member)
    {
        QuickLog.Instance.Log("OnLobbyMemberDisconnected Callback");
        SteamManager.Instance.CloseP2P(member.Id);
        RemovePlayerFromList(member.Id);
    }

    // Executed only when a member leaves
    // Which means when i leave it will not get executed 
    private void OnLobbyMemberLeaveCallback(Lobby lobby, Friend member)
    {
        QuickLog.Instance.Log("OnLobbyMemberLeave Callback");
        // The idea is when a member leaves the lobby
        // All users in lobby will execute this
        SteamManager.Instance.CloseP2P(member.Id);
        RemovePlayerFromList(member.Id);
        Debug.Log("The owner is : " + CurrentLobby.Owner.Id);
    }

    // Executed when a member joins the lobby
    private void OnLobbyMemberJoinedCallback(Lobby lobby, Friend member)
    {
        QuickLog.Instance.Log("OnLobbyMemberJoined Callback");
        // The idea is when a member joins a lobby
        // All users in lobby will execute this
        SteamManager.Instance.AcceptP2P(member.Id);
        AddPlayerToList(member.Id, member.Name);
    }

    // Executed when we enter the lobby
    private void OnLobbyEnteredCallback(Lobby lobby)
    {
        QuickLog.Instance.Log("OnLobbyEntered Callback");
        CurrentLobby = lobby;

        // Load lobby scene
        SceneManager.LoadScene(1);
    }

    private void OnLobbyInviteCallback(Friend member, Lobby lobby)
    {

    }

    // When Game is started
    private void OnLobbyGameCreatedCallback(Lobby lobby, uint ip, ushort port, SteamId gameServerId)
    {
        Debug.Log("Game is Started by: " + gameServerId.ToString());
        HostPlayerId = gameServerId;

        // Load game scene
        SceneManager.LoadScene(2);
    }

    // Executed when we receive a chat message
    private void OnChatMessageCallback(Lobby lobby, Friend friend, string message)
    {
        // Received chat message
        QuickLog.Instance.Log($"{friend.Name}: {message}");
    }

    private void OnP2PSessionRequestCallback(SteamId user)
    {
        Debug.Log("P2P Request from " + user.ToString());
        SteamManager.Instance.AcceptP2P(user);
    }

    // Executed when a scene is loaded
    private void OnSceneLoadedCallback(Scene scene, LoadSceneMode loadSceneMode)
    {
        // Lobby Scene
        if (scene.buildIndex == 1)
        {
            // When lobby scene is loaded
            // Load players that are in the scene
            PopulatePlayerList(CurrentLobby);
        } else if (scene.buildIndex == 2)
        {
            CleanPlayerList();
            // Instantiate Players
            var packet = P2PPacket.Compose_InstantiatePlayerPacket();
            P2PNetworkSend.SendToAllLobby(CurrentLobby, packet);
        } 
        else // if Any scene else we clean Playerlist
        {
            CleanPlayerList();
        }
    }

    #endregion

}
