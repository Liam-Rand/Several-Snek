using CobaPlatinum.DebugTools.ExposedFields;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class DartEntity : WorldEntity
{
    [SerializeField] private float timeUntilMove;
    [SerializeField] private float moveTime;
    [SerializeField] private int range;
    private int distanceMoved = 0;

    [SerializeField] private GameObject gfx;

    [SerializeField] private NetworkVariable<ulong> ownerClientID = new NetworkVariable<ulong>(1000);

    protected override void Start()
    {
        base.Start();

        timeUntilMove = moveTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;

        SetGFXRotation();

        CheckForMovement();
    }

    public void SetOwnerClientID(ulong _ownerClientID)
    {
        ownerClientID.Value = _ownerClientID;
    }

    private void CheckForMovement()
    {
        timeUntilMove -= Time.deltaTime;

        if (timeUntilMove <= 0)
        {
            MoveDart();

            timeUntilMove = moveTime;
        }
    }

    private void MoveDart()
    {
        if (!IsServer) return;

        WorldGridPosition previousWorldGrisPosition = GetWorldGridPosition();

        Vector2Int newWorldPosition = GameManager.instance.GetWorldManager().GetWorldGrid()
            .ValidateWorldPosition(GetWorldGridPosition().GetPosition() + GetWorldGridPosition().GetDirection());

        if (newWorldPosition != new Vector2Int(-1, -1))
            SetWorldPosition(newWorldPosition);
        else
            DestroyWorldEntityServerRpc(true);

        OnDartMoved();
    }

    private void OnDartMoved()
    {
        CheckForCollision();

        distanceMoved++;

        if (distanceMoved >= range)
            DestroyWorldEntityServerRpc(true);
    }

    private void SetGFXRotation()
    {
        gfx.transform.eulerAngles = GetWorldGridPosition().GetVector3Rotation();
    }

    private void CheckForCollision()
    {
        if (!IsServer) return;

        List<WorldEntity> worldEntities = GameManager.instance.GetWorldManager().CheckForEntityCollision(this);

        foreach(WorldEntity worldEntity in worldEntities)
        {
            if (worldEntity.GetType() == typeof(Snake) || worldEntity.GetType() == typeof(SnakeSegmentEntity))
            {
                if (worldEntity.GetType() == typeof(Snake) || worldEntity.GetType() == typeof(SnakeSegmentEntity))
                {
                    if (worldEntity.GetType() == typeof(Snake))
                    {
                        if (((Snake)worldEntity).GetSnakeData().CanDieToOtherObjects())
                        {
                            ulong otherClientID = ((Snake)worldEntity).OwnerClientId;
                            GameManager.instance.KillSnakeServerRpc(otherClientID);

                            if(otherClientID != ownerClientID.Value)
                                MultiplayerManager.instance.IncrementPlayerKillCount(ownerClientID.Value);
                        }

                        //GameManager.instance.GetNetworkedAudio().PlaySnakeHurtSound();

                        DestroyWorldEntityServerRpc(true);
                    }
                    else
                    {
                        if (((SnakeSegmentEntity)worldEntity).GetParentSnake().GetSnakeData().CanDieToOtherObjects())
                        {
                            ((SnakeSegmentEntity)worldEntity).GetParentSnake().SetSnakeLengthServerRpc(((SnakeSegmentEntity)worldEntity).GetSnakeSegmentIndex());
                        }

                        DestroyWorldEntityServerRpc(true);

                        GameManager.instance.GetNetworkedAudio().PlaySnakeHurtSound();
                        GameManager.instance.ShakeCameraServerRpc(GameManager.instance.GetGameData().GetCameraShakeIntensity(), GameManager.instance.GetGameData().GetCameraShakeTime());
                    }
                }
            }
            else if(worldEntity.GetType() == typeof(WorldObstacleEntity))
            {
                DestroyWorldEntityServerRpc(true);
            }
        }
    }

    [ClientRpc]
    public override void UpdateWorldGridPositionClientRpc(WorldGridPosition _worldGridPosition, ClientRpcParams clientRpcParams = default)
    {
        base.UpdateWorldGridPositionClientRpc(_worldGridPosition, clientRpcParams);

        SetGFXRotation();
    }
}
