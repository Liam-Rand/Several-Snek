using CobaPlatinum.DebugTools;
using CobaPlatinum.DebugTools.ExposedFields;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerLobby : MonoBehaviour
{
    #region Singleton
    public static MultiplayerLobby instance;

    private void Awake()
    {
        if (instance != null)
        {
            CP_DebugWindow.LogWarning(this, "More than one instance of MultiplayerLobby found!");

            return;
        }

        instance = this;

        DontDestroyOnLoad(instance.gameObject);

        InitializeUnityAuthentication();
    }
    #endregion

    private const string KEY_CAN_QUICK_JOIN = "CanQuickJoin";
    private const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";
    
    private Lobby joinedLobby;
    [ExposedField] private float heartBeatTimer;

    [ExposedField][SerializeField] private int maxActiveGames = 0;
    [ExposedField] private int activeGames = 0;
    private float gamesCountTimer;

    public event EventHandler OnCreateLobbyStarted;
    public event EventHandler OnCreateLobbyFailed;

    public event EventHandler OnJoinStarted;
    public event EventHandler OnQuickJoinFailed;
    public event EventHandler OnJoinFailed;

    public event EventHandler OnGamesCountChanged;

    private async void InitializeUnityAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(UnityEngine.Random.Range(0, 10000).ToString());

            await UnityServices.InitializeAsync();

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public int GetActiveGames(){ return activeGames; }
    public int GetMaxActiveGames() { return maxActiveGames; }

    public bool AreActiveGamesAvailable()
    {
        MultiplayerLobby.instance.UpdateNumberOfActiveGames();

        if (GetActiveGames() >= GetMaxActiveGames())
            return false;
        else
            return true;
    }

    private async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MultiplayerManager.instance.GetMaxPlayers() - 1);

            return allocation;
        }
        catch (RelayServiceException e)
        {
            CP_DebugWindow.LogError(this, e);

            return default;
        }
    }

    private async Task<string> GetRelayJoinCode(Allocation _allocation)
    {
        try
        {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(_allocation.AllocationId);

            return relayJoinCode;
        }
        catch(RelayServiceException e) 
        {
            CP_DebugWindow.LogError(this, e);
            return default;
        }
    }

    private async Task<JoinAllocation> JoinRelay(string _joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(_joinCode);

            CP_DebugWindow.Log(this, "Unity relay allocated and joined!");

            return joinAllocation;
        }
        catch(RelayServiceException e)
        {
            CP_DebugWindow.LogError(this, e);
            return default;
        }
    }

    public async void CreateLobby(string _lobbyName, bool _isPrivate)
    {
        OnCreateLobbyStarted?.Invoke(this, EventArgs.Empty);

        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions();

            options.IsPrivate = false;

            options.Data = new Dictionary<string, DataObject>()
            {
                {
                    KEY_CAN_QUICK_JOIN, new DataObject(
                        visibility: DataObject.VisibilityOptions.Public,
                        value: _isPrivate.ToString(),
                        index: DataObject.IndexOptions.S1)
                }
            };

            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(_lobbyName, MultiplayerManager.instance.GetMaxPlayers(), options);

            Allocation allocation = await AllocateRelay();

            string relayJoinCode = await GetRelayJoinCode(allocation);

            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>()
                {
                    {
                        KEY_RELAY_JOIN_CODE, new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: relayJoinCode)
                    }
                }
            });

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

            CP_DebugWindow.Log(this, $"New multiplayer lobby created! \n\t Private: {_isPrivate}");

            MultiplayerManager.instance.StartHost();
            NetworkSceneManager.instance.LoadNetworkScene("Scene_CharacterSelect", false);
        }
        catch(LobbyServiceException e)
        {
            CP_DebugWindow.LogError(this, e);
            OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void QuickJoin()
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);

        try
        {
            CP_DebugWindow.Log(this, "Searching for random public lobby!");
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();

            options.Filter = new List<QueryFilter>()
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.S1,
                    op: QueryFilter.OpOptions.EQ,
                    value: "True"
                    )
            };

            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);

            string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            CP_DebugWindow.Log(this, "Joined random public lobby!");

            MultiplayerManager.instance.StartClient();
        }
        catch (LobbyServiceException e) 
        {
            CP_DebugWindow.LogError(this, e);
            OnQuickJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void JoinWithCode(string _gameCode)
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);

        try
        {
            CP_DebugWindow.Log(this, $"Attempting to join lobby with code: {_gameCode}!");
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(_gameCode);

            string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            CP_DebugWindow.Log(this, "Successfuly joined lobby!");
            MultiplayerManager.instance.StartClient();
        }
        catch (LobbyServiceException e) 
        {
            CP_DebugWindow.LogError(this, e);
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Update()
    {
        HandleHeartbeat();
        HandlePeriodicGamesCount();
    }

    private void HandlePeriodicGamesCount()
    {
        if (!UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Equals("Scene_Lobby")) return;

        gamesCountTimer -= Time.deltaTime;

        if( gamesCountTimer <= 0f) 
        {
            float gamesCountTimerMax = 5f;
            gamesCountTimer = gamesCountTimerMax;
            UpdateNumberOfActiveGames();
        }
    }

    private void HandleHeartbeat()
    {
        if(IsLobbyHost())
        {
            heartBeatTimer -= Time.deltaTime;

            if(heartBeatTimer <= 0f) 
            {
                float heartBeatTimerMax = 15f;
                heartBeatTimer = heartBeatTimerMax;

                LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    public async void UpdateNumberOfActiveGames()
    {
        if (joinedLobby != null || !AuthenticationService.Instance.IsSignedIn) return;

        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
            activeGames = queryResponse.Results.Count;

            OnGamesCountChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (LobbyServiceException e) 
        {
            CP_DebugWindow.LogError(this, e);
        }
    }

    public async void DeleteLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);

                joinedLobby = null;
            }
            catch(LobbyServiceException e)
            {
                CP_DebugWindow.LogError(this, e);
            }
        }
    }

    public async void LeaveLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                joinedLobby = null;
            }
            catch (LobbyServiceException e)
            {
                CP_DebugWindow.LogError(this, e);
            }
        }
    }

    public async void KickPlayer(string _playerID)
    {
        if (IsLobbyHost())
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, _playerID);
            }
            catch (LobbyServiceException e)
            {
                CP_DebugWindow.LogError(this, e);
            }
        }
    }

    public Lobby GetLobby() { return joinedLobby; }
}
