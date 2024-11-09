using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ConsumableEntity : WorldEntity
{
    public override void TriggerCollisionEvent(WorldEntity _sender)
    {
        base.TriggerCollisionEvent(_sender);

        Snake snake = _sender as Snake;

        if (snake != null) 
        {
            snake.ChangeSnakeLength(1);
        }

        DestroyWorldEntityServerRpc(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public override void DestroyWorldEntityServerRpc(bool _destroyEvent)
    {
        base.DestroyWorldEntityServerRpc(_destroyEvent);
    }
}
