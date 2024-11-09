using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTile
{
    [SerializeField] private ulong ownerClientID;
    [SerializeField] private Vector2Int position;
    [SerializeField] private WorldTileObject worldTileObject;

    public WorldTile()
    {
        ownerClientID = 1111;
    }

    public Vector2Int GetPosition() { return position; }
    public void SetPosition(Vector2Int _position) {  position = _position; }
    public WorldTileObject GetWorldTileObject() {  return worldTileObject; }
    public void SetWorldTileObject(WorldTileObject _worldTileObject) { worldTileObject = _worldTileObject; }
    public ulong GetOwnerClientID() { return ownerClientID; }
    public void SetOwnerClientID(ulong _ownerClientID) 
    {  
        ownerClientID = _ownerClientID;
        worldTileObject.SetColor(MultiplayerManager.instance.GetColorFromIndex(
            MultiplayerManager.instance.GetPlayerDataFromClientId(_ownerClientID).GetPlayerColorIndex()));
    }

    public void ResetTile()
    {
        ownerClientID = 1111;
        worldTileObject.ResetTile();
    }
}
