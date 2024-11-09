using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Game Mode", menuName = "Scriptable Objects/Game Modes/New Game Mode")]
public class GameModeDefinition : ScriptableObject, IEquatable<GameModeDefinition>, INetworkSerializable
{
    [SerializeField] private int id;
    [SerializeField] private new string name;
    [SerializeField] private string description;
    [SerializeField] private bool snakesRespawn;
    [SerializeField] private bool isTimed;
    [SerializeField] private bool requireMultiplePlayers;

    public bool Equals(GameModeDefinition _other)
    {
        return id == _other.GetID();
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref id);
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref description);
        serializer.SerializeValue(ref snakesRespawn);
        serializer.SerializeValue(ref isTimed);
        serializer.SerializeValue(ref requireMultiplePlayers);
    }

    public int GetID() { return id; }
    public string GetName() { return name; }
    public string GetDestription() { return description; }
    public bool DoSnakesRespawn() { return snakesRespawn; }
    public bool IsTimed() { return isTimed; }
    public bool RequiresMultiplePlayers() { return requireMultiplePlayers;}
}
