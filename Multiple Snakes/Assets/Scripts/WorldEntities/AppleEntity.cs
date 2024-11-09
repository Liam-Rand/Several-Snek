using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AppleEntity : ConsumableEntity
{
    [ServerRpc(RequireOwnership = false)]
    public override void DestroyWorldEntityServerRpc(bool _destroyEvent)
    {
        GameManager.instance.GetGameData().ChangeCurrentSpawnedApples(-1);

        base.DestroyWorldEntityServerRpc(_destroyEvent);

        if(GameManager.instance.GetGameState().Equals(GameState.GameOngoing))
            GameManager.instance.GetWorldManager().SpawnApple();
    }
}
