using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PowerupEntity : ConsumableEntity
{
    [SerializeField] protected PowerupDefinition powerup;

    public override void TriggerCollisionEvent(WorldEntity _sender)
    {
        Snake snake = _sender as Snake;

        if (snake != null)
        {
            if(snake.OwnerClientId == NetworkManager.Singleton.LocalClientId)
                GameManager.instance.SetLocalPlayerPowerup(powerup);
        }

        DestroyWorldEntityServerRpc(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public override void DestroyWorldEntityServerRpc(bool _destroyEvent)
    {
        GameManager.instance.GetGameData().ChangeCurrentSpawnedPowerups(-1);

        base.DestroyWorldEntityServerRpc(_destroyEvent);
    }
}
