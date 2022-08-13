using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SteamSocketManager : SocketManager
{
    public override void OnConnecting(Connection connection, ConnectionInfo data)
    {
        base.OnConnecting(connection, data);//The base class will accept the connection
        Debug.Log("SocketManager OnConnecting");
    }

    public override void OnConnected(Connection connection, ConnectionInfo data)
    {
        base.OnConnected(connection, data);
        Debug.Log("New player connecting");
    }

    public override void OnDisconnected(Connection connection, ConnectionInfo data)
    {
        base.OnDisconnected(connection, data);
        Debug.Log("Player disconnected");
    }

    public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        // Socket server received message, forward on message to all members of socket server
        SteamManager.Instance.RelaySocketMessageReceived(data, size, connection.Id);
        Debug.Log("Socket message received");
    }
}

// CONNECTION MANAGER that enables all players to connect to Socket Server
public class SteamConnectionManager : ConnectionManager
{
    public override void OnConnected(ConnectionInfo info)
    {
        base.OnConnected(info);
        Debug.Log("ConnectionOnConnected");
    }

    public override void OnConnecting(ConnectionInfo info)
    {
        base.OnConnecting(info);
        Debug.Log("ConnectionOnConnecting");
    }

    public override void OnDisconnected(ConnectionInfo info)
    {
        base.OnDisconnected(info);
        Debug.Log("ConnectionOnDisconnected");
        Debug.Log(info.EndReason.ToString());
    }

    public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        // Message received from socket server, delegate to method for processing
        SteamManager.Instance.ProcessMessageFromSocketServer(data, size);
        Debug.Log("Connection Got A Message");
    }
}

public class SteamManager : MonoBehaviour
{

    public static SteamManager Instance;
    private static uint AppId = 480u;

    public string PlayerName { get; set; }
    public SteamId PlayerSteamId { get; set; }

    private string m_PlayerSteamIdString;
    public string PlayerSteamIdString { get => m_PlayerSteamIdString; }

    private Friend m_LobbyPartner;
    public Friend LobbyPartner { get => m_LobbyPartner; set => m_LobbyPartner = value; }
    public SteamId OpponentSteamId { get; set; }
    public bool LobbyPartnerDisconnected { get; set; }

    public List<Lobby> LobbiesList;
    public Lobby CurrentLobby;
    private Lobby HostedMultiplayerLobby;

    [SerializeField]
    public GameObject LobbyTestUI;

    [SerializeField]
    public GameObject ProfileUI;

    [SerializeField]
    public GameObject PlayerList;


    SteamSocketManager steamSocketManager;
    SteamConnectionManager steamConnectionManager;
    bool activeSteamSocketServer = false;
    bool activeSteamSocketConnection = false;
    bool isHost = false;

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
                m_PlayerSteamIdString = PlayerSteamId.ToString();
                LobbiesList = new List<Lobby>();

