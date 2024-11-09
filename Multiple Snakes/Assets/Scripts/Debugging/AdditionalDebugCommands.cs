using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CobaPlatinum.DebugTools.Console;
using Unity.Netcode;
using System.Globalization;
using JetBrains.Annotations;

public class AdditionalDebugCommands : MonoBehaviour
{
    [PC_Command("Network.StartHost")]
    [PC_CommandQuickAction("Start Network Host")]
    public void DebugStartHost()
    {
        MultiplayerManager.instance.StartHost();
        Debug.Log("Starting network host!");
    }

    [PC_Command("Network.StartClient")]
    [PC_CommandQuickAction("Start Network Client")]
    public void DebugStartClient()
    {
        MultiplayerManager.instance.StartClient();
        Debug.Log("Starting network client!");
    }

    [PC_Command("Snake.SetLength")]
    public void SetSnakeLength(int _length)
    {
        Snake snake = GameManager.instance.GetLocalPlayerSnake();

        if (snake != null)
            snake.SetSnakeLengthServerRpc(_length);
    }

    [PC_Command("Snake.CreateSegment")]
    [PC_CommandQuickAction("Add Snake Segment")]
    public void CreateSnakeSegment()
    {
        Snake snake = GameManager.instance.GetLocalPlayerSnake();

        if(snake != null)
            snake.ChangeSnakeLength(1);
    }
}
