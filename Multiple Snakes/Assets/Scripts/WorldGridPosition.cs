using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct WorldGridPosition : INetworkSerializable
{
    [SerializeField] private Vector2Int gridPosition;
    [SerializeField] private Vector2Int gridDirection;

    public WorldGridPosition(Vector2Int _gridPosition, Vector2Int _gridDirection)
    {
        gridPosition = _gridPosition;
        gridDirection = _gridDirection;
    }

    public Vector2Int GetPosition() { return gridPosition; }
    public void SetGridPosition(Vector2Int _gridPosition) { gridPosition = _gridPosition; }
    public Vector2Int GetDirection() { return gridDirection; }
    public void SetGridDirection(Vector2Int _gridDirection) { gridDirection = _gridDirection; }

    public Vector3 GetVector3Rotation()
    {
        if (gridDirection == WorldGridDirection.UP)
        {
            return new Vector3(0, 0, 0);
        }
        else if (gridDirection == WorldGridDirection.DOWN)
        {
            return new Vector3(0, 0, 180);
        }
        else if (gridDirection == WorldGridDirection.LEFT)
        {
            return new Vector3(0, 0, 90);
        }
        else
        {
            return new Vector3(0, 0, -90);
        }
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref gridPosition);
        serializer.SerializeValue(ref gridDirection);
    }
}
