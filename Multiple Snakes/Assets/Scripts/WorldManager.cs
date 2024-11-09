using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using CobaPlatinum.DebugTools;

public class WorldManager : NetworkBehaviour
{
    [SerializeField] private WorldGrid worldGrid;
    [SerializeField] private int entitySpawnPadding;

    [SerializeField] private WorldTile[,] worldTiles;

    [SerializeField] private List<WorldEntity> spawnedEntities = new List<WorldEntity>();

    private void Start()
    {
        if (IsServer)
        {
            InvokeRepeating("SpawnApple", 0, 5);
            InvokeRepeating("SpawnPowerup", 0, 15);
        }

        if(MultiplayerManager.instance.GetGameSettings().GetGameMode().Equals(GameModes.KING_OF_THE_SNEKS))
            InitializeWorldTiles();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnWorldObstaclesServerRpc()
    {
        int obstacleLayoutIndex = ObstacleLayouts.GetRandomLayoutIndex();

        SpawnWorldObstacles(obstacleLayoutIndex);
    }

    public void SpawnWorldObstacles(int _obstacleLayoutIndex)
    {
        int[,] obstacleLayout = ObstacleLayouts.GetLayout(_obstacleLayoutIndex);

        for (int i = 0; i < obstacleLayout.GetUpperBound(0); i++)
        {
            for (int j = 0; j < obstacleLayout.GetUpperBound(1); j++)
            {
                if (obstacleLayout[i, j] == 1)
                {
                    WorldGridPosition spawnPos = new WorldGridPosition(new Vector2Int(j, i), WorldGridDirection.UP);
                    SpawnWorldObstacle(spawnPos);
                }
            }
        }
    }

    public void SpawnWorldObstacle(WorldGridPosition _worldGridPosition)
    {
        WorldObstacleEntity spawnedEntity = Instantiate(GameManager.instance.GetGameData().GetWorldObstaclePrefab().gameObject).GetComponent<WorldObstacleEntity>();
        spawnedEntity.SetWorldGridPosition(_worldGridPosition);

        spawnedEntity.GetComponent<NetworkObject>().Spawn();
        spawnedEntity.UpdateWorldGridPositionClientRpc(_worldGridPosition);

        CP_DebugWindow.Log(this, $"Spawned obstacle entity: \n\t[{spawnedEntity.gameObject.name}] at world location: [{_worldGridPosition}] ");
    }

    public void InitializeWorldTiles()
    {
        worldTiles = new WorldTile[worldGrid.GetWidth(), worldGrid.GetHeight()];

        for (int i = 0; i < worldGrid.GetHeight(); i++)
        {
            for (int j = 0; j < worldGrid.GetWidth(); j++)
            {
                worldTiles[j, i] = new WorldTile();
                worldTiles[j, i].SetPosition(new Vector2Int(j, i));

                WorldTileObject worldTileObject = Instantiate(GameManager.instance.GetGameData().GetWorldTileObjectPrefab()).GetComponent<WorldTileObject>();
                worldTileObject.gameObject.transform.position = new Vector3(j, i, 0);

                worldTiles[j, i].SetWorldTileObject(worldTileObject);
                worldTiles[j, i].GetWorldTileObject().ResetTile();
            }
        }
    }

    public WorldTile GetWorldTile(Vector2Int _worldGridPosition)
    {
        return worldTiles[_worldGridPosition.x, _worldGridPosition.y];
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetWorldTileOwnerServerRpc(Vector2Int _worldGridPosition, ulong _playerClientID)
    {
        //Server
        SetWorldTileOwner(_worldGridPosition, _playerClientID);

        //Clients
        SetWorldTileOwnerClientRpc(_worldGridPosition, _playerClientID);
    }

    [ClientRpc]
    public void SetWorldTileOwnerClientRpc(Vector2Int _worldGridPosition, ulong _playerClientID)
    {
        SetWorldTileOwner(_worldGridPosition, _playerClientID);
    }

    public void SetWorldTileOwner(Vector2Int _worldGridPosition, ulong _playerClientID)
    {
        worldTiles[_worldGridPosition.x, _worldGridPosition.y].SetOwnerClientID(_playerClientID);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetOwnedTilesServerRpc(PlayerData _playerData)
    {
        //Server
        ResetOwnedTiles(_playerData);

        //Clients
        ResetOwnedTilesClientRpc(_playerData);
    }

    [ClientRpc]
    public void ResetOwnedTilesClientRpc(PlayerData _playerData)
    {
        ResetOwnedTiles(_playerData);
    }

    public void ResetOwnedTiles(PlayerData _playerData)
    {
        foreach (WorldTile tile in worldTiles) 
        {
            if (tile.GetOwnerClientID().Equals(_playerData.GetClientID()))
                tile.ResetTile();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetAllOwnedTilesServerRpc()
    {
        //Server
        ResetAllOwnedTiles();

        //Clients
        ResetAllOwnedTilesClientRpc();
    }

    [ClientRpc]
    public void ResetAllOwnedTilesClientRpc()
    {
        ResetAllOwnedTiles();
    }

    public void ResetAllOwnedTiles()
    {
        foreach (WorldTile tile in worldTiles)
        {
            tile.ResetTile();
        }
    }

    public float CalculateTotalOwnedTilesPercentage(PlayerData _playerData)
    {
        float totalTiles = worldTiles.Length;
        float ownedTiles = 0;

        foreach(WorldTile worldTile in worldTiles)
        {
            if (worldTile.GetOwnerClientID() == _playerData.GetClientID())
                ownedTiles++;
        }

        return ownedTiles / totalTiles;
    }

    public void SpawnApple()
    {
        SpawnAppleServerRpc();
    }

    [ServerRpc]
    private void SpawnAppleServerRpc()
    {
        if (GameManager.instance.GetGameState() != GameState.GameOngoing) return;

        if (GameManager.instance.GetGameData().GetCurrentSpawnedApples() >= GameManager.instance.GetGameData().GetMaxSpawnedApples()) return;

        WorldGridPosition spawnPos = new WorldGridPosition(worldGrid.GetRandomPosition(), WorldGridDirection.UP);

        if(!IsWorldGridPositionWithinPadding(spawnPos, entitySpawnPadding) || IsWorldGridPositionOccupied(spawnPos))
        {
            SpawnAppleServerRpc();
            return;
        }

        WorldEntity spawnedEntity = Instantiate(GameManager.instance.GetGameData().GetAppleEntity().gameObject).GetComponent<WorldEntity>();
        spawnedEntity.SetWorldGridPosition(spawnPos);

        spawnedEntity.GetComponent<NetworkObject>().Spawn();
        spawnedEntity.UpdateWorldGridPositionClientRpc(spawnPos);

        GameManager.instance.GetGameData().ChangeCurrentSpawnedApples(1);

        CP_DebugWindow.Log(this, $"Spawned consumable entity: \n\t[{spawnedEntity.gameObject.name}] at world location: [{spawnPos}] ");
    }

    public void SpawnPowerup()
    {
        SpawnPowerupServerRpc();
    }

    [ServerRpc]
    private void SpawnPowerupServerRpc()
    {
        if (GameManager.instance.GetGameState() != GameState.GameOngoing || !MultiplayerManager.instance.GetGameSettings().PowerupsEnabled()) return;

        if (GameManager.instance.GetGameData().GetCurrentSpawnedPowerups() >= GameManager.instance.GetGameData().GetMaxSpawnedPowerups()) return;

        WorldGridPosition spawnPos = new WorldGridPosition(worldGrid.GetRandomPosition(), WorldGridDirection.UP);

        if (!IsWorldGridPositionWithinPadding(spawnPos, entitySpawnPadding) || IsWorldGridPositionOccupied(spawnPos))
        {
            SpawnPowerupServerRpc();
            return;
        }

        WorldEntity spawnedEntity = Instantiate(GameManager.instance.GetGameData().GetRandomConsumableEntity().gameObject).GetComponent<WorldEntity>();
        spawnedEntity.SetWorldGridPosition(spawnPos);

        spawnedEntity.GetComponent<NetworkObject>().Spawn();
        spawnedEntity.UpdateWorldGridPositionClientRpc(spawnPos);

        GameManager.instance.GetGameData().ChangeCurrentSpawnedPowerups(1);

        CP_DebugWindow.Log(this, $"Spawned powerup entity: \n\t[{spawnedEntity.gameObject.name}] at world location: [{spawnPos}] ");
    }

    public bool IsWorldGridPositionWithinPadding(WorldGridPosition _worldGridPosition, int _padding)
    {
        if (_worldGridPosition.GetPosition().x >= _padding &&
            _worldGridPosition.GetPosition().x < (worldGrid.GetWidth() - _padding) &&
            _worldGridPosition.GetPosition().y >= _padding &&
            _worldGridPosition.GetPosition().y < (worldGrid.GetHeight() - _padding))
        {
            return true;
        }
        else
            return false;
    }

    public void AddWorldEntity(WorldEntity _worldEntity)
    {
        spawnedEntities.Add(_worldEntity);
    }

    public void RemoveWorldEntity(WorldEntity _worldEntity)
    {
        spawnedEntities.Remove(_worldEntity);
    }

    public void ClearWorldEntities(bool _includeSnakes)
    {
        if (!IsServer) return;

        foreach (WorldEntity worldEntity in spawnedEntities)
        {
            if(!_includeSnakes)
                if (worldEntity.GetType() == typeof(Snake) || worldEntity.GetType() == typeof(SnakeSegmentEntity))
                    continue;

            worldEntity.DestroyWorldEntityServerRpc(false);
        }
    }

    public bool IsWorldGridPositionOccupied(WorldGridPosition _worldGridPosition)
    {
        foreach (WorldEntity entity in spawnedEntities) 
        {
            if (entity.GetWorldGridPosition().GetPosition() == _worldGridPosition.GetPosition())
                return true;
        }

        return false;
    }

    public List<WorldEntity> GetClientEntities(ulong _clientID)
    {
        List<WorldEntity> clientEntities = new List<WorldEntity>();

        foreach(WorldEntity worldEntity in spawnedEntities)
        {
            if(worldEntity.NetworkObject != null)
            {
                if (worldEntity.NetworkObject.OwnerClientId == _clientID)
                    clientEntities.Add(worldEntity);
            }
        }

        return clientEntities;
    }

    public WorldEntity GetEntityAtPosition(Vector2Int _worldPos)
    {
        foreach (WorldEntity worldEntity in spawnedEntities)
        {
            if(worldEntity.GetWorldGridPosition().GetPosition() == _worldPos)
                return worldEntity;
        }

        return null;
    }

    public List<WorldEntity> CheckForEntityCollision(WorldEntity _primaryWorldEntity)
    {
        List<WorldEntity> worldEntities = new List<WorldEntity>();
        foreach (WorldEntity worldEntity in spawnedEntities)
        {
            if (worldEntity.GetWorldGridPosition().GetPosition() == _primaryWorldEntity.GetWorldGridPosition().GetPosition() && worldEntity != _primaryWorldEntity)
                worldEntities.Add(worldEntity);
        }

        return worldEntities;
    }

    public List<WorldEntity> PreCheckForEntityCollision(Vector2Int _futurePosition, WorldEntity _primaryWorldEntity)
    {
        List<WorldEntity> worldEntities = new List<WorldEntity>();
        foreach (WorldEntity worldEntity in spawnedEntities)
        {
            if (worldEntity.GetWorldGridPosition().GetPosition() == _futurePosition && worldEntity != _primaryWorldEntity)
                worldEntities.Add(worldEntity);
        }

        return worldEntities;
    }

    public List<WorldEntity> GetSpawnedEntities() 
    { 
        return spawnedEntities; 
    }
    public WorldEntity GetWorldEntityAtPosition(Vector2Int _worldPosition)
    {
        foreach(WorldEntity worldEntity in spawnedEntities)
        {
            if(worldEntity.GetWorldGridPosition().GetPosition() == _worldPosition)
                return worldEntity;
        }

        return null;
    }
    public WorldGrid GetWorldGrid() { return worldGrid; }
}
