using CobaPlatinum.DebugTools.ExposedFields;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData : MonoBehaviour
{
    //World Entities (Apple)
    [Header("Apple Entities")]
    [ExposedField][SerializeField] private int maxSpawnedApples = 0;
    [ExposedField] private int currentSpawnedApples = 0;
    [SerializeField] private ConsumableEntity appleEntity;

    //World Entities (Powerups)
    [Header("Powerup Entities")]
    [SerializeField] private WeightedConsumableEntity[] weightedConsumables;
    [ExposedField][SerializeField] private int maxSpawnedPowerups = 0;
    [ExposedField] private int currentSpawnedPowerups = 0;

    //Snake
    [Header("Snake Values")]
    [SerializeField] private float baseMoveTime;
    [SerializeField] private Color ironScalesColor;
    [SerializeField] private float initialStarveTime;
    [SerializeField] private float starveTime;
    [SerializeField] private float starveTimeAfterConsume;

    //Projectiles
    [Header("Projectiles")]
    [SerializeField] private GameObject dartPrefab;
    [SerializeField] private int maxDarts;

    //World Tiles
    [Header("World Tiles")]
    [SerializeField] private GameObject worldTileObjectPrefab;

    //Obstacles
    [Header("Obstacles")]
    private ObstacleLayouts obstacleLayouts;
    [SerializeField] private WorldEntity worldObstaclePrefab;

    //Audio
    [Header("Audio")]
    [SerializeField] private AudioClip countdownAudio;
    [SerializeField] private AudioClip snakeHurtAudio;

    //Camera Shake
    [Header("Camera Shake")]
    [SerializeField] private float cameraShakeIntensity;
    [SerializeField] private float cameraShakeTime;

    public GameObject GetWorldTileObjectPrefab() { return worldTileObjectPrefab; }
    public int GetMaxSpawnedApples() { return maxSpawnedApples; }
    public int GetCurrentSpawnedApples() { return currentSpawnedApples; }
    public void ChangeCurrentSpawnedApples(int _changeAmount) { currentSpawnedApples += _changeAmount; }
    public int GetMaxSpawnedPowerups() { return maxSpawnedPowerups; }
    public int GetCurrentSpawnedPowerups() { return currentSpawnedPowerups; }
    public void ChangeCurrentSpawnedPowerups(int _changeAmount) { currentSpawnedPowerups += _changeAmount; }
    public ConsumableEntity GetAppleEntity() { return appleEntity; }
    public ConsumableEntity GetRandomConsumableEntity()
    {
        int totalWeight = 0;
        foreach(WeightedConsumableEntity weightedConsumableEntity in weightedConsumables)
        {
            totalWeight += weightedConsumableEntity.GetWeight();
        }

        int randomWeight = Random.Range(1, totalWeight + 1);

        ConsumableEntity selectedConsumableEntity = null;

        int currentWeight = 0;
        foreach (WeightedConsumableEntity weightedConsumableEntity in weightedConsumables)
        {
            currentWeight += weightedConsumableEntity.GetWeight();

            if(randomWeight <= currentWeight) 
            {
                selectedConsumableEntity = weightedConsumableEntity.GetConsumableEntity();
                break;
            }
        }

        return selectedConsumableEntity;
    }
    public float GetBaseMoveTime() { return baseMoveTime; }
    public Color GetIronScalesColor() { return ironScalesColor; }
    public float GetInitialStarveTime() { return initialStarveTime; }
    public float GetStarveTime() { return starveTime; }
    public float GetStarveTimeAfterConsume() { return starveTimeAfterConsume; }
    public GameObject GetDartPrefab() { return dartPrefab; }
    public int GetMaxDarts() { return maxDarts; }
    public WorldEntity GetWorldObstaclePrefab() { return worldObstaclePrefab; }
    public AudioClip GetCountdownAudio() { return countdownAudio; }
    public AudioClip GetSnakeHurtAudio() { return snakeHurtAudio; }
    public float GetCameraShakeIntensity() { return cameraShakeIntensity; }
    public float GetCameraShakeTime() { return cameraShakeTime; }

    private void Awake()
    {
        obstacleLayouts = new ObstacleLayouts();
    }

    private void Start()
    {
        maxSpawnedApples = MultiplayerManager.instance.GetGameSettings().GetMaxApples();
        maxSpawnedPowerups = MultiplayerManager.instance.GetGameSettings().GetMaxPowerups();
    }

    //World Entities (Snakes)
    [Header("World Entities (Snakes)")]
    [SerializeField] private GameObject snakePrefab;
    [SerializeField] private GameObject snakeSegmentPrefab;
    public GameObject getSnakePrefab() { return snakePrefab; }
    public GameObject getSnakeSegmentPrefab() { return snakeSegmentPrefab; }
}

[System.Serializable]
public class WeightedConsumableEntity
{
    [SerializeField] private ConsumableEntity entity;
    [SerializeField] private int weight;

    public ConsumableEntity GetConsumableEntity() { return entity; }
    public int GetWeight() { return weight; }
}
