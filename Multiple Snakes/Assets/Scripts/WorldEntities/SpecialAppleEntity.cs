using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpecialAppleEntity : ConsumableEntity
{
    [SerializeField] private int snakeLengthChangeAmount;

    public override void TriggerCollisionEvent(WorldEntity _sender)
    {
        Debug.Log($"{gameObject.name} was collided with {_sender.gameObject.name}!");

        Snake snake = _sender as Snake;

        if (snake != null)
        {
            if(snakeLengthChangeAmount < 0)
            {
                GameManager.instance.GetNetworkedAudio().PlaySnakeHurtSound();
                GameManager.instance.ShakeCameraServerRpc(GameManager.instance.GetGameData().GetCameraShakeIntensity(), GameManager.instance.GetGameData().GetCameraShakeTime());
            }

            snake.ChangeSnakeLength(snakeLengthChangeAmount);
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
