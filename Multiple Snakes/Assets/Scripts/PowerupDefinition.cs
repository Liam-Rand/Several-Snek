using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
[CreateAssetMenu(fileName = "New Powerup", menuName = "Scriptable Objects/Powerups/New Powerup")]
public class PowerupDefinition : ScriptableObject, IEquatable<PowerupDefinition>, INetworkSerializable
{
    [SerializeField] private int id;
    [SerializeField] new private string name;
    [SerializeField] private string description;
    [SerializeField] private int effectTime;

    [SerializeField] private Sprite icon;
    [SerializeField] private Color color;

    public bool Equals(PowerupDefinition _other)
    {
        return id == _other.GetID();
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref id);
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref description);
        serializer.SerializeValue(ref effectTime);
    }

    public int GetID() { return id; }
    public string GetName() { return name; }

    public string GetDestription() 
    {
        return description.Replace("[effectTime]", effectTime.ToString()).Replace("[Shoot]", SettingsManager.instance.GetPlayerInputActions().FindAction("Fire").GetBindingDisplayString());
    }

    public int GetEffectTime() { return effectTime; }
    public Sprite GetIcon() { return icon; }
    public Color GetColor() { return color; }
}
