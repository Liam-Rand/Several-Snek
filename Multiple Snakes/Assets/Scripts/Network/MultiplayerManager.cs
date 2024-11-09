using CobaPlatinum.DebugTools;
using CobaPlatinum.DebugTools.ExposedFields;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerManager : NetworkBehaviour
{
    #region Singleton
    public static MultiplayerManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of MultiplayerManager found!");
            return;
        }

        instance = this;

        DontDestroyOnLoad(instance.gameObject);

        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;

        playerDisplayName = PlayerPrefs.GetString(PLAYER_PREFS_DISPLAY_NAME, "Snek-" + UnityEngine.Random.Range(100, 1000));
    }
    #endregion

    [ExposedField][SerializeField] private int maxPlayers = 4;

    private NetworkList<PlayerData> playerDataNetworkList;

    private const string PLAYER_PREFS_DISPLAY_NAME = "PlayerDisplayName";
    [ExposedField] private string playerDisplayName;

    //Events
    public event EventHandler OnTryingToConnect;
    public event EventHandler OnFailedToConnect;
    public event EventHandler OnPlayerDataNetworkListChanged;

    //Player Colors
    [SerializeField] private Color[] playerColors;

    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private NetworkSceneManager loadingScreenManager;

    private bool newLobby = true;

    public bool IsNewLobby() { return newLobby; }
    public void SetNewLobby(bool _newLobby) { newLobby = _newLobby;}
    public GameSettings GetGameSettings() { return gameSettings; }
    public NetworkSceneManager GetLoadingScreenManager() { return loadingScreenManager; }
    public string GetPlayerDisplayName() { return playerDisplayName; }
    public void SetPlayerDisplayName(string _playerDisplayName) 
    { 
        playerDisplayName = _playerDisplayName;

        PlayerPrefs.SetString(PLAYER_PREFS_DISPLAY_NAME, playerDisplayName);

        CP_DebugWindow.Log(this, $"Player display name set to: {playerDisplayName}");
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetMaxPlayers() { return maxPlayers; }

    public void StartHost()
    {
        CP_DebugWindow.Log(this, "Starting network host!");

        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_Server_OnClientDisconnectCallback(ulong _clientID)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            PlayerData playerData = playerDataNetworkList[i];

            if(playerData.GetClientID() == _clientID)
            {
                playerDataNetworkList.RemoveAt(i);
            }
        }
    }

    private void NetworkManager_Server_OnClientConnectedCallback(ulong _clientID)
    {
        PlayerData playerData = new PlayerData(_clientID);
        playerData.SetPlayerColorIndex(GetUnusedColor());
        playerDataNetworkList.Add(playerData);

        SetPlayerDisplayNameServerRpc(GetPlayerDisplayName());
        SetPlayerIDServerRpc(AuthenticationService.Instance.PlayerId);
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Scene_CharacterSelect")
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game has already started!";
            CP_DebugWindow.LogError(this, "Connection denied! " + connectionApprovalResponse.Reason);
            return;
        }

        if(NetworkManager.Singleton.ConnectedClientsIds.Count >= maxPlayers)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game is full!";
            CP_DebugWindow.LogError(this, "Connection denied! " + connectionApprovalResponse.Reason);
            return;
        }

        CP_DebugWindow.LogError(this, "Connection approved!");
        connectionApprovalResponse.Approved = true;
    }

    public void StartClient()
    {
        CP_DebugWindow.Log(this, "Starting network client!");
        OnTryingToConnect?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_Client_OnClientConnectedCallback(ulong _clientID)
    {
        SetPlayerDisplayNameServerRpc(GetPlayerDisplayName());
        SetPlayerIDServerRpc(AuthenticationService.Instance.PlayerId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerDisplayNameServerRpc(string _playerDipslayName, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.SetPlayerDisplayName(_playerDipslayName);

        playerDataNetworkList[playerDataIndex] = playerData;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIDServerRpc(string _playerID, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.SetPlayerID(_playerID);

        playerDataNetworkList[playerDataIndex] = playerData;
    }

    private void NetworkManager_Client_OnClientDisconnectCallback(ulong _clientID)
    {
        OnFailedToConnect?.Invoke(this, EventArgs.Empty);
    }

    public bool IsPlayerIndexConnected(int _playerIndex)
    {
        return _playerIndex < playerDataNetworkList.Count;
    }

    public List<PlayerData> GetAllPlayerData() 
    { 
        List<PlayerData> players = new List<PlayerData>();
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            players.Add(playerDataNetworkList[i]);
        }
        return players;
    }

    public void SetPlayerData(int _playerDataIndex,  PlayerData playerData)
    {
        playerDataNetworkList[_playerDataIndex] = playerData;
    }

    public int GetPlayerDataIndexFromClientId(ulong _clientID)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].GetClientID() == _clientID)
                return i;
        }
        return -1;
    }

    public PlayerData GetPlayerDataFromClientId(ulong _clientID)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if(playerData.GetClientID() == _clientID)
                return playerData;
        }
        return default;
    }

    public PlayerData GetLocalPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    public PlayerData GetPlayerDataFromIndex(int _playerIndex)
    {
        return playerDataNetworkList[_playerIndex];
    }

    public Color GetColorFromIndex(int _colorIndex)
    {
        return playerColors[_colorIndex];
    }

    public Color[] GetPlayerColors() { return playerColors; }

    public void SetPlayerColor(int _colorIndex)
    {
        ChangePlayerColorServerRpc(_colorIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerColorServerRpc(int _colorIndex, ServerRpcParams serverRpcParams = default)
    {
        if (!IsColorAvailable(_colorIndex))
            return;

        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.SetPlayerColorIndex(_colorIndex);

        playerDataNetworkList[playerDataIndex] = playerData;

        CP_DebugWindow.Log(this, $"Changed player color to index: {_colorIndex}");
    }

    public void SetPlayerAlive(ulong _clientID, bool _isAlive)
    {
        ChangePlayerIsAliveServerRpc(_clientID, _isAlive);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerIsAliveServerRpc(ulong _clientID, bool _isAlive, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(_clientID);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.SetPlayerAlive(_isAlive);

        playerDataNetworkList[playerDataIndex] = playerData;

        CP_DebugWindow.Log(this, $"Changed Player isAlive: {_isAlive}");
    }
    public void SetPlayerKillIndex(ulong _clientID, int _killIndex)
    {
        SetPlayerKillIndexServerRpc(_clientID, _killIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerKillIndexServerRpc(ulong _clientID, int _killIndex, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(_clientID);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.SetKillIndex(_killIndex);

        playerDataNetworkList[playerDataIndex] = playerData;

        CP_DebugWindow.Log(this, $"Changed Player killIndex: {_killIndex}");
    }

    public void SetPlayerKillCount(ulong _clientID, int _killCount)
    {
        SetPlayerKillCountServerRpc(_clientID, _killCount);
    }
    public void IncrementPlayerKillCount(ulong _clientID)
    {
        SetPlayerKillCountServerRpc(_clientID, GetPlayerDataFromClientId(_clientID).GetKillCount() + 1);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerKillCountServerRpc(ulong _clientID, int _killCount, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(_clientID);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.SetKillCount(_killCount);

        playerDataNetworkList[playerDataIndex] = playerData;

        CP_DebugWindow.Log(this, $"Changed Player killCount: {_killCount}");
    }

    public bool IsColorAvailable(int _colorIndex)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.GetPlayerColorIndex() == _colorIndex)
                return false;
        }
        return true;
    }

    private int GetUnusedColor()
    {
        for(int i = 0; i < playerColors.Length; i++) 
        {
            if (IsColorAvailable(i))
                return i;
        }
        return -1;
    }

    public void KickPlayer(ulong _cliendId)
    {
        CP_DebugWindow.Log(this, $"Kicking player: {_cliendId}");
        NetworkManager.Singleton.DisconnectClient(_cliendId);
        NetworkManager_Server_OnClientDisconnectCallback(_cliendId);
    }

    public void ResetPlayerData()
    {
        foreach(PlayerData playerData in playerDataNetworkList)
        {
            SetPlayerAlive(playerData.GetClientID(), true);
            SetPlayerKillCount(playerData.GetClientID(), 0);
        }
    }

    public void DisconnectClient()
    {
        CP_DebugWindow.Log(this, $"Disconnecting from server!");
        MultiplayerLobby.instance.LeaveLobby();
        NetworkManager.Singleton.Shutdown();
        NetworkSceneManager.instance.LoadScene("Scene_MainMenu", false);
    }
}
