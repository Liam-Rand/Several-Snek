using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EffectOthersPowerupEntity : PowerupEntity
{
    public override void TriggerCollisionEvent(WorldEntity _sender)
    {
        Snake senderSnake = _sender as Snake;

        if (senderSnake != null)
        {
            foreach (PlayerData playerData in MultiplayerManager.instance.GetAllPlayerData())
            {
                if(playerData.GetClientID() != NetworkManager.Singleton.LocalClientId)
                    GameManager.instance.SetRemotePlayerPowerup(playerData.GetClientID(), Powerups.SLOW_SNEK);
            }
        }

        DestroyWorldEntityServerRpc(true);
    }
}
