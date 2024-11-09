using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SnakeData
{
    private const int PREVIOUS_POSITION_BUFFER = 1;

    private Snake snake;

    [SerializeField] private int snakeLength;
    [SerializeField] private List<WorldGridPosition> previousGridPositions;
    [SerializeField] private List<SnakeSegmentEntity> snakeSegmentEntities;

    [SerializeField] private Color snakeColor;

    [SerializeField] private bool canDieToOtherSnakes;
    [SerializeField] private bool canDieToSelf;
    [SerializeField] private bool canDieToOtherObjects;
    [SerializeField] private bool canKill;
    [SerializeField] private bool canConsume;

    public SnakeData(Snake _snake)
    {
        snake = _snake;

        snakeLength = 0;
        previousGridPositions = new List<WorldGridPosition>();
        snakeSegmentEntities = new List<SnakeSegmentEntity>();
        snakeColor = Color.white;

        canDieToOtherSnakes = true;
        canDieToSelf = true;
        canDieToOtherObjects = true;
        canKill = true;
        canConsume = true;
    }

    public void AddNewPosition(WorldGridPosition _newWorldGridPosition)
    {
        previousGridPositions.Insert(0, _newWorldGridPosition);

        ValidatePreviousPositions();
    }

    public void ValidatePreviousPositions()
    {
        if (previousGridPositions.Count > snakeLength + PREVIOUS_POSITION_BUFFER)
        {
            TrimPreviousPositions((previousGridPositions.Count) - (snakeLength + PREVIOUS_POSITION_BUFFER));
        }
    }

    public void TrimPreviousPositions(int _amountToTrim)
    {
        for (int i = 0; i < _amountToTrim; i++)
        {
            previousGridPositions.RemoveAt(previousGridPositions.Count - 1);
        }
    }

    public int GetSnakeLength() { return snakeLength; }
    public List<SnakeSegmentEntity> GetSnakeSegmentEntities() { return snakeSegmentEntities; }
    public List<WorldGridPosition> GetPreviousGridPositions() { return previousGridPositions; }
    public void ChangeSnakeLength(int _amount) { snakeLength += _amount; if (snakeLength < 0) snakeLength = 0; }
    public void SetSnakeLength(int _snakeLength) { snakeLength = _snakeLength; }
    public Color GetSnakeColor() { return snakeColor; }
    public void SetSnakeColor(Color _snakeColor) { snakeColor = _snakeColor; }

    public bool CanDieToOtherSnakes() { return canDieToOtherSnakes; }
    public void SetCanDieToOtherSnakes(bool _canDieToOtherSnakes) { canDieToOtherSnakes = _canDieToOtherSnakes; }
    public bool CanDieToSelf() { return canDieToSelf; }
    public void SetCanDieToSelf(bool _canDieToSelf) { canDieToSelf = _canDieToSelf; }
    public bool CanDieToOtherObjects() { return canDieToOtherObjects; }
    public void SetCanDieToOtherObjects(bool _canDieToOtherObjects) { canDieToOtherObjects = _canDieToOtherObjects; }

    public bool CanKill() { return canKill; }
    public void SetCanKill(bool _canKill) { canKill = _canKill; }
    public bool CanComsume() { return canConsume; }
    public void SetCanComsume(bool _canConsume) { canConsume = _canConsume; }

    public void AddSnakeSegmentEntity(SnakeSegmentEntity _newSegment)
    {
        snakeSegmentEntities.Add(_newSegment);
    }
}
