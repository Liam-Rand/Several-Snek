using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class WorldEntity : NetworkBehaviour
{
    [SerializeField] private WorldGridPosition worldGridPosition;

    [SerializeField] protected NetworkObject networkObject;

    [SerializeField] protected GameObject spawnParticleEffect;
    [SerializeField] protected GameObject destroyParticleEffect;

    [SerializeField] protected AudioClip spawnSoundEffect;
    [SerializeField] protected AudioClip destroySoundEffect;


    public virtual void SetWorldGridPosition(WorldGridPosition _worldGridPosition) 
    { 
        worldGridPosition = _worldGridPosition;

        transform.position = new Vector2(GetWorldGridPosition().GetPosition().x, GetWorldGridPosition().GetPosition().y);
    }

    public virtual void SetWorldPosition(Vector2Int _position) 
    { 
        worldGridPosition.SetGridPosition(_position);

        transform.position = new Vector2(GetWorldGridPosition().GetPosition().x, GetWorldGridPosition().GetPosition().y);
    }
    public virtual void SetWorldDirection(Vector2Int _direction) { worldGridPosition.SetGridDirection(_direction); }

    public WorldGridPosition GetWorldGridPosition() { return worldGridPosition; }

    protected virtual void Start()
    {
        networkObject = GetComponent<NetworkObject>();

        GameManager.instance.GetWorldManager().AddWorldEntity(this);

        if (spawnParticleEffect != null)
            Instantiate(spawnParticleEffect, transform.position, Quaternion.identity);

        if(spawnSoundEffect != null)
            AudioManager.instance.GetSoundEffectsAudioSource().PlayOneShot(spawnSoundEffect);
    }

    [ServerRpc(RequireOwnership = true)]
    public virtual void UpdateWorldGridPositionServerRpc(WorldGridPosition _worldGridPosition, ServerRpcParams serverRpcParams = default)
    {
        UpdateWorldGridPositionClientRpc(_worldGridPosition);
    }

    [ClientRpc]
    public virtual void UpdateWorldGridPositionClientRpc(WorldGridPosition _worldGridPosition, ClientRpcParams clientRpcParams = default)
    {
        if (IsOwner) return;

        Vector2Int newPosition = _worldGridPosition.GetPosition();

        SetWorldPosition(newPosition);
        SetWorldDirection(_worldGridPosition.GetDirection());
    }

    public override void OnDestroy()
    {
        GameManager.instance.GetWorldManager().RemoveWorldEntity(this);
    }

    public virtual void DestroyEvent()
    {
        if (destroySoundEffect != null)
            AudioManager.instance.GetSoundEffectsAudioSource().PlayOneShot(destroySoundEffect);

        if (destroyParticleEffect != null)
            Instantiate(destroyParticleEffect, transform.position, Quaternion.identity);
    }

    [ClientRpc]
    public void DestroyEventClientRpc()
    {
        DestroyEvent();
    }

    public virtual void TriggerCollisionEvent(WorldEntity _sender)
    {
        Debug.Log($"{gameObject.name} was collided with {_sender.gameObject.name}!");
    }

    public NetworkObject GetNetworkObject() { return networkObject; }

    [ServerRpc (RequireOwnership = false)]
    public virtual void DestroyWorldEntityServerRpc(bool _destroyEvent)
    {
        if (_destroyEvent)
        {
            DestroyEvent();
            DestroyEventClientRpc();
        }

        networkObject.Despawn();
        Destroy(gameObject);
    }
}
