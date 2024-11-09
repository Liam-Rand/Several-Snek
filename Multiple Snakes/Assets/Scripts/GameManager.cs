using CobaPlatinum.DebugTools.ExposedFields;
using System;
using System.Collections.Generic;
using System.Net;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    #region Singleton
    public static GameManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of GameManager found!");
            return;
        }

        instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>();
        snakesDictionary = new Dictionary<ulong, Snake>();
        playerPowerupDictionary = new Dictionary<ulong, PowerupDefinition>();
    }
    #endregion

    [Header("Referenced Managers")]
    [SerializeField] private GameData gameData;
    [SerializeField] private WorldManager worldManager;
    [SerializeField] private NetworkedAudio networkedAudio;
    [SerializeField] private CinemachineShake cinemachineShake;

    [Header("Player Ready Screen")]
    [SerializeField] private GameObject playerReadyPanel;
    [SerializeField] private GameObject readyButton;
    [SerializeField] private GameObject waitingMessage;
    [SerializeField] private TextMeshProUGUI gameModeTitleText;
    [SerializeField] private TextMeshProUGUI movementTileText;
    [SerializeField] private GameObject collectFoodTile;
    [SerializeField] private GameObject collectFoodWithStarvingTile;
    [SerializeField] private GameObject collectPowerupsTile;
    [SerializeField] private GameObject lastSnekStandingTile;
    [SerializeField] private GameObject biggestBaddestSnekTile;
    [SerializeField] private GameObject snekExterminationTile;
    [SerializeField] private GameObject kingOfSneksTile;

    [Header("Message Screen")]
    [SerializeField] private MessageWindow messageWindow;

    [Header("Game Over Screen")]
    [SerializeField] private GameOverScreen gameOverPanel;
    [SerializeField] private GameObject confettiParticles;

    public event EventHandler OnGameStateChanged;
    public event EventHandler OnLocalPlayerReadyChanged;
    public event EventHandler OnPlayerAliveChanged;

    private NetworkVariable<GameState> gameState = new NetworkVariable<GameState>(GameState.WaitingToStart);
    [ExposedField] private bool isLocalPlayerReady = false;
    [ExposedField] private GameState debugGameState = GameState.WaitingToStart;

    [Header("Game Start Countdown")]
    [SerializeField] private NetworkVariable<float> gameCountdownTimer = new NetworkVariable<float>(3f);
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private TextMeshProUGUI countdownText;
    private float lastCountdownCeil = 0;

    [Header("Player Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Player Info HUD")]
    [SerializeField] private GameObject playerInfoParentObject;
    [SerializeField] private PlayerInfoHUD[] playerInfoHUDs;

    [Header("Music")]
    [SerializeField] private AudioClip inGameMusic;
    [SerializeField] private AudioClip gameOverLoseMusic;
    [SerializeField] private AudioClip gameOverWinMusic;

    [Header("Game Timer")]
    [SerializeField] private GameObject gameTimerObject;
    [SerializeField] private TextMeshProUGUI gameTimerText;
    [SerializeField] private int defaultGameTime;


    //Dictionaries
    private Dictionary<ulong, Snake> snakesDictionary;
    private Dictionary<ulong, bool> playerReadyDictionary;
    private Dictionary<ulong, PowerupDefinition> playerPowerupDictionary;

    [Header("Powerups")]
    [SerializeField] private float powerupEffectTimer;
    [SerializeField] private GameObject powerupDescriptionUI;
    [SerializeField] private TextMeshProUGUI powerupDescriptionText;

    [Header("Darts")]
    [SerializeField] private float currentDarts;
    [SerializeField] private GameObject dartHUD;
    [SerializeField] private TextMeshProUGUI dartCountText;

    [Header("Escape Menu")]
    [SerializeField] private GameObject escapeMenu;
    [SerializeField] private GameObject endGameButton;

    [Space]
    [SerializeField] private NetworkVariable<ulong> winningPlayerID = new NetworkVariable<ulong>(10);
    [SerializeField] private NetworkVariable<float> gameTimer = new NetworkVariable<float>(600f);
    [SerializeField] private NetworkVariable<int> currentKillIndex = new NetworkVariable<int>(0);

    private void Start()
    {
        if (IsServer)
        {
            MultiplayerManager.instance.ResetPlayerData();
            gameTimer.Value = defaultGameTime;
        }

        gameTimerObject.SetActive(false);

        UpdatePlayerInfoHUDs();

        InitGameInfoPanel();

        MultiplayerManager.instance.OnPlayerDataNetworkListChanged += MultiplayerManager_OnPlayerDataNetworkListChanged;
        winningPlayerID.OnValueChanged += GameManager_OnWinningPlayerIDChanged;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
    }

    private void InitGameInfoPanel()
    {
        playerReadyPanel.SetActive(true);
        gameModeTitleText.text = MultiplayerManager.instance.GetGameSettings().GetGameMode().GetName();

        movementTileText.text = movementTileText.text.Replace("[moveup]", SettingsManager.instance.GetPlayerInputActions().FindAction("Move Up").GetBindingDisplayString())
            .Replace("[moveleft]", SettingsManager.instance.GetPlayerInputActions().FindAction("Move Left").GetBindingDisplayString())
            .Replace("[movedown]", SettingsManager.instance.GetPlayerInputActions().FindAction("Move Down").GetBindingDisplayString())
            .Replace("[moveright]", SettingsManager.instance.GetPlayerInputActions().FindAction("Move Right").GetBindingDisplayString());

        collectPowerupsTile.SetActive(MultiplayerManager.instance.GetGameSettings().PowerupsEnabled());

        collectFoodTile.SetActive(!MultiplayerManager.instance.GetGameSettings().StarvingSneksEnabled());
        collectFoodWithStarvingTile.SetActive(!collectFoodTile.activeSelf);

        if(MultiplayerManager.instance.GetGameSettings().GetGameMode().Equals(GameModes.LAST_SNEK_SLITHERING))
            lastSnekStandingTile.SetActive(true);
        else if (MultiplayerManager.instance.GetGameSettings().GetGameMode().Equals(GameModes.BIGGEST_SNEK))
            biggestBaddestSnekTile.SetActive(true);
        else if (MultiplayerManager.instance.GetGameSettings().GetGameMode().Equals(GameModes.SNEK_EXTERMINATION))
            snekExterminationTile.SetActive(true);
        else if (MultiplayerManager.instance.GetGameSettings().GetGameMode().Equals(GameModes.KING_OF_THE_SNEKS))
            kingOfSneksTile.SetActive(true);
    }

    private void GameManager_OnWinningPlayerIDChanged(ulong previousValue, ulong newValue)
    {
        gameOverPanel.SetWinningSnake(winningPlayerID.Value);

        if (winningPlayerID.Value == NetworkManager.Singleton.LocalClientId)
            AudioManager.instance.SetMusic(gameOverWinMusic);
        else
            AudioManager.instance.SetMusic(gameOverLoseMusic);
    }

    private void MultiplayerManager_OnPlayerDataNetworkListChanged(object sender, EventArgs e)
    {
        UpdatePlayerInfoHUDs();
        CheckForWinner();
    }

    private void InitPowerups()
    {
        foreach (ulong clientID in playerPowerupDictionary.Keys)
        {
            snakesDictionary[clientID].InitPowerup(playerPowerupDictionary[clientID]);
        }

        UpdatePlayerInfoHUDs();
    }

    [ServerRpc(RequireOwnership = false)]
    public void KillSnakeServerRpc(ulong _clientID)
    {
        KillSnakeClientRpc(_clientID);
    }

    [ClientRpc]
    private void KillSnakeClientRpc(ulong _clientID)
    {
        if(NetworkManager.Singleton.LocalClientId == _clientID)
        {
            KillLocalSnake();
        }
    }

    public void KillLocalSnake()
    {
        if (!MultiplayerManager.instance.GetGameSettings().GetGameMode().DoSnakesRespawn())
        {
            MultiplayerManager.instance.SetPlayerAlive(NetworkManager.Singleton.LocalClientId, false);

            RemoveSnake(NetworkManager.Singleton.LocalClientId, true);

            if (MultiplayerManager.instance.GetGameSettings().GetGameMode().Equals(GameModes.LAST_SNEK_SLITHERING))
            {
                MultiplayerManager.instance.SetPlayerKillIndex(NetworkManager.Singleton.LocalClientId, currentKillIndex.Value);
                IncrementCurrentKillIndexServerRpc();
            }
        }
        else
        {
            RespawnLocalSnake();
        }

        if (MultiplayerManager.instance.GetGameSettings().GetGameMode().Equals(GameModes.KING_OF_THE_SNEKS))
            worldManager.ResetOwnedTilesServerRpc(MultiplayerManager.instance.GetLocalPlayerData());

        networkedAudio.PlaySnakeHurtSound();
        ShakeCameraServerRpc(gameData.GetCameraShakeIntensity(), gameData.GetCameraShakeTime());
    }

    [ServerRpc(RequireOwnership = false)]
    public void IncrementCurrentKillIndexServerRpc()
    {
        currentKillIndex.Value++;
    }

    public void RespawnLocalSnake()
    {
        WorldGridPosition spawnPos = new WorldGridPosition(new Vector2Int((int)GetSpawnPoints()[MultiplayerManager.instance.GetPlayerDataIndexFromClientId(NetworkManager.Singleton.LocalClientId)].position.x,
                (int)GetSpawnPoints()[MultiplayerManager.instance.GetPlayerDataIndexFromClientId(NetworkManager.Singleton.LocalClientId)].position.y), WorldGridDirection.UP);
        GetLocalPlayerSnake().SetWorldGridPosition(spawnPos);

        SetLocalPlayerPowerup(Powerups.RESPAWN_PROTECTION);
        GetLocalPlayerSnake().SetSnakeLengthServerRpc(0);
    }

    public void SetLocalPlayerPowerup(PowerupDefinition _powerupDefinition)
    {
        playerPowerupDictionary[NetworkManager.Singleton.LocalClientId] = _powerupDefinition;
        powerupEffectTimer = _powerupDefinition.GetEffectTime();

        if(_powerupDefinition.Equals(Powerups.DARTS))
            currentDarts = gameData.GetMaxDarts();

        InitPowerups();

        SetPlayerPowerupServerRpc(_powerupDefinition);
    }

    public void SetRemotePlayerPowerup(ulong _clientID, PowerupDefinition _powerupDefinition)
    {
        SetRemotePlayerPowerupServerRpc(_clientID, _powerupDefinition);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetRemotePlayerPowerupServerRpc(ulong _clientID, PowerupDefinition _powerupDefinition, ServerRpcParams serverRpcParams = default)
    {
        //Server
        if (NetworkManager.Singleton.LocalClientId == _clientID)
        {
            SetLocalPlayerPowerup(_powerupDefinition);
            return;
        }

        //Clients
        SetRemotePlayerPowerupClientRpc(_clientID, _powerupDefinition);
    }

    [ClientRpc]
    private void SetRemotePlayerPowerupClientRpc(ulong _clientID, PowerupDefinition _powerupDefinition, ClientRpcParams clientRpcParams = default)
    {
        if (NetworkManager.Singleton.LocalClientId == _clientID)
            SetLocalPlayerPowerup(_powerupDefinition);
    }

    public PowerupDefinition GetPlayerPowerup(ulong _clientID)
    {
        if (playerPowerupDictionary.ContainsKey(_clientID))
        {
            return playerPowerupDictionary[_clientID];
        }

        return null;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerPowerupServerRpc(PowerupDefinition _powerupDefinition, ServerRpcParams serverRpcParams = default)
    {
        //Server
        playerPowerupDictionary[serverRpcParams.Receive.SenderClientId] = _powerupDefinition;
        InitPowerups();

        //Clients
        SetPlayerPowerupClientRpc(serverRpcParams.Receive.SenderClientId, _powerupDefinition);
    }

    [ClientRpc]
    private void SetPlayerPowerupClientRpc(ulong _clientID, PowerupDefinition _powerupDefinition)
    {
        playerPowerupDictionary[_clientID] = _powerupDefinition;
        InitPowerups();
    }

    public void CheckForWinner()
    {
        if (!IsServer) return;

        if(MultiplayerManager.instance.GetGameSettings().GetGameMode().Equals(GameModes.TESTING))
        {
            if (GetNumSnakesAlive() == 0)
            {
                SetGameState(GameState.GameEnded);
            }
        }
        else if (MultiplayerManager.instance.GetGameSettings().GetGameMode().Equals(GameModes.LAST_SNEK_SLITHERING))
        {
            if (GetNumSnakesAlive() == 1)
            {
                SetGameState(GameState.GameEnded);
            }
        }
        else if(MultiplayerManager.instance.GetGameSettings().GetGameMode().Equals(GameModes.BIGGEST_SNEK) ||
            MultiplayerManager.instance.GetGameSettings().GetGameMode().Equals(GameModes.SNEK_EXTERMINATION) ||
            MultiplayerManager.instance.GetGameSettings().GetGameMode().Equals(GameModes.KING_OF_THE_SNEKS))
        {
            if(gameTimer.Value <= 0)
                SetGameState(GameState.GameEnded);
        }
    }

    public void ReturnToCharacterSelect()
    {
        SetGameState(GameState.WaitingToStart);
        CleanupGameServerRpc();

        if (!IsServer) return;

        NetworkSceneManager.instance.LoadNetworkScene("Scene_CharacterSelect", false);
    }

    public void EndGame()
    {
        if (!IsServer) return;

        winningPlayerID.Value = SelectWinner();

        //Server
        CleanupGameServerRpc();

        ShowGameOverScreen();

        //Clients
        ShowGameOverScreenClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void CleanupGameServerRpc()
    {
        CleanupGame();
    }

    public ulong SelectWinner()
    {
        if (MultiplayerManager.instance.GetGameSettings().GetGameMode().Equals(GameModes.LAST_SNEK_SLITHERING))
        {
            int highestKillIndex = -10;
            PlayerData lastKilledPlayer = default;
            foreach (PlayerData player in MultiplayerManager.instance.GetAllPlayerData())
            {
                if (player.GetKillIndex() > highestKillIndex)
                {
                    highestKillIndex = player.GetKillIndex();
                    lastKilledPlayer = player;
                }

                if (player.IsAlive())
                {
                    return player.GetClientID();
                }
            }

            return lastKilledPlayer.GetClientID();
        }
        else if (MultiplayerManager.instance.GetGameSettings().GetGameMode().Equals(GameModes.TESTING))
        {
            return MultiplayerManager.instance.GetAllPlayerData()[0].GetClientID();
        }
        else if (MultiplayerManager.instance.GetGameSettings().GetGameMode().Equals(GameModes.BIGGEST_SNEK))
        {
            ulong largestSnakeClientID = 0;
            foreach (Snake snake in snakesDictionary.Values)
            {
                if (snake.GetSnakeLength() > snakesDictionary[largestSnakeClientID].GetSnakeLength())
                    largestSnakeClientID = snake.OwnerClientId;
            }

            return largestSnakeClientID;
        }
        else if (MultiplayerManager.instance.GetGameSettings().GetGameMode().Equals(GameModes.SNEK_EXTERMINATION))
        {
            ulong mostKillsClientID = 0;
            foreach (PlayerData playerData in MultiplayerManager.instance.GetAllPlayerData())
            {
                if (playerData.GetKillCount() > MultiplayerManager.instance.GetPlayerDataFromClientId(mostKillsClientID).GetKillCount())
                    mostKillsClientID = playerData.GetClientID();
            }

            return mostKillsClientID;
        }
        else if (MultiplayerManager.instance.GetGameSettings().GetGameMode().Equals(GameModes.KING_OF_THE_SNEKS))
        {
            ulong mostOwnedTilesClientID = 0;
            float highestOwnedTilesPercentage = 0f;

            foreach (PlayerData playerData in MultiplayerManager.instance.GetAllPlayerData())
            {
                float ownedTilesPercentage = worldManager.CalculateTotalOwnedTilesPercentage(playerData);
                Debug.Log(playerData.GetClientID() + ": " + ownedTilesPercentage);
                if (ownedTilesPercentage > highestOwnedTilesPercentage)
                {
                    highestOwnedTilesPercentage = ownedTilesPercentage;
                    mostOwnedTilesClientID = playerData.GetClientID();
                }
            }

            return mostOwnedTilesClientID;
        }

        return 1000;
    }

    public void CleanupGame()
    {
        worldManager.ClearWorldEntities(false);

        foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Debug.Log("Removing Snek: " + clientID);
            RemoveSnake(clientID, false);
        }

        if (MultiplayerManager.instance.GetGameSettings().GetGameMode().Equals(GameModes.KING_OF_THE_SNEKS))
            worldManager.ResetAllOwnedTilesServerRpc();
    }

    [ClientRpc]
    public void ShowGameOverScreenClientRpc()
    {
        ShowGameOverScreen();
    }

    public void ShowGameOverScreen()
    {
        playerInfoParentObject.SetActive(false);
        gameTimerObject.SetActive(false);
        gameOverPanel.Show();
        confettiParticles.SetActive(true);

        NetworkSceneManager.instance.EndLoadingScreen();
    }

    public int GetNumSnakesAlive()
    {
        int numSnakesAlive = 0;
        foreach (PlayerData player in MultiplayerManager.instance.GetAllPlayerData())
        {
            if(player.IsAlive())
                numSnakesAlive++;
        }

        return numSnakesAlive;
    }

    public void AddSnake(ulong _clientID, Snake _snake)
    {
        Debug.Log("Adding snake: " + _clientID);

        snakesDictionary.TryAdd(_clientID, _snake);
        playerPowerupDictionary.TryAdd(_clientID, Powerups.NONE);
    }

    public void RemoveSnake(ulong _clientID, bool _destroyEvent)
    {
        if (snakesDictionary.ContainsKey(_clientID))
        {
            snakesDictionary[_clientID].DestroyWorldEntityServerRpc(_destroyEvent);
            //snakesDictionary.Remove(_clientID);
        }
    }

    public Snake GetPlayerSnake(ulong _clientID)
    {
        return snakesDictionary[_clientID];
    }

    public Snake GetLocalPlayerSnake()
    {
        return GetPlayerSnake(NetworkManager.Singleton.LocalClientId);
    }

    private void Update()
    {
        debugGameState = gameState.Value;
        switch (gameState.Value) 
        {
            case GameState.WaitingToStart:

                break;
            case GameState.GameStarting:
                UpdateCountdownTimer();
                UpdateCountdownTimerDisplay();
                break;
            case GameState.GameOngoing:
                if (MultiplayerManager.instance.GetGameSettings().GetGameMode().IsTimed())
                {
                    UpdateGameTimer();
                    UpdateGameTimerDisplay();
                }
                break;
            case GameState.GameEnded:

                break;
        }

        if(Input.GetKeyDown(KeyCode.Escape) && gameState.Value == GameState.GameOngoing) 
        {
            ToggleEscapeMenu(!escapeMenu.activeSelf);
        }

        UpdatePowerupEffectTimer();
    }

    public void ToggleEscapeMenu(bool _toggle)
    {
        escapeMenu.SetActive(_toggle);

        if (_toggle)
        {
            endGameButton.SetActive(IsHost);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            if (gameState.Value == GameState.GameOngoing)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    public void SpawnDart(ulong _clientID, WorldGridPosition _worldGridPosition)
    {
        SpawnDartServerRpc(_clientID, _worldGridPosition);

        currentDarts--;

        UpdatePlayerInfoHUDs();

        if (currentDarts <= 0)
        {
            currentDarts = 0;
            powerupEffectTimer = 0;
            SetLocalPlayerPowerup(Powerups.NONE);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnDartServerRpc(ulong _clientID, WorldGridPosition _worldGridPosition)
    {
        DartEntity dart = Instantiate(GameManager.instance.GetGameData().GetDartPrefab()).GetComponent<DartEntity>();
        dart.SetWorldGridPosition(_worldGridPosition);

        dart.SetOwnerClientID(_clientID);
        dart.GetComponent<NetworkObject>().Spawn();
        dart.UpdateWorldGridPositionClientRpc(dart.GetWorldGridPosition());
    }

    private void UpdatePowerupEffectTimer()
    {
        if(powerupEffectTimer > 0)
        {
            powerupEffectTimer -= Time.deltaTime;
        }
        else
        {
            if (!playerPowerupDictionary.ContainsKey(NetworkManager.Singleton.LocalClientId) ||
                playerPowerupDictionary[NetworkManager.Singleton.LocalClientId].Equals(Powerups.DARTS) || 
                playerPowerupDictionary[NetworkManager.Singleton.LocalClientId].Equals(Powerups.NONE)) return;

            powerupEffectTimer = 0;
            SetLocalPlayerPowerup(Powerups.NONE);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShakeCameraServerRpc(float _intensity, float _time)
    {
        //Server
        cinemachineShake.ShakeCamera(_intensity, _time);

        //Clients
        ShakeCameraClientRpc(_intensity, _time);
    }

    [ClientRpc]
    public void ShakeCameraClientRpc(float _intensity, float _time)
    {
        cinemachineShake.ShakeCamera(_intensity, _time);
    }

    public void UpdatePlayerInfoHUDs()
    {
        for (int i = 0; i < playerInfoHUDs.Length; i++)
        {
            if (MultiplayerManager.instance.IsPlayerIndexConnected(i))
            {
                //Player Index Connected
                PlayerData playerData = MultiplayerManager.instance.GetPlayerDataFromIndex(i);

                playerInfoHUDs[i].SetPlayer(i);
            }
            else
            {
                //Player Index Not Connected
                playerInfoHUDs[i].Reset();
            }
        }

        PowerupDefinition currentPowerup = GameManager.instance.GetPlayerPowerup(NetworkManager.Singleton.LocalClientId);
        if (currentPowerup != null)
        {
            if (!currentPowerup.Equals(Powerups.NONE))
            {
                powerupDescriptionUI.SetActive(true);
                powerupDescriptionText.text = currentPowerup.GetDestription();

                if (currentPowerup.Equals(Powerups.DARTS))
                {
                    dartHUD.SetActive(true);

                    dartCountText.text = $"<mspace=.54em>{currentDarts}/{gameData.GetMaxDarts()}";
                }
                else
                    dartHUD.SetActive(false);
            }
            else
            {
                powerupDescriptionUI.SetActive(false);
                dartHUD.SetActive(false);
            }
        }
        else
        {
            powerupDescriptionUI.SetActive(false);
            dartHUD.SetActive(false);
        }
    }

    private void UpdateCountdownTimer()
    {
        if (!IsServer) return;

        gameCountdownTimer.Value -= Time.deltaTime;

        if(gameCountdownTimer.Value < 0f)
        {
            SetGameState(GameState.GameOngoing);
        }
    }

    private void UpdateCountdownTimerDisplay()
    {
        countdownText.text = Mathf.Ceil(gameCountdownTimer.Value).ToString();

        if(lastCountdownCeil != Mathf.Ceil(gameCountdownTimer.Value))
        {
            AudioManager.instance.GetSoundEffectsAudioSource().PlayOneShot(gameData.GetCountdownAudio());
            lastCountdownCeil = Mathf.Ceil(gameCountdownTimer.Value);
        }
    }

    private void UpdateGameTimer()
    {
        if (!IsServer) return;

        gameTimer.Value -= Time.deltaTime;

        if (gameTimer.Value < 0f)
        {
            gameTimer.Value = 0f;
            CheckForWinner();
        }
    }

    private void UpdateGameTimerDisplay()
    {
        int ceilGameTimer = (int)Mathf.Ceil(gameTimer.Value);

        int gameTimerMinutes = ceilGameTimer / 60;
        int gameTimerSeconds = ceilGameTimer % 60;

        gameTimerText.text = $"<mspace=0.54em>{gameTimerMinutes.ToString("00")}:{gameTimerSeconds.ToString("00")}";
    }

    public void SetLocalPlayerReady()
    {
        if(gameState.Value == GameState.WaitingToStart) 
        {
            isLocalPlayerReady = true;

            ShowWaitingMessage();

            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);

            SetPlayerReadyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if(!playerReadyDictionary.ContainsKey(clientID) || !playerReadyDictionary[clientID])
            {
                allClientsReady = false;
                break;
            }
        }

        if(allClientsReady)
        {
            SetGameState(GameState.GameStarting);
        }
    }

    private void ShowWaitingMessage()
    {
        if(isLocalPlayerReady)
        {
            readyButton.SetActive(false);
            waitingMessage.SetActive(true);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }

        gameState.OnValueChanged += GameState_OnValueChanged;
    }

    private void GameState_OnValueChanged(GameState previousValue, GameState newValue)
    {
        if (gameState.Value == GameState.GameStarting)
        {
            AudioManager.instance.StopMusic();
            messageWindow.Hide();
            playerReadyPanel.SetActive(false);
            countdownPanel.SetActive(true);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (gameState.Value == GameState.GameOngoing)
        {
            messageWindow.Hide();
            countdownPanel.SetActive(false);
            AudioManager.instance.SetMusic(inGameMusic);

            if (MultiplayerManager.instance.GetGameSettings().GetGameMode().IsTimed())
            {
                gameTimerObject.SetActive(true);
            }

            GetLocalPlayerSnake().SetCanMove(true);
        }
        else if(gameState.Value == GameState.GameEnded)
        {
            messageWindow.Hide();
            countdownPanel.SetActive(false);
            powerupDescriptionUI.SetActive(false);
            dartHUD.SetActive(false);

            AudioManager.instance.StopMusic();

            NetworkSceneManager.instance.ShowLoadingScreenClientRpc();
            ToggleEscapeMenu(false);
            EndGame();

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        OnGameStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerSnake = Instantiate(gameData.getSnakePrefab()).transform;

            playerSnake.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }

        if (MultiplayerManager.instance.GetGameSettings().ObstaclesEnabled())
            worldManager.SpawnWorldObstaclesServerRpc();

        NetworkSceneManager.instance.EndLoadingScreen();
        NetworkSceneManager.instance.EndLoadingScreenClientRpc();
    }

    public Transform[] GetSpawnPoints() { return spawnPoints; }

    public GameData GetGameData() { return  gameData; }
    public WorldManager GetWorldManager() {  return worldManager; }
    public NetworkedAudio GetNetworkedAudio() { return networkedAudio; }
    public CinemachineShake GetCinemachineShake() { return cinemachineShake; }
    public bool IsLocalPlayerReady() {  return isLocalPlayerReady; }
    public void SetGameState(GameState _gameState)
    {
        if (!IsServer) return;

        gameState.Value = _gameState;
    }
    public GameState GetGameState() { return gameState.Value; }
    private void NetworkManager_OnClientDisconnectCallback(ulong _clientID)
    {
        if (_clientID == NetworkManager.ServerClientId)
            ShowErrorMessage("You have been disconnected!", "You have been disconnected from the game, \nyou will now be returned to the main menu!");

        if(MultiplayerManager.instance.GetGameSettings().GetGameMode().RequiresMultiplePlayers() &&
            MultiplayerManager.instance.GetAllPlayerData().Count < 2)
        {
            ShowErrorMessage("Not Enough Players!", "There are no longer enough players for this game mode!\nYou will now be returned to the main menu!");
        }
    }

    private void ShowMessage(string _title, string _message, Sprite _icon)
    {
        playerInfoParentObject.SetActive(false);
        gameOverPanel.Hide();
        confettiParticles.SetActive(false);

        messageWindow.ShowMessage(_title, _message, _icon);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void ShowErrorMessage(string _title, string _message)
    {
        playerInfoParentObject.SetActive(false);
        gameTimerObject.SetActive(false);
        gameOverPanel.Hide();
        confettiParticles.SetActive(false);
        powerupDescriptionUI.SetActive(false);
        dartHUD.SetActive(false);



        worldManager.ClearWorldEntities(false);

        if(MultiplayerManager.instance.GetGameSettings().GetGameMode().Equals(GameModes.KING_OF_THE_SNEKS))
            worldManager.ResetAllOwnedTiles();

        messageWindow.ShowErrorMessage(_title, _message, null, ReturnToLobby);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ReturnToLobby()
    {
        DisconenctClient();
    }

    public void DisconenctClient()
    {
        MultiplayerManager.instance.DisconnectClient();
    }

    public override void OnDestroy()
    {
        if(IsServer)
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;

        MultiplayerManager.instance.OnPlayerDataNetworkListChanged -= MultiplayerManager_OnPlayerDataNetworkListChanged;
        NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;

        base.OnDestroy();
    }
}

public enum GameState
{
    WaitingToStart,
    GameStarting,
    GameOngoing,
    GameEnded
}
