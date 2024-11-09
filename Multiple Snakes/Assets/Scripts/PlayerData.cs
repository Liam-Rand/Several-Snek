using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    private ulong clientID;
    private int playerColorIndex;
    private FixedString64Bytes playerDisplayName;
    private FixedString64Bytes playerID;
    private bool isAlive;
    private int killCount;
    private int killIndex;

    public PlayerData(ulong _clientID)
    {
        clientID = _clientID;
        playerColorIndex = 0;
        playerDisplayName = "Missing Display Name";
        playerID = "Missing Player ID";

        isAlive = true;
        killCount = 0;
        killIndex = -1;
    }

    public bool Equals(PlayerData _other)
    {
        return clientID == _other.clientID;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientID);
        serializer.SerializeValue(ref playerColorIndex);
        serializer.SerializeValue(ref playerDisplayName);
        serializer.SerializeValue(ref playerID);
        serializer.SerializeValue(ref isAlive);
        serializer.SerializeValue(ref killCount);
        serializer.SerializeValue(ref killIndex);
    }

    public ulong GetClientID() { return clientID; }
    public int GetPlayerColorIndex() {  return playerColorIndex; }
    public void SetPlayerColorIndex(int _playerColorIndex) { playerColorIndex = _playerColorIndex; }
    public FixedString64Bytes GetPlayerDisplayName() { return playerDisplayName; }
    public void SetPlayerDisplayName(FixedString64Bytes _playerDisplayName) { playerDisplayName = _playerDisplayName; }
    public FixedString64Bytes GetPlayerID() { return playerID; }
    public void SetPlayerID(FixedString64Bytes _playerID) {  playerID = _playerID; }
    public void SetPlayerAlive(bool _isAlive) { isAlive = _isAlive; }
    public bool IsAlive() { return isAlive; }
    public int GetKillCount() { return killCount; }
    public void SetKillCount(int _killCount) { killCount = _killCount; }
    public int GetKillIndex() { return killIndex; }
    public void SetKillIndex(int _killIndex) { killIndex = _killIndex; }
}
