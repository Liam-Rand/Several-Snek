using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldGrid
{
    [SerializeField] private int width;
    [SerializeField] private int height;

    public WorldGrid(int _width, int _height)
    {
        width = _width;
        height = _height;
    }

    public Vector2Int GetRandomPosition()
    {
        return new Vector2Int(Random.Range(0, width), Random.Range(0, height));
    }

    public Vector2Int ValidateWorldPosition(Vector2Int _worldPosition)
    {
        if(_worldPosition.x < 0)
        {
            if(MultiplayerManager.instance.GetGameSettings().MapWrappingEnabled() || 
                !GameManager.instance.GetLocalPlayerSnake().GetSnakeData().CanDieToOtherObjects())
                _worldPosition.x = width - 1;
            else
                return new Vector2Int(-1, -1);
        }

        if (_worldPosition.x > width - 1)
        {
            if (MultiplayerManager.instance.GetGameSettings().MapWrappingEnabled() || 
                !GameManager.instance.GetLocalPlayerSnake().GetSnakeData().CanDieToOtherObjects())
                _worldPosition.x = 0;
            else
                return new Vector2Int(-1, -1);
        }

        if (_worldPosition.y < 0)
        {
            if (MultiplayerManager.instance.GetGameSettings().MapWrappingEnabled() || 
                !GameManager.instance.GetLocalPlayerSnake().GetSnakeData().CanDieToOtherObjects())
                _worldPosition.y = height - 1;
            else
                return new Vector2Int(-1, -1);
        }

        if (_worldPosition.y > height - 1)
        {
            if (MultiplayerManager.instance.GetGameSettings().MapWrappingEnabled() || 
                !GameManager.instance.GetLocalPlayerSnake().GetSnakeData().CanDieToOtherObjects())
                _worldPosition.y = 0;
            else
                return new Vector2Int(-1, -1);
        }

        return _worldPosition;
    }

    public int GetWidth() { return width; }
    public int GetHeight() { return height; }
}

public struct WorldGridDirection
{
    public static Vector2Int UP = new Vector2Int(0, 1);
    public static Vector2Int DOWN = new Vector2Int(0, -1);
    public static Vector2Int LEFT = new Vector2Int(-1, 0);
    public static Vector2Int RIGHT = new Vector2Int(1, 0);
}
