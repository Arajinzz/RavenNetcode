using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class SteamLobbyManager : MonoBehaviour
{
    public static SteamLobbyManager Instance;

    // Lobby currently joined
    public Lobby CurrentLobby { get; set; }

    public Dictionary<SteamId, GameObject> PlayersInLobby { get; set; }

    // Here resides the lobby search result
    public List<Lobby> LobbiesResult;

    [SerializeField]
    GameObject LobbyUI;
    [SerializeField]
    GameObject PlayerProfileUI;
    [SerializeField]
    GameObject PlayerListUI;

    private void Awake()
    {

        if (Instance == null)
        {
            DontDestroyOnLoad(this);
            Instance = this;
            PlayersInLobby = new Dictionary<SteamId, GameObject>();
            LobbiesResult = new List<Lobby>();
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
    }

    private void OnDisable()
    {
        CleanUp();
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
    }

    private static async Task<Image?> GetAvatar(SteamId id)
    {
        try
        {
            // Get Avatar using await
            return await SteamFriends.GetLargeAvatarAsync(id);
        }
        catch (System.Exception e)
        {
            // If something goes wrong, log it
            QuickLog.Instance.Log(e);
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

    private async void AddPlayerToList(SteamId id, string name)
    {
        if (PlayersInLobby.ContainsKey(id)) {
            return;
        }
        
        Image? avatar = await GetAvatar(id);
        Texture2D image = Covert(avatar.Value);
        
        GameObject obj = Instantiate(PlayerProfileUI, PlayerListUI.transform);
        obj.GetComponentInChildren<UnityEngine.UI.RawImage>().texture = image;
        obj.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = name;

        PlayersInLobby.Add(id, obj);
    }

    private void RemovePlayerFromList(SteamId id)
    {
        if(!PlayersInLobby.ContainsKey(id))
        {
            return;
        }
        GameObject obj = PlayersInLobby[id];
        Destroy(obj);
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
            QuickLog.Instance.Log(e);
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
        CleanPlayerList();
        LobbyUI.SetActive(false);
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
            QuickLog.Instance.Log(e);
        }
        
        return false;
    }

    #endregion

    #region Callbacks

    private void OnLobbyCreatedCallback(Result result, Lobby lobby)
    {
        QuickLog.Instance.Log("OnLobbyCreated Callback");
        if (result == Result.OK)
        {
            CurrentLobby = lobby;
        }
    }

    private void OnLobbyMemberDisconnectedCallback(Lobby lobby, Friend member)
    {
        QuickLog.Instance.Log("OnLobbyMemberDisconnected Callback");
        SteamManager.Instance.CloseP2P(member.Id);
        RemovePlayerFromList(member.Id);
    }

    private void OnLobbyMemberLeaveCallback(Lobby lobby, Friend member)
    {
        QuickLog.Instance.Log("OnLobbyMemberLeave Callback");
        SteamManager.Instance.CloseP2P(member.Id);
        RemovePlayerFromList(member.Id);
    }

    private void OnLobbyMemberJoinedCallback(Lobby lobby, Friend member)
    {
        QuickLog.Instance.Log("OnLobbyMemberJoined Callback");
        SteamManager.Instance.AcceptP2P(member.Id);
        AddPlayerToList(member.Id, member.Name);
    }

    private void OnLobbyEnteredCallback(Lobby lobby)
    {
        QuickLog.Instance.Log("OnLobbyEntered Callback");
        CurrentLobby = lobby;
        PopulatePlayerList(lobby);
        LobbyUI.SetActive(true);
    }

    private void OnLobbyInviteCallback(Friend member, Lobby lobby)
    {

    }

    private void OnLobbyGameCreatedCallback(Lobby lobby, uint ip, ushort port, SteamId gameServerId)
    {

    }

    private void OnChatMessageCallback(Lobby lobby, Friend friend, string message)
    {
        // Received chat message
        QuickLog.Instance.Log($"{friend.Name}: {message}");
    }

    #endregion

}
