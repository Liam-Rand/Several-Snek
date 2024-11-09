using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameModes : MonoBehaviour
{
    [SerializeField] private GameModeDefinition setLastSnekStanding;
    [SerializeField] private GameModeDefinition setBiggestSnek;
    [SerializeField] private GameModeDefinition setSnekExtermination;
    [SerializeField] private GameModeDefinition setKingOfTheSneks;
    [SerializeField] private GameModeDefinition setTesting;

    public static GameModeDefinition LAST_SNEK_SLITHERING;
    public static GameModeDefinition BIGGEST_SNEK;
    public static GameModeDefinition SNEK_EXTERMINATION;
    public static GameModeDefinition KING_OF_THE_SNEKS;
    public static GameModeDefinition TESTING;

    public static List<GameModeDefinition> GAME_MODES = new List<GameModeDefinition>();

    public void Awake()
    {
        LAST_SNEK_SLITHERING = setLastSnekStanding;
        BIGGEST_SNEK = setBiggestSnek;
        SNEK_EXTERMINATION = setSnekExtermination;
        KING_OF_THE_SNEKS = setKingOfTheSneks;
        TESTING = setTesting;

        GAME_MODES.Clear();

        GAME_MODES.Add(LAST_SNEK_SLITHERING);
        GAME_MODES.Add(BIGGEST_SNEK);
        GAME_MODES.Add(SNEK_EXTERMINATION);
        GAME_MODES.Add(KING_OF_THE_SNEKS);
        //GAME_MODES.Add(TESTING);
    }

    public static GameModeDefinition GetGameMode(int _id)
    {
        foreach (GameModeDefinition gameMode in GAME_MODES)
        {
            if(gameMode.GetID() == _id)
                return gameMode;
        }

        return default;
    }

    public static int GetGameModeIndex(GameModeDefinition _gameModeDefinition)
    {
        for (int i = 0; i < GAME_MODES.Count; i++)
        {
            if (GAME_MODES[i].Equals(_gameModeDefinition))
                return i;
        }

        return -1;
    }
}
