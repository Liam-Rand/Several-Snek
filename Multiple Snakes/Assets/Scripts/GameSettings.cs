using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameSettings : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<GameModeDefinition> gameMode = new NetworkVariable<GameModeDefinition>(GameModes.LAST_SNEK_SLITHERING);
    [SerializeField] private NetworkVariable<int> maxApples = new NetworkVariable<int>(0);
    [SerializeField] private NetworkVariable<bool> enablePowerups = new NetworkVariable<bool>(false);
    [SerializeField] private NetworkVariable<int> maxPowerups = new NetworkVariable<int>(0);
    [SerializeField] private NetworkVariable<bool> enableObstacles = new NetworkVariable<bool>(false);
    [SerializeField] private NetworkVariable<bool> enableMapWrapping = new NetworkVariable<bool>(true);
    [SerializeField] private NetworkVariable<bool> enableStarvingSneks = new NetworkVariable<bool>(false);

    public GameModeDefinition GetGameMode() { return gameMode.Value; }
    public NetworkVariable<GameModeDefinition> GetNetworkGameMode() { return gameMode; }
    public void SetGameMode(GameModeDefinition _gameMode) { gameMode.Value = _gameMode;}

    public int GetMaxApples() {  return maxApples.Value; }
    public NetworkVariable<int> GetNetworkMaxApples() { return maxApples; }
    public void SetMaxApples(int _maxApples) { maxApples.Value = _maxApples; }

    public bool PowerupsEnabled() { return enablePowerups.Value; }
    public NetworkVariable<bool> NetworkPowerupsEnabled() { return enablePowerups; }
    public void SetPowerupsEnabled(bool _enablePowerups) { enablePowerups.Value = _enablePowerups; }

    public int GetMaxPowerups() { return maxPowerups.Value; }
    public NetworkVariable<int> GetNetworkMaxPowerups() { return maxPowerups; }
    public void SetMaxPowerups(int _maxPowerups) { maxPowerups.Value = _maxPowerups; }

    public bool ObstaclesEnabled() { return enableObstacles.Value; }
    public NetworkVariable<bool> NetworkObstaclesEnabled() { return enableObstacles; }
    public void SetObstaclesEnabled(bool _enableObstacles) { enableObstacles.Value = _enableObstacles; }

    public bool MapWrappingEnabled() { return enableMapWrapping.Value; }
    public NetworkVariable<bool> NetworkMapWrappingEnabled() { return enableMapWrapping; }
    public void SetMapWrappingEnabled(bool _enableMapWrapping) { enableMapWrapping.Value = _enableMapWrapping; }

    public bool StarvingSneksEnabled() { return enableStarvingSneks.Value; }
    public NetworkVariable<bool> NetworkStarvingSneksEnabled() { return enableStarvingSneks; }
    public void SetStarvingSneksEnabled(bool _enableStarvingSneks) { enableStarvingSneks.Value = _enableStarvingSneks; }
}