                // Helpful to reduce time to use SteamNetworkingSockets later
                SteamNetworkingUtils.InitRelayNetworkAccess();

            } catch (Exception e)
            {
                Debug.Log("Error connecting to steam");
                Debug.Log(e);
            }

        } else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {

        // Callbacks
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreatedCallback;
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreatedCallback;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEnteredCallback;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoinedCallback;
        SteamMatchmaking.OnChatMessage += OnChatMessageCallback;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnectedCallback;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeaveCallback;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequestedCallback;
        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    private void Update()
    {
        SteamClient.RunCallbacks();

        try
        {
            if (activeSteamSocketServer)
            {
                steamSocketManager.Receive();
            }
            if (activeSteamSocketConnection)
            {
                steamConnectionManager.Receive();
            }
        }
        catch
        {
            Debug.Log("Error receiving data on socket/connection");
        }
    }

    private void OnApplicationQuit()
    {
        GameCleanup();
    }


    ///////////////////////////////// MY FUNCTIONS ////////////////////////////////////////////////

    private void CreateSteamSocketServer()
    {
        steamSocketManager = SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>(0);
        // Host needs to connect to own socket server with a ConnectionManager to send/receive messages
        // Relay Socket servers are created/connected to through SteamIds rather than "Normal" Socket Servers which take IP addresses
        Debug.Log(PlayerSteamId.ToString());
        steamConnectionManager = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(PlayerSteamId);
        activeSteamSocketServer = true;
        activeSteamSocketConnection = true;
        isHost = true;
    }

    private void JoinSteamSocketServer()
    {
        Debug.Log("joining socket server");
        Debug.Log(OpponentSteamId.ToString());
        steamConnectionManager = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(OpponentSteamId, 0);
        activeSteamSocketServer = false;
        activeSteamSocketConnection = true;
    }

    private void LeaveSteamSocketServer()
    {
        activeSteamSocketServer = false;
        activeSteamSocketConnection = false;
        try
        {
            // Shutdown connections/sockets. I put this in try block because if player 2 is leaving they don't have a socketManager to close, only connection
            steamConnectionManager.Close();
            steamSocketManager.Close();
        }
        catch
        {
            Debug.Log("Error closing socket server / connection manager");
        }
    }

    public void RelaySocketMessageReceived(IntPtr message, int size, uint connectionSendingMessageId)
    {
        try
        {
            // Loop to only send messages to socket server members who are not the one that sent the message
            for (int i = 0; i < steamSocketManager.Connected.Count; i++)
            {
                if (steamSocketManager.Connected[i].Id != connectionSendingMessageId)
                {
                    Result success = steamSocketManager.Connected[i].SendMessage(message, size);
                    if (success != Result.OK)
                    {
                        Result retry = steamSocketManager.Connected[i].SendMessage(message, size);
                    }
                }
            }
        }
        catch
        {
            Debug.Log("Unable to relay socket server message");
        }
    }

    public bool SendMessageToSocketServer(byte[] messageToSend)
    {
        try
        {
            // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
            int sizeOfMessage = messageToSend.Length;
            IntPtr intPtrMessage = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeOfMessage);
            System.Runtime.InteropServices.Marshal.Copy(messageToSend, 0, intPtrMessage, sizeOfMessage);
            Result success = steamConnectionManager.Connection.SendMessage(intPtrMessage, sizeOfMessage, SendType.Reliable);
            if (success == Result.OK)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
                return true;
            }
            else
            {
                // RETRY
                Result retry = steamConnectionManager.Connection.SendMessage(intPtrMessage, sizeOfMessage, SendType.Reliable);
                System.Runtime.InteropServices.Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
                if (retry == Result.OK)
                {
                    return true;
                }
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log("Unable to send message to socket server");
            return false;
        }
    }

    public void ProcessMessageFromSocketServer(IntPtr messageIntPtr, int dataBlockSize)
    {
        try
        {
            byte[] message = new byte[dataBlockSize];
            System.Runtime.InteropServices.Marshal.Copy(messageIntPtr, message, 0, dataBlockSize);
            string messageString = System.Text.Encoding.UTF8.GetString(message);

            // Do something with received message

        }
        catch
        {
            Debug.Log("Unable to process message from socket server");
        }
    }

    public void LeaveLobby()
    {
        try
        {
            CurrentLobby.Leave();
            LobbyTestUI.SetActive(false);
        } catch
        {
            Debug.Log("Error leaving current lobby");
        }

        try
        {
            SteamNetworking.CloseP2PSessionWithUser(OpponentSteamId);
        } catch
        {
            Debug.Log("Error closing P2P session with opponent");
        }

    }

    private void OtherLobbyMemberLeft(Friend friend)
    {
        if (friend.Id != PlayerSteamId)
        {
            Debug.Log("Oponnent has left the lobby");
            LobbyPartnerDisconnected = true;

            try
            {
                SteamNetworking.CloseP2PSessionWithUser(friend.Id);
            } catch
            {
                Debug.Log("Unable to update disconnected player nameplate / process disconnect cleanly");
            }
        }
    }

    private static async Task<Image?> GetAvatar(SteamId id)
    {
        try
        {
            // Get Avatar using await
            return await SteamFriends.GetLargeAvatarAsync(id);
        }
        catch (Exception e)
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

    private void AcceptP2P(SteamId opponentId)
    {
        try
        {
            // For two players to send P2P packets to each other, they each must call this on the other player
            SteamNetworking.AcceptP2PSessionWithUser(opponentId);
        }
        catch
        {
            Debug.Log("Unable to accept P2P Session with user");
        }
    }

    private void GameCleanup()
    {
        LeaveLobby();
        SteamClient.Shutdown();
    }

    public async void InstantiateProfileUI(SteamId id, string name)
    {
        Image? avatar = await GetAvatar(id);
        Texture2D image = Covert(avatar.Value);
        GameObject pro = Instantiate(ProfileUI, PlayerList.transform);
        pro.GetComponentInChildren<UnityEngine.UI.RawImage>().texture = image;
        pro.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = name;
    }

    public async Task<bool> RefreshMultiplayerLobbies()
    {
        try
        {
            LobbiesList.Clear();
            Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithMaxResults(20).WithKeyValue("GameName", "TestingSteamworks").RequestAsync();
            if (lobbies != null)
            {
                foreach (Lobby lobby in lobbies.ToList())
                {
                    LobbiesList.Add(lobby);
                }
            }
            return true;
        }
        catch
        {
            Debug.Log("Error fetching multiplayer lobbies");
            return true;
        }
    }

    public async Task<bool> CreateLobby(int lobbyParameters)
    {
        try
        {
            var createLobbyOutput = await SteamMatchmaking.CreateLobbyAsync(2);
            if (!createLobbyOutput.HasValue)
            {
                Debug.Log("Lobby created but not correctly instantiated");
                throw new Exception();
            }

            LobbyPartnerDisconnected = false;
            HostedMultiplayerLobby = createLobbyOutput.Value;
            HostedMultiplayerLobby.SetPublic();
            HostedMultiplayerLobby.SetJoinable(true);
            HostedMultiplayerLobby.SetData("GameName", "TestingSteamworks");

            CurrentLobby = HostedMultiplayerLobby;

            CreateSteamSocketServer();
            Debug.Log("Server Socket Created");

            return true;
        }
        catch (Exception exception)
        {
            Debug.Log("Failed to create multiplayer lobby");
            Debug.Log(exception.ToString());
            return false;
        }
    }

    public async Task<bool> JoinLobby(Lobby lobby)
    {
        RoomEnter joinedLobbySuccess = await lobby.Join();
        if (joinedLobbySuccess != RoomEnter.Success)
        {
            Debug.Log("failed to join lobby");
            return false;
        }
        Debug.Log("JOIN SOCKET CRAETED");
        JoinSteamSocketServer();
        return true;
    }

    #region CALLBACKS

    private void OnLobbyMemberDisconnectedCallback(Lobby lobby, Friend friend)
    {
        Debug.Log($"{friend.Name} Disconnected");
        OtherLobbyMemberLeft(friend);
    }

    private void OnLobbyMemberLeaveCallback(Lobby lobby, Friend friend)
    {
        OtherLobbyMemberLeft(friend);
    }

    private void OnLobbyMemberJoinedCallback(Lobby lobby, Friend friend)
    {
        // The lobby member joined
        Debug.Log("someone else joined lobby");
        if (friend.Id != PlayerSteamId)
        {
            LobbyPartner = friend;
            OpponentSteamId = friend.Id;
            AcceptP2P(OpponentSteamId);
            LobbyPartnerDisconnected = false;

            //LobbyTestUI.SetActive(true);
            SceneManager.LoadScene(1);
            InstantiateProfileUI(friend.Id, friend.Name);
        }
    }

    private void OnLobbyCreatedCallback(Result result, Lobby lobby)
    {
        // Lobby was created
        LobbyPartnerDisconnected = false;
        if (result != Result.OK)
        {
            Debug.Log("lobby creation result not ok");
            Debug.Log(result.ToString());
            return;
        }


        //LobbyTestUI.SetActive(true);
        SceneManager.LoadScene(1);
        InstantiateProfileUI(PlayerSteamId, PlayerName);
    }

    private void OnLobbyGameCreatedCallback(Lobby lobby, uint ip, ushort port, SteamId steamId)
    {
        AcceptP2P(OpponentSteamId);
        // Scene to load
    }

    private void OnLobbyEnteredCallback(Lobby lobby)
    {
        Debug.Log("Lobby entered");
        lobby.SendChatString("incoming player info");
        // You joined the lobby
        if (lobby.MemberCount != 1) // I do this because this callback triggers on host, I only wanted to use for players joining after host
        {
            // You will need to have gotten OpponentSteamId from various methods before (lobby data, joined invite, etc)
            AcceptP2P(OpponentSteamId);

            // Examples of things to do
            lobby.SendChatString("incoming player info");

            //LobbyTestUI.SetActive(true);
            SceneManager.LoadScene(1);
            InstantiateProfileUI(PlayerSteamId, PlayerName);
        }
    }

    private async void OnGameLobbyJoinRequestedCallback(Lobby joinedLobby, SteamId id)
    {
        // Attempt to join lobby
        RoomEnter joinedLobbySuccess = await joinedLobby.Join();
        if (joinedLobbySuccess != RoomEnter.Success)
        {
            Debug.Log("failed to join lobby");
        }
        else
        {
            // This was hacky, I didn't have clean way of getting lobby host steam id when joining lobby from game invite from friend 
            foreach (Friend friend in SteamFriends.GetFriends())
            {
                if (friend.Id == id)
                {
                    m_LobbyPartner = friend;
                    break;
                }
            }
            CurrentLobby = joinedLobby;
            OpponentSteamId = id;
            LobbyPartnerDisconnected = false;
            AcceptP2P(OpponentSteamId);
            // Maybe load a scene here

            JoinSteamSocketServer();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {

    }

    private void OnChatMessageCallback(Lobby lobby, Friend friend, string message)
    {
        // Received chat message
        if (friend.Id != PlayerSteamId)
        {
            Debug.Log("incoming chat message");
            Debug.Log(message);
        }
    }

    #endregion

    string myLog;
    Queue myLogQueue = new Queue();

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
        GameCleanup();
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        myLog = logString;
        string newString = "\n [" + type + "] : " + myLog;
        myLogQueue.Enqueue(newString);
        if (type == LogType.Exception)
        {
            newString = "\n" + stackTrace;
            myLogQueue.Enqueue(newString);
        }
        myLog = string.Empty;
        foreach (string mylog in myLogQueue)
        {
            myLog += mylog;
        }
    }

    void OnGUI()
    {
        GUILayout.Label(myLog);
    }

}
