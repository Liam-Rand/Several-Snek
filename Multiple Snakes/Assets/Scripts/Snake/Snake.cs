using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using CobaPlatinum.DebugTools.ExposedFields;
using UnityEngine.UIElements;
using Unity.Netcode.Components;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class Snake : WorldEntity
{
    private SnakeData snakeData;
    [ExposedField] int debugSnakeLength;

    [SerializeField] private float timeUntilMove;
    [ExposedField][SerializeField] private float moveTime;

    [SerializeField] private GameObject gfx;

    [SerializeField] private SpriteRenderer[] spriteRenderers;

    [ExposedField] private bool canMove = false;

    private bool canChangeDirection = true;

    private NetworkAnimator animator;

    private Queue<Vector2Int> directionQueue = new Queue<Vector2Int>();

    private float starveTimer = 0f;

    private List<WorldEntity> checkedWorldEntities = new List<WorldEntity>();
    private Queue<WorldEntity> collisions = new Queue<WorldEntity>();

    private void Awake()
    {
        snakeData = new SnakeData(this);

        SetWorldDirection(WorldGridDirection.UP);

        timeUntilMove = moveTime;
    }

    protected override void Start()
    {
        base.Start();

        GetComponent<PlayerInput>().enabled = IsOwner;
        GetComponent<PlayerInput>().actions = SettingsManager.instance.GetPlayerInputActions();

        animator = GetComponent<NetworkAnimator>();

        GameManager.instance.AddSnake(OwnerClientId, this);

        InitPowerup(Powerups.NONE);

        gameObject.name += $" [Client: {MultiplayerManager.instance.GetPlayerDataIndexFromClientId(GetComponent<NetworkObject>().OwnerClientId)}]";

        //InvokeRepeating("UpdateSnake", 0, 0.5f);

        WorldGridPosition spawnPos = new WorldGridPosition(new Vector2Int((int)GameManager.instance.GetSpawnPoints()[MultiplayerManager.instance.GetPlayerDataIndexFromClientId(GetComponent<NetworkObject>().OwnerClientId)].position.x,
                (int)GameManager.instance.GetSpawnPoints()[MultiplayerManager.instance.GetPlayerDataIndexFromClientId(GetComponent<NetworkObject>().OwnerClientId)].position.y), WorldGridDirection.UP);
        SetWorldGridPosition(spawnPos);

        moveTime = GameManager.instance.GetGameData().GetBaseMoveTime();

        UpdateSnakeColors();

        starveTimer = GameManager.instance.GetGameData().GetInitialStarveTime();
    }

    public void OnSnakeDataChanged()
    {
        UpdateSnakeColors();
    }

    public void SetCanMove(bool _canMove) { canMove = _canMove; }

    public void InitPowerup(PowerupDefinition _powerupDefinition)
    {
        if (_powerupDefinition.Equals(Powerups.RESPAWN_PROTECTION))
        {
            SetCanDieToOtherSnakes(false);
            SetCanDieToSelf(false);
            SetCanDieToOtherObjects(false);
            SetCanKill(false);
            SetCanConsume(false);
            SetMoveTime(GameManager.instance.GetGameData().GetBaseMoveTime());
        }
        else if(_powerupDefinition.Equals(Powerups.SPEED_BOOST))
        {
            SetCanDieToOtherSnakes(true);
            SetCanDieToSelf(true);
            SetCanDieToOtherObjects(true);
            SetCanKill(true);
            SetCanConsume(true);
            SetMoveTime(GameManager.instance.GetGameData().GetBaseMoveTime() / 2);
        }
        else if (_powerupDefinition.Equals(Powerups.INVINCIBLE))
        {
            SetCanDieToOtherSnakes(false);
            SetCanDieToSelf(false);
            SetCanDieToOtherObjects(false);
            SetCanKill(false);
            SetCanConsume(true);
            SetMoveTime(GameManager.instance.GetGameData().GetBaseMoveTime());
        }
        else if (_powerupDefinition.Equals(Powerups.IRON_SNEK))
        {
            SetCanDieToOtherSnakes(false);
            SetCanDieToSelf(true);
            SetCanDieToOtherObjects(true);
            SetCanKill(true);
            SetCanConsume(true);
            SetMoveTime(GameManager.instance.GetGameData().GetBaseMoveTime());
        }
        else if (_powerupDefinition.Equals(Powerups.SLOW_SNEK))
        {
            SetCanDieToOtherSnakes(true);
            SetCanDieToSelf(true);
            SetCanDieToOtherObjects(true);
            SetCanKill(true);
            SetCanConsume(true);
            SetMoveTime(GameManager.instance.GetGameData().GetBaseMoveTime() * 2);
        }
        else if (_powerupDefinition.Equals(Powerups.DARTS))
        {
            SetCanDieToOtherSnakes(true);
            SetCanDieToSelf(true);
            SetCanDieToOtherObjects(true);
            SetCanKill(true);
            SetCanConsume(true);
            SetMoveTime(GameManager.instance.GetGameData().GetBaseMoveTime());
        }
        else
        {
            SetCanDieToOtherSnakes(true);
            SetCanDieToSelf(true);
            SetCanDieToOtherObjects(true);
            SetCanKill(true);
            SetCanConsume(true);
            SetMoveTime(GameManager.instance.GetGameData().GetBaseMoveTime());
        }
    }

    public void UpdateSnakeColors()
    {
        if (GameManager.instance.GetPlayerPowerup(GetComponent<NetworkObject>().OwnerClientId).Equals(Powerups.RESPAWN_PROTECTION) ||
            GameManager.instance.GetPlayerPowerup(GetComponent<NetworkObject>().OwnerClientId).Equals(Powerups.INVINCIBLE))
        {
            Color snakeColor = MultiplayerManager.instance.GetColorFromIndex(
            MultiplayerManager.instance.GetPlayerDataFromClientId(
                GetComponent<NetworkObject>().OwnerClientId).GetPlayerColorIndex());

            snakeData.SetSnakeColor(new Color(snakeColor.r, snakeColor.g, snakeColor.b, 0.4f));
        }
        else if (GameManager.instance.GetPlayerPowerup(GetComponent<NetworkObject>().OwnerClientId).Equals(Powerups.IRON_SNEK))
        {
            Color snakeColor = GameManager.instance.GetGameData().GetIronScalesColor();

            snakeData.SetSnakeColor(snakeColor);
        }
        else
        {
            Color snakeColor = MultiplayerManager.instance.GetColorFromIndex(
            MultiplayerManager.instance.GetPlayerDataFromClientId(
                GetComponent<NetworkObject>().OwnerClientId).GetPlayerColorIndex());

            snakeData.SetSnakeColor(snakeColor);
        }

        foreach (SpriteRenderer spriteRenderer in spriteRenderers) 
        {
            spriteRenderer.color = snakeData.GetSnakeColor();
        }
    }

    public void Update()
    {
        SetGFXRotation();

        if (!IsOwner) return;

        CheckForMovement();

        //Debugging
        debugSnakeLength = snakeData.GetSnakeLength();

        if(MultiplayerManager.instance.GetGameSettings().StarvingSneksEnabled())
        {
            if(snakeData.CanDieToSelf() && GameManager.instance.GetGameState().Equals(GameState.GameOngoing))
            {
                if (starveTimer <= 0)
                {
                    starveTimer = GameManager.instance.GetGameData().GetStarveTime();

                    if (snakeData.GetSnakeLength() > 0)
                    {
                        SetSnakeLengthServerRpc(snakeData.GetSnakeLength() - 1);
                        GameManager.instance.GetNetworkedAudio().PlaySnakeHurtSound();
                        GameManager.instance.ShakeCameraServerRpc(GameManager.instance.GetGameData().GetCameraShakeIntensity(), GameManager.instance.GetGameData().GetCameraShakeTime());
                    }
                    else
                        GameManager.instance.KillLocalSnake();
                }
                else
                    starveTimer -= Time.deltaTime;
            }
        }
    }

    public void FixedUpdate()
    {
        if(!IsOwner) return;

        CollisionUpdate();
    }

    public void QueueWorldEntityForCheck(WorldEntity _worldEntity)
    {
        if (!collisions.Contains(_worldEntity) && !checkedWorldEntities.Contains(_worldEntity))
            collisions.Enqueue(_worldEntity);
    }

    public void FlagWorldEntityAsChecked(WorldEntity _worldEntity)
    {
        if(!checkedWorldEntities.Contains(_worldEntity))
            checkedWorldEntities.Add(_worldEntity);
    }

    public void ClearCheckedWorldEntities()
    {
        checkedWorldEntities.Clear();
    }

    public void CollisionUpdate()
    {
        if (!IsOwner) return;

        List<WorldEntity> collisionEntities = CheckForCollision();

        foreach(WorldEntity worldEntity in collisionEntities)
        {
            QueueWorldEntityForCheck(worldEntity);
        }

        //CheckForCollision();
        if(collisions.Count > 0)
            HandleCollision(collisions.Dequeue());
    }

    private List<WorldEntity> CheckForCollision()
    {
        //Vector2Int newWorldPosition = GameManager.instance.GetWorldManager().GetWorldGrid()
        //.ValidateWorldPosition(GetWorldGridPosition().GetPosition() + GetWorldGridPosition().GetDirection());

        //List<WorldEntity> worldEntities = GameManager.instance.GetWorldManager().PreCheckForEntityCollision(newWorldPosition, this);
        List<WorldEntity> worldEntities = GameManager.instance.GetWorldManager().CheckForEntityCollision(this);

        return worldEntities;
    }

    public void HandleCollision(WorldEntity _worldEntity)
    {
        FlagWorldEntityAsChecked(_worldEntity);
        Debug.Log("Handling collision for: " +  _worldEntity);

        if (_worldEntity.GetType() == typeof(Snake) || _worldEntity.GetType() == typeof(SnakeSegmentEntity))
        {
            bool canOtherSnakeKill = false;
            ulong otherClientID = default;
            if (_worldEntity.GetType() == typeof(Snake))
            {
                otherClientID = ((Snake)_worldEntity).OwnerClientId;

                if (otherClientID == NetworkManager.Singleton.LocalClientId)
                    canOtherSnakeKill = snakeData.CanDieToSelf();
                else
                    canOtherSnakeKill = (GameManager.instance.GetPlayerSnake(otherClientID).snakeData.CanKill() && snakeData.CanDieToOtherSnakes());
            }
            else
            {
                otherClientID = ((SnakeSegmentEntity)_worldEntity).GetParentSnake().OwnerClientId;

                if (otherClientID == NetworkManager.Singleton.LocalClientId)
                    canOtherSnakeKill = snakeData.CanDieToSelf();
                else
                    canOtherSnakeKill = (GameManager.instance.GetPlayerSnake(otherClientID).snakeData.CanKill() && snakeData.CanDieToOtherSnakes());
            }

            if (canOtherSnakeKill)
            {
                GameManager.instance.KillLocalSnake();

                if (otherClientID != NetworkManager.Singleton.LocalClientId)
                    MultiplayerManager.instance.IncrementPlayerKillCount(otherClientID);
            }
            else
            {
                PowerupDefinition currentPowerup = GameManager.instance.GetPlayerPowerup(NetworkManager.Singleton.LocalClientId);
                if (currentPowerup != null)
                {
                    if (currentPowerup.Equals(Powerups.IRON_SNEK))
                    {
                        if (_worldEntity.GetType() == typeof(Snake))
                        {
                            ((Snake)_worldEntity).SetSnakeLengthServerRpc(0);
                        }
                        else
                        {
                            ((SnakeSegmentEntity)_worldEntity).GetParentSnake().SetSnakeLengthServerRpc(((SnakeSegmentEntity)_worldEntity).GetSnakeSegmentIndex());
                            GameManager.instance.GetNetworkedAudio().PlaySnakeHurtSound();
                            GameManager.instance.ShakeCameraServerRpc(GameManager.instance.GetGameData().GetCameraShakeIntensity(), GameManager.instance.GetGameData().GetCameraShakeTime());
                        }
                    }
                }
            }
        }
        else if (_worldEntity.GetType() == typeof(ConsumableEntity) || _worldEntity.GetType() == typeof(AppleEntity) ||
            _worldEntity.GetType() == typeof(SpecialAppleEntity) || _worldEntity.GetType() == typeof(PowerupEntity) ||
                _worldEntity.GetType() == typeof(EffectOthersPowerupEntity))
        {
            if (snakeData.CanComsume())
            {
                if (_worldEntity.GetType() == typeof(AppleEntity) && MultiplayerManager.instance.GetGameSettings().StarvingSneksEnabled())
                {
                    starveTimer = GameManager.instance.GetGameData().GetStarveTimeAfterConsume();
                }

                _worldEntity.TriggerCollisionEvent(this);

                animator.SetTrigger("consume");
            }
        }
        else if (_worldEntity.GetType() == typeof(WorldObstacleEntity))
        {
            if (snakeData.CanDieToOtherObjects())
                GameManager.instance.KillLocalSnake();
        }
    }

    public override void TriggerCollisionEvent(WorldEntity _sender)
    {
        base.TriggerCollisionEvent(_sender);
    }

    public void AddWorldDirectionToQueue(Vector2Int _direction)
    {
        directionQueue.Enqueue(_direction);

        if(directionQueue.Count > 4)
            directionQueue.Dequeue();
    }

    public void MoveUp(InputAction.CallbackContext _context)
    {
        if (!_context.performed) return;

        AddWorldDirectionToQueue(WorldGridDirection.UP);
    }
    public void MoveDown(InputAction.CallbackContext _context) 
    {
        if (!_context.performed) return;

        AddWorldDirectionToQueue(WorldGridDirection.DOWN);
    }
    public void MoveLeft(InputAction.CallbackContext _context) 
    {
        if (!_context.performed) return;

        AddWorldDirectionToQueue(WorldGridDirection.LEFT);
    }
    public void MoveRight(InputAction.CallbackContext _context)
    {
        if (!_context.performed) return;

        AddWorldDirectionToQueue(WorldGridDirection.RIGHT);
    }
    public void Fire(InputAction.CallbackContext _context)
    {
        if (!_context.performed) return;

        if (GameManager.instance.GetPlayerPowerup(OwnerClientId).Equals(Powerups.DARTS))
            ShootDart();
    }

    private void CheckForMovement()
    {
        if (!canMove) return;

        timeUntilMove -= Time.deltaTime;

        if (timeUntilMove <= 0)
        {
            MoveSnake();

            timeUntilMove = moveTime;
        }
    }

    private void MoveSnake()
    {
        //Owner Client Only
        CheckForNewDirection();

        snakeData.AddNewPosition(GetWorldGridPosition());

        WorldGridPosition previousWorldGrisPosition = GetWorldGridPosition();

        Vector2Int newWorldPosition = GameManager.instance.GetWorldManager().GetWorldGrid()
            .ValidateWorldPosition(GetWorldGridPosition().GetPosition() + GetWorldGridPosition().GetDirection());

        if (newWorldPosition != new Vector2Int(-1, -1))
            SetWorldPosition(newWorldPosition);
        else
            GameManager.instance.KillLocalSnake();

        CheckWorldTile(newWorldPosition);

        //CollisionUpdate();
        ClearCheckedWorldEntities();
        OnSnakeMoved();

        //All other clients
        UpdateSnakePositionServerRpc(previousWorldGrisPosition, GetWorldGridPosition());

        canChangeDirection = true;
    }

    private void CheckForNewDirection()
    {
        if (directionQueue.Count <= 0) return;
        
        Vector2Int newDirection = directionQueue.Dequeue();

        if (newDirection == WorldGridDirection.UP)
        {
            if (!(snakeData.GetSnakeLength() > 0 && GetWorldGridPosition().GetDirection() == WorldGridDirection.DOWN) && canChangeDirection)
            {
                SetWorldDirection(WorldGridDirection.UP);
                canChangeDirection = false;
            }
        }
        else if (newDirection == WorldGridDirection.LEFT)
        {
            if (!(snakeData.GetSnakeLength() > 0 && GetWorldGridPosition().GetDirection() == WorldGridDirection.RIGHT) && canChangeDirection)
            {
                SetWorldDirection(WorldGridDirection.LEFT);
                canChangeDirection = false;
            }
        }
        else if (newDirection == WorldGridDirection.DOWN)
        {
            if (!(snakeData.GetSnakeLength() > 0 && GetWorldGridPosition().GetDirection() == WorldGridDirection.UP) && canChangeDirection)
            {
                SetWorldDirection(WorldGridDirection.DOWN);
                canChangeDirection = false;
            }
        }
        else if (newDirection == WorldGridDirection.RIGHT)
        {
            if (!(snakeData.GetSnakeLength() > 0 && GetWorldGridPosition().GetDirection() == WorldGridDirection.LEFT) && canChangeDirection)
            {
                SetWorldDirection(WorldGridDirection.RIGHT);
                canChangeDirection = false;
            }
        }
    }

    private void ShootDart()
    {
        Vector2Int dartSpawnPosition = GameManager.instance.GetWorldManager().GetWorldGrid()
            .ValidateWorldPosition(GetWorldGridPosition().GetPosition() + GetWorldGridPosition().GetDirection());

        GameManager.instance.SpawnDart(OwnerClientId, new WorldGridPosition(dartSpawnPosition, GetWorldGridPosition().GetDirection()));
    }

    private void CheckWorldTile(Vector2Int _worldPosition)
    {
        if (MultiplayerManager.instance.GetGameSettings().GetGameMode().Equals(GameModes.KING_OF_THE_SNEKS) && snakeData.CanComsume() && GameManager.instance.GetGameState().Equals(GameState.GameOngoing))
        {
            WorldTile worldTile = GameManager.instance.GetWorldManager().GetWorldTile(_worldPosition);

            if(worldTile.GetOwnerClientID() != OwnerClientId)
                GameManager.instance.GetWorldManager().SetWorldTileOwnerServerRpc(_worldPosition, MultiplayerManager.instance.GetLocalPlayerData().GetClientID());
        }
    }

    [ServerRpc(RequireOwnership = true)]
    private void UpdateSnakePositionServerRpc(WorldGridPosition _newSegmentPositionGridPosition, WorldGridPosition _worldGridPosition, ServerRpcParams serverRpcParams = default)
    {
        UpdateSnakePositionClientRpc(_newSegmentPositionGridPosition, _worldGridPosition);
    }

    [ClientRpc]
    private void UpdateSnakePositionClientRpc(WorldGridPosition _newSegmentPositionGridPosition, WorldGridPosition _worldGridPosition, ClientRpcParams clientRpcParams = default)
    {
        if (IsOwner) return;
        
        snakeData.AddNewPosition(_newSegmentPositionGridPosition);

        Vector2Int newPosition = _worldGridPosition.GetPosition();

        SetWorldPosition(newPosition);
        SetWorldDirection(_worldGridPosition.GetDirection());

        SetGFXRotation();

        OnSnakeMoved();
    }

    private void OnSnakeMoved()
    {
        ValidateSnakeLength();
        DrawSnake();
    }

    private void SetGFXRotation()
    {
        gfx.transform.eulerAngles = GetWorldGridPosition().GetVector3Rotation();
    }

    public void ValidateSnakeLength()
    {
        if (snakeData.GetSnakeSegmentEntities().Count > snakeData.GetSnakeLength())
        {
            TrimSnakeEntitiesToLength();
        }
    }

    public void TrimSnakeEntitiesToLength()
    {
        int amountToTrim = snakeData.GetSnakeSegmentEntities().Count - snakeData.GetSnakeLength();

        if (amountToTrim > 0)
        {
            for (int i = 0; i < amountToTrim; i++)
            {
                int indexToRemove = snakeData.GetSnakeSegmentEntities().Count - 1;
                snakeData.GetSnakeSegmentEntities()[indexToRemove].DestroyEvent();
                Destroy(snakeData.GetSnakeSegmentEntities()[indexToRemove].gameObject);
                snakeData.GetSnakeSegmentEntities().RemoveAt(indexToRemove);
            }
        }
    }

    public void SetSnakeLength(int _length)
    {
        snakeData.SetSnakeLength(_length);
    }

    public void DrawSnake()
    {
        for (int i = 1; i < snakeData.GetPreviousGridPositions().Count; i++)
        {
            if(snakeData.GetSnakeSegmentEntities().Count >= i && snakeData.GetSnakeSegmentEntities()[i - 1] != null)
            {
                snakeData.GetSnakeSegmentEntities()[i - 1].SetWorldGridPosition(snakeData.GetPreviousGridPositions()[i - 1]);
                snakeData.GetSnakeSegmentEntities()[i - 1].SetSegmentColor(snakeData.GetSnakeColor());
            }
            else
            {
                SnakeSegmentEntity newSnakeSegmentEntity = Instantiate(GameManager.instance.GetGameData().getSnakeSegmentPrefab(),
                    new Vector2(GetWorldGridPosition().GetPosition().x, GetWorldGridPosition().GetPosition().y), Quaternion.identity).GetComponent<SnakeSegmentEntity>();

                newSnakeSegmentEntity.SetSnakeSegmentIndex(i - 1);
                newSnakeSegmentEntity.SetWorldGridPosition(snakeData.GetPreviousGridPositions()[i - 1]);
                newSnakeSegmentEntity.SetSegmentColor(snakeData.GetSnakeColor());

                newSnakeSegmentEntity.SetParentSnake(this);

                snakeData.AddSnakeSegmentEntity(newSnakeSegmentEntity);
            }

            snakeData.GetSnakeSegmentEntities()[i - 1].SetMidSegment();
        }

        if (snakeData.GetSnakeSegmentEntities().Count > 0)
        {
            if (snakeData.GetSnakeSegmentEntities().Count > 1)
            {
                for (int i = 0; i < snakeData.GetSnakeSegmentEntities().Count - 1; i++)
                {
                    if (snakeData.GetSnakeSegmentEntities()[i].GetWorldGridPosition().GetDirection() != snakeData.GetSnakeSegmentEntities()[i + 1].GetWorldGridPosition().GetDirection())
                        snakeData.GetSnakeSegmentEntities()[i].SetTurnSegment(snakeData.GetPreviousGridPositions()[i + 1].GetDirection());
                }
            }

            snakeData.GetSnakeSegmentEntities()[snakeData.GetSnakeSegmentEntities().Count - 1].SetEndSegment();
        }
    }

    public void ChangeSnakeLength(int _amount)
    {
        //Owner Client Only
        snakeData.ChangeSnakeLength(_amount);

        //All other clients
        ChangeSnakeLengthServerRpc(_amount);
    }

    public int GetSnakeLength()
    {
        return snakeData.GetSnakeLength();
    }

    public void SetCanDieToOtherSnakes(bool _canDieToOtherSnakes) { snakeData.SetCanDieToOtherSnakes(_canDieToOtherSnakes); OnSnakeDataChanged(); }
    public void SetCanDieToSelf(bool _canDieToSelf) { snakeData.SetCanDieToSelf(_canDieToSelf); OnSnakeDataChanged(); }
    public void SetCanDieToOtherObjects(bool _canDieToOtherObjects) { snakeData.SetCanDieToOtherObjects(_canDieToOtherObjects); OnSnakeDataChanged(); }
    public void SetMoveTime(float _moveSpeed) { moveTime = _moveSpeed; }
    public void SetCanKill(bool _canKill) { snakeData.SetCanKill(_canKill); OnSnakeDataChanged(); }
    public void SetCanConsume(bool _canConsume) { snakeData.SetCanComsume(_canConsume); OnSnakeDataChanged(); }

    public SnakeData GetSnakeData() { return snakeData; }

    [ServerRpc(RequireOwnership = false)]
    public override void DestroyWorldEntityServerRpc(bool _destroyEvent)
    {
        if (_destroyEvent)
        {
            DestroyEvent();
            DestroyEventClientRpc();
        }

        SetSnakeLengthServerRpc(0);
        ToggleSnakeServerRpc(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ToggleSnakeServerRpc(bool _toggle)
    {
        gameObject.SetActive(_toggle);

        ToggleSnakeClientRpc(_toggle);
    }

    [ClientRpc]
    private void ToggleSnakeClientRpc(bool _toggle)
    {
        gameObject.SetActive(_toggle);
    }

    public override void OnDestroy()
    {
        GameManager.instance.GetWorldManager().RemoveWorldEntity(this);
        CleanupSnake();
    }

    public void OnDisable()
    {
        GameManager.instance.GetWorldManager().RemoveWorldEntity(this);
        CleanupSnake();
    }

    public void CleanupSnake()
    {
        foreach (SnakeSegmentEntity snakeSegmentEntity in snakeData.GetSnakeSegmentEntities())
        {
            if (snakeSegmentEntity != null)
            {
                Destroy(snakeSegmentEntity.gameObject);
            }
        }

        if (IsOwner)
            SetSnakeLength(0);
    }

    [ServerRpc(RequireOwnership = true)]
    public void ChangeSnakeLengthServerRpc(int _amount, ServerRpcParams serverRpcParams = default)
    {
        IncreaseSnakeLengthClientRpc(_amount);
    }

    [ClientRpc]
    public void IncreaseSnakeLengthClientRpc(int _amount, ClientRpcParams clientRpcParams = default)
    {
        if (IsOwner) return;

        snakeData.ChangeSnakeLength(_amount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetSnakeLengthServerRpc(int _length, ServerRpcParams serverRpcParams = default)
    {
        SetSnakeLength(_length);

        SetSnakeLengthClientRpc(_length);
    }

    [ClientRpc]
    public void SetSnakeLengthClientRpc(int _length, ClientRpcParams clientRpcParams = default)
    {
        SetSnakeLength(_length);
    }
}
