using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectManager : NetworkBehaviour
{
    private Dictionary<ulong, bool> playerReadyDictionary;

    [Header("Character Select Window")]
    [SerializeField] private GameObject characterSelectPanel;

    [Header("Character Settings Window")]
    [SerializeField] private GameObject gameSettingsPanel;
    [SerializeField] private ButtonListSelectUI gameModeSelect;
    [SerializeField] private ButtonListSelectUI maxApplesSelect;
    [SerializeField] GameObject enablePowerupsToggle;
    [SerializeField] private int maxApplesPossible = 12;
    [SerializeField] private int defaultMaxApples = 4;
    [SerializeField] private ButtonListSelectUI maxPowerupsSelect;
    [SerializeField] private int maxPowerupsPossible = 12;
    [SerializeField] private int defaultMaxPowerups = 4;
    [SerializeField] private TextMeshProUGUI gameModeDescText;
    [SerializeField] GameObject enableObstaclesToggle;
    [SerializeField] GameObject enableMapWrappingToggle;
    [SerializeField] GameObject enableStarvingSneksToggle;

    [Space]
    [SerializeField] private TextMeshProUGUI gameModeSelectIndicatorText;
    [SerializeField] private TextMeshProUGUI maxApplesSelectIndicatorText;
    [SerializeField] private ReadyIndicator enablePowerupsIndicator;
    [SerializeField] private TextMeshProUGUI maxPowerupsSelectIndicatorText;
    [SerializeField] private ReadyIndicator enableObstaclesIndicator;
    [SerializeField] private ReadyIndicator enableMapWrappingIndicator;
    [SerializeField] private ReadyIndicator enableStarvingSneksIndicator;

    [Header("Game Name Title")]
    [SerializeField] private RectTransform gameNameLayout;
    [SerializeField] private TextMeshProUGUI gameNameText;
    [SerializeField] private RectTransform joinCodeLayout;
    [SerializeField] private GameObject lobbyCodeCopiedMessage;
    [SerializeField] private TextMeshProUGUI joinCodeText;

    [Header("Player Info Panels")]
    [SerializeField] private PlayerInfoPanel[] playerInfoPanels;

    [Header("Player Color Selection")]
    [SerializeField] private Transform colorSelectionGroup;
    [SerializeField] private GameObject colorSelectToggle;

    [Header("Message Screen")]
    [SerializeField] private MessageWindow messageWindow;

    private void Awake()
    {
        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    private void Start()
    {
        MultiplayerManager.instance.OnPlayerDataNetworkListChanged += MultiplayerManager_OnPlayerDataNetworkListChanged;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

        MultiplayerManager.instance.GetGameSettings().GetNetworkGameMode().OnValueChanged += GameSettings_OnGameModeChanged;
        MultiplayerManager.instance.GetGameSettings().GetNetworkMaxApples().OnValueChanged += GameSettings_OnMaxApplesChanged;
        MultiplayerManager.instance.GetGameSettings().NetworkPowerupsEnabled().OnValueChanged += GameSettings_OnPowerupsEnabledChanged;
        MultiplayerManager.instance.GetGameSettings().GetNetworkMaxPowerups().OnValueChanged += GameSettings_OnMaxPowerupsChanged;
        MultiplayerManager.instance.GetGameSettings().NetworkObstaclesEnabled().OnValueChanged += GameSettings_OnObstaclesEnabledChanged;
        MultiplayerManager.instance.GetGameSettings().NetworkMapWrappingEnabled().OnValueChanged += GameSettings_OnMapWrappingEnabledChanged;
        MultiplayerManager.instance.GetGameSettings().NetworkStarvingSneksEnabled().OnValueChanged += GameSettings_OnStarvingSneksEnabledChanged;

        PopulateColorSelect();

        UpdatePlayer();

        Lobby lobby = MultiplayerLobby.instance.GetLobby();

        gameNameText.text = lobby.Name;
        joinCodeText.text = $"Join Code: {lobby.LobbyCode}";

        LayoutRebuilder.ForceRebuildLayoutImmediate(gameNameLayout);
        LayoutRebuilder.ForceRebuildLayoutImmediate(joinCodeLayout);

        InvokeRepeating("UpdateCopyMessage", 0, 0.5f);

        InitGameSettings();

        //Ensure that the cursor is visible
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void GameSettings_OnGameModeChanged(GameModeDefinition previousValue, GameModeDefinition newValue)
    {
        if (IsServer) return;

        gameModeSelectIndicatorText.text = newValue.GetName();
        gameModeDescText.text = newValue.GetDestription();
    }

    private void GameSettings_OnMaxApplesChanged(int previousValue, int newValue)
    {
        if(IsServer) return;

        maxApplesSelectIndicatorText.text = newValue.ToString();
    }

    private void GameSettings_OnPowerupsEnabledChanged(bool previousValue, bool newValue)
    {
        if (IsServer) return;

        enablePowerupsIndicator.Toggle(newValue);
        maxPowerupsSelect.transform.parent.gameObject.SetActive(newValue);
    }

    private void GameSettings_OnMaxPowerupsChanged(int previousValue, int newValue)
    {
        if (IsServer) return;

        maxPowerupsSelectIndicatorText.text = newValue.ToString();
    }

    private void GameSettings_OnObstaclesEnabledChanged(bool previousValue, bool newValue)
    {
        if (IsServer) return;

        enableObstaclesIndicator.Toggle(newValue);
    }

    private void GameSettings_OnMapWrappingEnabledChanged(bool previousValue, bool newValue)
    {
        if (IsServer) return;

        enableMapWrappingIndicator.Toggle(newValue);
    }

    private void GameSettings_OnStarvingSneksEnabledChanged(bool previousValue, bool newValue)
    {
        if (IsServer) return;

        enableStarvingSneksIndicator.Toggle(newValue);
    }

    private void InitGameSettings()
    {
        List<string> gameModeOptions = new List<string>();
        foreach (GameModeDefinition gameMode in GameModes.GAME_MODES)
        {
            gameModeOptions.Add(gameMode.GetName());
        }
        gameModeSelect.SetOptions(gameModeOptions);

        List<string> maxApplesOptions = new List<string>();
        for (int i = 0; i < maxApplesPossible; i++)
        {
            maxApplesOptions.Add((i + 1).ToString());
        }
        maxApplesSelect.SetDefaultOption(defaultMaxApples - 1);
        maxApplesSelect.SetOptions(maxApplesOptions);

        List<string> maxPowerupsOptions = new List<string>();
        for (int i = 0; i < maxPowerupsPossible; i++)
        {
            maxPowerupsOptions.Add((i + 1).ToString());
        }
        maxPowerupsSelect.SetDefaultOption(defaultMaxPowerups - 1);
        maxPowerupsSelect.SetOptions(maxPowerupsOptions);

        if (!MultiplayerManager.instance.IsNewLobby())
        {
            maxApplesSelect.SelectOption(MultiplayerManager.instance.GetGameSettings().GetMaxApples() - 1);
            maxPowerupsSelect.SelectOption(MultiplayerManager.instance.GetGameSettings().GetMaxPowerups() - 1);
            gameModeSelect.SelectOption(GameModes.GetGameModeIndex(MultiplayerManager.instance.GetGameSettings().GetGameMode()));
        }

        enablePowerupsToggle.GetComponent<Toggle>().isOn = MultiplayerManager.instance.GetGameSettings().PowerupsEnabled();
        enableMapWrappingToggle.GetComponent<Toggle>().isOn = MultiplayerManager.instance.GetGameSettings().MapWrappingEnabled();
        enableStarvingSneksToggle.GetComponent<Toggle>().isOn = MultiplayerManager.instance.GetGameSettings().StarvingSneksEnabled();
        enableObstaclesToggle.GetComponent<Toggle>().isOn = MultiplayerManager.instance.GetGameSettings().ObstaclesEnabled();

        maxPowerupsSelect.transform.parent.gameObject.SetActive(MultiplayerManager.instance.GetGameSettings().PowerupsEnabled());

        if(!IsServer)
        {
            gameModeSelect.gameObject.SetActive(false);
            maxApplesSelect.gameObject.SetActive(false);
            enablePowerupsToggle.gameObject.SetActive(false);
            maxPowerupsSelect.gameObject.SetActive(false);
            enableObstaclesToggle.gameObject.SetActive(false);
            enableMapWrappingToggle.gameObject.SetActive(false);
            enableStarvingSneksToggle.gameObject.SetActive(false);

            gameModeSelectIndicatorText.gameObject.SetActive(true);
            maxApplesSelectIndicatorText.gameObject.SetActive(true);
            enablePowerupsIndicator.gameObject.SetActive(true);
            maxPowerupsSelectIndicatorText.gameObject.SetActive(true);
            enableObstaclesIndicator.gameObject.SetActive(true);
            enableMapWrappingIndicator.gameObject.SetActive(true);
            enableStarvingSneksIndicator.gameObject.SetActive(true);

            GameSettings_OnGameModeChanged(MultiplayerManager.instance.GetGameSettings().GetGameMode(),
                MultiplayerManager.instance.GetGameSettings().GetGameMode());
            GameSettings_OnMaxApplesChanged(MultiplayerManager.instance.GetGameSettings().GetMaxApples(),
                MultiplayerManager.instance.GetGameSettings().GetMaxApples());
            GameSettings_OnPowerupsEnabledChanged(MultiplayerManager.instance.GetGameSettings().PowerupsEnabled(), 
                MultiplayerManager.instance.GetGameSettings().PowerupsEnabled());
            GameSettings_OnMaxPowerupsChanged(MultiplayerManager.instance.GetGameSettings().GetMaxPowerups(),
                MultiplayerManager.instance.GetGameSettings().GetMaxPowerups());
            GameSettings_OnObstaclesEnabledChanged(MultiplayerManager.instance.GetGameSettings().ObstaclesEnabled(),
                MultiplayerManager.instance.GetGameSettings().ObstaclesEnabled());
            GameSettings_OnMapWrappingEnabledChanged(MultiplayerManager.instance.GetGameSettings().MapWrappingEnabled(),
                MultiplayerManager.instance.GetGameSettings().MapWrappingEnabled());
            GameSettings_OnStarvingSneksEnabledChanged(MultiplayerManager.instance.GetGameSettings().StarvingSneksEnabled(),
                MultiplayerManager.instance.GetGameSettings().StarvingSneksEnabled());
        }
    }

    public void ChangeGameMode(int _gameModeID)
    {
        if (!IsServer) return;

        MultiplayerManager.instance.GetGameSettings().SetGameMode(GameModes.GetGameMode(_gameModeID));

        gameModeDescText.text = GameModes.GetGameMode(_gameModeID).GetDestription();
    }

    public void SetMaxApples(int _maxApplesIndex)
    {
        if (!IsServer) return;

        MultiplayerManager.instance.GetGameSettings().SetMaxApples(_maxApplesIndex + 1);
    }

    public void EnablePowerups(bool _powerupsEnabled)
    {
        if (!IsServer) return;

        MultiplayerManager.instance.GetGameSettings().SetPowerupsEnabled(_powerupsEnabled);

        maxPowerupsSelect.transform.parent.gameObject.SetActive(_powerupsEnabled);
    }

    public void SetMaxPowerups(int _maxPowerupsIndex)
    {
        if (!IsServer) return;

        MultiplayerManager.instance.GetGameSettings().SetMaxPowerups(_maxPowerupsIndex + 1);
    }

    public void EnableObstacles(bool _obstaclesEnabled) 
    {
        if (!IsServer) return;

        MultiplayerManager.instance.GetGameSettings().SetObstaclesEnabled(_obstaclesEnabled);
    }

    public void EnableMapWrapping(bool _mapWrappingEnabled)
    {
        if (!IsServer) return;

        MultiplayerManager.instance.GetGameSettings().SetMapWrappingEnabled(_mapWrappingEnabled);
    }

    public void EnableStarvingSneks(bool _starvingSneksEnabled)
    {
        if (!IsServer) return;

        MultiplayerManager.instance.GetGameSettings().SetStarvingSneksEnabled(_starvingSneksEnabled);
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong _clientID)
    {
        if(_clientID == NetworkManager.ServerClientId)
            ShowErrorMessage("You have been disconnected!", "You have been disconnected from the game, \nyou will now be returned to the main menu!");
    }

    private void ShowMessage(string _title, string _message, Sprite _icon)
    {
        characterSelectPanel.SetActive(false);
        gameSettingsPanel.SetActive(false);

        messageWindow.ShowMessage(_title, _message, _icon);
    }

    private void ShowErrorMessage(string _title, string _message)
    {
        characterSelectPanel.SetActive(false);
        gameSettingsPanel.SetActive(false);

        messageWindow.ShowErrorMessage(_title, _message, null, ReturnToLobby);
    }

    public void ReturnToLobby()
    {
        DisconenctClient();
    }

    private void UpdateCopyMessage()
    {
        if (MultiplayerLobby.instance.GetLobby().LobbyCode.Equals(GUIUtility.systemCopyBuffer))
            lobbyCodeCopiedMessage.SetActive(true);
        else
            lobbyCodeCopiedMessage.SetActive(false);
    }

    public void CopyJoinCode()
    {
        GUIUtility.systemCopyBuffer = MultiplayerLobby.instance.GetLobby().LobbyCode;
    }

    private void PopulateColorSelect()
    {
        for (int i = 0; i < MultiplayerManager.instance.GetPlayerColors().Length; i++)
        {
            ColorToggle colorToggle = Instantiate(colorSelectToggle, colorSelectionGroup.transform).GetComponent<ColorToggle>();
            colorToggle.SetColor(i);
        }
    }

    private void MultiplayerManager_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdatePlayer(); 
    }

    private void UpdatePlayer()
    {
        int localPlayerIndex = -1;
        for (int i = 0; i < playerInfoPanels.Length; i++)
        { 
            if(MultiplayerManager.instance.IsPlayerIndexConnected(i))
            {
                //Player Index Connected
                PlayerData playerData = MultiplayerManager.instance.GetPlayerDataFromIndex(i);

                if (playerData.GetClientID() == NetworkManager.LocalClientId)
                {
                    playerInfoPanels[i].SetOwnerPlayer(i);
                    localPlayerIndex = i;
                }
                else
                {
                    if (i > localPlayerIndex && localPlayerIndex != -1)
                        playerInfoPanels[i].SetServerPlayer(i, true);
                    else
                        playerInfoPanels[i].SetServerPlayer(i, false);

                    if (IsServer)
                        playerInfoPanels[i].EnableKick();
                }

                playerInfoPanels[i].UpdateReadyState(IsPlayerReady(playerData.GetClientID()));

                playerInfoPanels[i].UpdatePlayerColor(MultiplayerManager.instance.GetColorFromIndex(playerData.GetPlayerColorIndex()));

                playerInfoPanels[i].UpdatePlayerDisplayName(playerData.GetPlayerDisplayName().ToString());
            }
            else
            {
                //Player Index Not Connected
                playerInfoPanels[i].Reset();
            }
        }
    }

    public bool IsPlayerReady(ulong _clientID)
    {
        return playerReadyDictionary.ContainsKey(_clientID) && playerReadyDictionary[_clientID];
    }

    public void SetPlayerReady(bool _isReady)
    {
        SetPlayerReadyServerRpc(_isReady);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(bool _isReady, ServerRpcParams serverRpcParams = default)
    {
        SetPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId, _isReady);
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = _isReady;

        bool allClientsReady = true;

        foreach(ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if(!playerReadyDictionary.ContainsKey(clientID) || !playerReadyDictionary[clientID]) 
            {
                //The player is not ready
                allClientsReady = false;
                break;
            }
        }

        if(allClientsReady)
        {
            if (MultiplayerManager.instance.GetGameSettings().GetGameMode().RequiresMultiplePlayers() && NetworkManager.Singleton.ConnectedClientsIds.Count <= 1) return;

            MultiplayerManager.instance.SetNewLobby(false);
            //MultiplayerLobby.instance.DeleteLobby();
            NetworkSceneManager.instance.LoadNetworkScene("Scene_GameScene", true);
        }
    }

    [ClientRpc]
    private void SetPlayerReadyClientRpc(ulong _clientID, bool _ready)
    {
        playerReadyDictionary[_clientID] = _ready;

        UpdatePlayer();
    }

    public void DisconenctClient()
    {
        MultiplayerManager.instance.DisconnectClient();
    }

    public void ShowGameSettings()
    {
        gameSettingsPanel.SetActive(true);
        characterSelectPanel.SetActive(false);
    }

    public void HideGameSettings()
    {
        gameSettingsPanel.SetActive(false);
        characterSelectPanel.SetActive(true);
    }

    public override void OnDestroy()
    {
        MultiplayerManager.instance.OnPlayerDataNetworkListChanged -= MultiplayerManager_OnPlayerDataNetworkListChanged;
        NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
        MultiplayerManager.instance.GetGameSettings().GetNetworkGameMode().OnValueChanged -= GameSettings_OnGameModeChanged;
        MultiplayerManager.instance.GetGameSettings().GetNetworkMaxApples().OnValueChanged -= GameSettings_OnMaxApplesChanged;
        MultiplayerManager.instance.GetGameSettings().NetworkPowerupsEnabled().OnValueChanged -= GameSettings_OnPowerupsEnabledChanged;
        MultiplayerManager.instance.GetGameSettings().GetNetworkMaxPowerups().OnValueChanged -= GameSettings_OnMaxPowerupsChanged;
        MultiplayerManager.instance.GetGameSettings().NetworkObstaclesEnabled().OnValueChanged -= GameSettings_OnObstaclesEnabledChanged;
        MultiplayerManager.instance.GetGameSettings().NetworkMapWrappingEnabled().OnValueChanged -= GameSettings_OnMapWrappingEnabledChanged;
        MultiplayerManager.instance.GetGameSettings().NetworkStarvingSneksEnabled().OnValueChanged -= GameSettings_OnStarvingSneksEnabledChanged;
    }
}
