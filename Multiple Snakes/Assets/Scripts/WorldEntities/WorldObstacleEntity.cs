using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WorldObstacleEntity : WorldEntity
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Sprite isolated;
    [SerializeField] private Sprite N, E, S, W;
    [SerializeField] private Sprite NS, EW;
    [SerializeField] private Sprite ESW, NES, SWN, WNE;
    [SerializeField] private Sprite NESW;
    [SerializeField] private Sprite ES, NE, SW, WN;

    private WorldObstacleEntity[] neighborEntities = new WorldObstacleEntity[4];

    protected override void Start()
    {
        base.Start();

        GetNeighbors(true);
    }

    public void GetNeighbors(bool _recursive = false)
    {
        //North
        WorldEntity neighbor = GameManager.instance.GetWorldManager().
            GetWorldEntityAtPosition(new Vector2Int(GetWorldGridPosition().GetPosition().x, GetWorldGridPosition().GetPosition().y + 1));
        if(neighbor != null )
            if (neighbor.GetType() == typeof(WorldObstacleEntity))
                neighborEntities[0] = (WorldObstacleEntity)neighbor;

        //East
        neighbor = GameManager.instance.GetWorldManager().
            GetWorldEntityAtPosition(new Vector2Int(GetWorldGridPosition().GetPosition().x + 1, GetWorldGridPosition().GetPosition().y));
        if (neighbor != null)
            if (neighbor.GetType() == typeof(WorldObstacleEntity))
                neighborEntities[1] = (WorldObstacleEntity)neighbor;

        //South
        neighbor = GameManager.instance.GetWorldManager().
            GetWorldEntityAtPosition(new Vector2Int(GetWorldGridPosition().GetPosition().x, GetWorldGridPosition().GetPosition().y - 1));
        if (neighbor != null)
            if (neighbor.GetType() == typeof(WorldObstacleEntity))
                neighborEntities[2] = (WorldObstacleEntity)neighbor;

        //West
        neighbor = GameManager.instance.GetWorldManager().
            GetWorldEntityAtPosition(new Vector2Int(GetWorldGridPosition().GetPosition().x - 1, GetWorldGridPosition().GetPosition().y));
        if (neighbor != null)
            if (neighbor.GetType() == typeof(WorldObstacleEntity))
                neighborEntities[3] = (WorldObstacleEntity)neighbor;

        UpdateTexture();

        if(_recursive)
        {
            foreach (WorldObstacleEntity neighborEntity in neighborEntities)
            {
                if(neighborEntity != null)
                    neighborEntity.GetNeighbors();
            }
        }
    }

    public void UpdateTexture()
    {
        if (neighborEntities[0] == null && neighborEntities[1] == null && neighborEntities[2] == null && neighborEntities[3] == null)
        {
            spriteRenderer.sprite = isolated;
        }
        else if (neighborEntities[0] != null && neighborEntities[1] == null && neighborEntities[2] == null && neighborEntities[3] == null)
        {
            spriteRenderer.sprite = N;
        }
        else if (neighborEntities[0] == null && neighborEntities[1] != null && neighborEntities[2] == null && neighborEntities[3] == null)
        {
            spriteRenderer.sprite = E;
        }
        else if (neighborEntities[0] == null && neighborEntities[1] == null && neighborEntities[2] != null && neighborEntities[3] == null)
        {
            spriteRenderer.sprite = S;
        }
        else if (neighborEntities[0] == null && neighborEntities[1] == null && neighborEntities[2] == null && neighborEntities[3] != null)
        {
            spriteRenderer.sprite = W;
        }
        else if (neighborEntities[0] != null && neighborEntities[1] == null && neighborEntities[2] != null && neighborEntities[3] == null)
        {
            spriteRenderer.sprite = NS;
        }
        else if (neighborEntities[0] == null && neighborEntities[1] != null && neighborEntities[2] == null && neighborEntities[3] != null)
        {
            spriteRenderer.sprite = EW;
        }
        else if (neighborEntities[0] == null && neighborEntities[1] != null && neighborEntities[2] != null && neighborEntities[3] != null)
        {
            spriteRenderer.sprite = ESW;
        }
        else if (neighborEntities[0] != null && neighborEntities[1] != null && neighborEntities[2] != null && neighborEntities[3] == null)
        {
            spriteRenderer.sprite = NES;
        }
        else if (neighborEntities[0] != null && neighborEntities[1] == null && neighborEntities[2] != null && neighborEntities[3] != null)
        {
            spriteRenderer.sprite = SWN;
        }
        else if (neighborEntities[0] != null && neighborEntities[1] != null && neighborEntities[2] == null && neighborEntities[3] != null)
        {
            spriteRenderer.sprite = WNE;
        }
        else if (neighborEntities[0] != null && neighborEntities[1] != null && neighborEntities[2] != null && neighborEntities[3] != null)
        {
            spriteRenderer.sprite = NESW;
        }
        else if (neighborEntities[0] == null && neighborEntities[1] != null && neighborEntities[2] != null && neighborEntities[3] == null)
        {
            spriteRenderer.sprite = ES;
        }
        else if (neighborEntities[0] != null && neighborEntities[1] != null && neighborEntities[2] == null && neighborEntities[3] == null)
        {
            spriteRenderer.sprite = NE;
        }
        else if (neighborEntities[0] == null && neighborEntities[1] == null && neighborEntities[2] != null && neighborEntities[3] != null)
        {
            spriteRenderer.sprite = SW;
        }
        else if (neighborEntities[0] != null && neighborEntities[1] == null && neighborEntities[2] == null && neighborEntities[3] != null)
        {
            spriteRenderer.sprite = WN;
        }
        else
        {
            spriteRenderer.sprite = isolated;
        }
    }
}
