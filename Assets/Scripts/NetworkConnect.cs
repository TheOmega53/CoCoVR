using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using TMPro;
using Unity.Services.Vivox;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
public class NetworkConnect : MonoBehaviour
{
    public int maxConnection = 20;
    public UnityTransport transport;

    public TextMeshProUGUI debugText;

    private Lobby currentLobby;
    private float heartBeatTimer;

    public GameObject playerInputOrigin;


    //These variables should be set to the projects Vivox credentials if the authentication package is not being used   
    //Credentials are available on the Vivox Developer Portal (developer.vivox.com) or the Unity Dashboard (dashboard.unity3d.com), depending on where the organization and project were made
    [SerializeField]
    string _key;
    [SerializeField]
    string _issuer;
    [SerializeField]
    string _domain;
    [SerializeField]
    string _server;

    private bool gameHasStarted;
    public Dictionary<ulong, ClientData> ClientData { get; private set; }


    [Header("Settings")]
    [SerializeField] private string gameplaySceneName = "Gameplay";
    [SerializeField] private string characterSelectSceneName = "Character Select";

    //public static NetworkConnect Instance { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public async Task InitializeUnityServices()
    {
        var options = new InitializationOptions();
        if (CheckManualCredentials())
        {
            options.SetVivoxCredentials(_server, _domain, _issuer, _key);
        }

        try
        {
            await UnityServices.InitializeAsync(options);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (Exception e)
        {
            if (debugText != null) { debugText.text += "Initialization failed. Error: " + e + "\n"; };
        }
        if (debugText != null) { debugText.text += "Initialized. \n"; };
    }

    bool CheckManualCredentials()
    {
        return !(string.IsNullOrEmpty(_issuer) && string.IsNullOrEmpty(_domain) && string.IsNullOrEmpty(_server));
    }

    public async Task JoinOrCreate()
    {
        try
        {
            await Join();
        } catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
            Debug.LogError("No lobby found, creating lobby");
            if (debugText != null) { debugText.text += "No lobby found, creating lobby.\n"; };
            await Create();
        }

    }
    public async Task Create()
    {

        Debug.LogError("creating lobby");
        if (debugText != null) { debugText.text += "creating lobby.\n"; };

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnection);
        string newJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        Debug.LogError("New Join Code: " + newJoinCode);
        if (debugText != null) { debugText.text += "New Join Code: " + newJoinCode + "\n"; };

        transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);


        CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
        lobbyOptions.IsPrivate = false;
        lobbyOptions.Data = new Dictionary<string, DataObject>();
        DataObject dataObject = new DataObject(DataObject.VisibilityOptions.Public, newJoinCode);
        lobbyOptions.Data.Add("JOIN_CODE", dataObject);

        currentLobby = await Lobbies.Instance.CreateLobbyAsync("Lobby Name", maxConnection, lobbyOptions);


        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnServerStarted += OnNetworkReady;

        ClientData = new Dictionary<ulong, ClientData>();

        NetworkManager.Singleton.StartHost();
        playerInputOrigin.layer = 7;
    }
    public async Task Join()
    {
        // Quick-join a random lobby with a maximum capacity of 10 or more players.
        QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();

        options.Filter = new List<QueryFilter>(){
                new QueryFilter(
                    field: QueryFilter.FieldOptions.MaxPlayers,
                    op: QueryFilter.OpOptions.GE,
                    value: ""+maxConnection)
            };

        Debug.LogError("Trying to join Lobby");
        if (debugText != null) { debugText.text += "Trying to join Lobby \n"; };
        currentLobby = await Lobbies.Instance.QuickJoinLobbyAsync(options);
        string relayJoinCode = currentLobby.Data["JOIN_CODE"].Value;

        Debug.LogError("Got join code: " + relayJoinCode);
        if (debugText != null) { debugText.text += "Got Join code" + relayJoinCode + "\n"; };

        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

        transport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);

        NetworkManager.Singleton.StartClient();

        playerInputOrigin.layer = 8;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (ClientData.Count >= 2 || gameHasStarted)
        {
            response.Approved = false;
            return;
        }

        response.Approved = true;
        response.CreatePlayerObject = false;
        response.Pending = false;

        ClientData[request.ClientNetworkId] = new ClientData(request.ClientNetworkId);

        Debug.Log($"added client {request.ClientNetworkId}");

    }


    private void OnNetworkReady()
    {
        //scene change
        // NetworkManager.Singleton.SceneManager.LoadScene(characterSelectSceneName, LoadSceneMode.Single);

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (ClientData.ContainsKey(clientId))
        {
            if (ClientData.Remove(clientId))
            {
                Debug.Log($"Removed client {clientId}");
            }
        }
    }

    public void SetCharacter(ulong clientId, int characterId)
    {
        if(ClientData.TryGetValue(clientId, out ClientData data))
        {
            data.characterId = characterId;
        }
    }

    public void StartGame()
    {
        gameHasStarted = true;

        //change scene
        NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
    }

    private void Update()
    {
        if(heartBeatTimer > 15)
        {
            heartBeatTimer -= 15;

            if(currentLobby != null && currentLobby.HostId == AuthenticationService.Instance.PlayerId)
            {
                LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            }

            heartBeatTimer += Time.deltaTime;
        }
    }
}
