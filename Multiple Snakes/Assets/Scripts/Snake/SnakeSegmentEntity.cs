using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class SnakeSegmentEntity : WorldEntity
{
    [SerializeField] private Snake snake;
    [SerializeField] private int segmentIndex;

    [SerializeField] private Sprite midSegmentSprite;
    [SerializeField] private Sprite endSegmentSprite;
    [SerializeField] private Sprite turnSegmentSprite;

    [SerializeField] private SpriteRenderer segmentSpriteRenderer;

    // Start is called before the first frame update
    protected override void Start()
    {
        networkObject = GetComponent<NetworkObject>();

        GameManager.instance.GetWorldManager().AddWorldEntity(this);

        ParticleSystem.MainModule particles = Instantiate(destroyParticleEffect, transform.position, Quaternion.identity).GetComponentInChildren<ParticleSystem>().main;
        particles.startColor = segmentSpriteRenderer.color;
    }

    public void SetParentSnake(Snake _snake)
    {
        snake = _snake;
    }

    public void SetSegmentColor(Color _color)
    {
        segmentSpriteRenderer.color = _color;
    }

    public void SetSegmentAlpha(float _alpha)
    {
        segmentSpriteRenderer.color = new Color(segmentSpriteRenderer.color.r, segmentSpriteRenderer.color.g, segmentSpriteRenderer.color.b, _alpha);
    }

    public void SetMidSegment()
    {
        segmentSpriteRenderer.sprite = midSegmentSprite;
        segmentSpriteRenderer.gameObject.transform.localScale =
                    new Vector3(Mathf.Abs(segmentSpriteRenderer.gameObject.transform.localScale.x), 
                    Mathf.Abs(segmentSpriteRenderer.gameObject.transform.localScale.y), 1);
    }

    public void SetEndSegment()
    {
        segmentSpriteRenderer.sprite = endSegmentSprite;
        segmentSpriteRenderer.gameObject.transform.localScale =
                    new Vector3(Mathf.Abs(segmentSpriteRenderer.gameObject.transform.localScale.x), 
                    Mathf.Abs(segmentSpriteRenderer.gameObject.transform.localScale.y), 1);
    }

    public void SetTurnSegment(Vector2Int _direction)
    {
        segmentSpriteRenderer.sprite = turnSegmentSprite;

        if(_direction == WorldGridDirection.UP)
        {
            if (GetWorldGridPosition().GetDirection() == WorldGridDirection.RIGHT)
                segmentSpriteRenderer.gameObject.transform.localScale = 
                    new Vector3(-Mathf.Abs(segmentSpriteRenderer.gameObject.transform.localScale.x), 
                    -Mathf.Abs(segmentSpriteRenderer.gameObject.transform.localScale.y), 1);
            else
                segmentSpriteRenderer.gameObject.transform.localScale =
                    new Vector3(Mathf.Abs(segmentSpriteRenderer.gameObject.transform.localScale.x), 
                    -Mathf.Abs(segmentSpriteRenderer.gameObject.transform.localScale.y), 1);
        }
        else if (_direction == WorldGridDirection.RIGHT)
        {
            if (GetWorldGridPosition().GetDirection() == WorldGridDirection.DOWN)
                segmentSpriteRenderer.gameObject.transform.localScale =
                    new Vector3(-Mathf.Abs(segmentSpriteRenderer.gameObject.transform.localScale.x),
                    -Mathf.Abs(segmentSpriteRenderer.gameObject.transform.localScale.y), 1);
            else
                segmentSpriteRenderer.gameObject.transform.localScale =
                    new Vector3(Mathf.Abs(segmentSpriteRenderer.gameObject.transform.localScale.x),
                    -Mathf.Abs(segmentSpriteRenderer.gameObject.transform.localScale.y), 1);
        }
        else if (_direction == WorldGridDirection.LEFT)
        {
            if (GetWorldGridPosition().GetDirection() == WorldGridDirection.UP)
                segmentSpriteRenderer.gameObject.transform.localScale =
                    new Vector3(-Mathf.Abs(segmentSpriteRenderer.gameObject.transform.localScale.x),
                    -Mathf.Abs(segmentSpriteRenderer.gameObject.transform.localScale.y), 1);
            else
                segmentSpriteRenderer.gameObject.transform.localScale =
                    new Vector3(Mathf.Abs(segmentSpriteRenderer.gameObject.transform.localScale.x),
                    -Mathf.Abs(segmentSpriteRenderer.gameObject.transform.localScale.y), 1);
        }
        else if (_direction == WorldGridDirection.DOWN)
        {
            if (GetWorldGridPosition().GetDirection() == WorldGridDirection.LEFT)
                segmentSpriteRenderer.gameObject.transform.localScale =
                    new Vector3(-Mathf.Abs(segmentSpriteRenderer.gameObject.transform.localScale.x),
                    -Mathf.Abs(segmentSpriteRenderer.gameObject.transform.localScale.y), 1);
            else
                segmentSpriteRenderer.gameObject.transform.localScale =
                    new Vector3(Mathf.Abs(segmentSpriteRenderer.gameObject.transform.localScale.x),
                    -Mathf.Abs(segmentSpriteRenderer.gameObject.transform.localScale.y), 1);
        }
    }

    public override void DestroyEvent()
    {
        if (destroyParticleEffect != null)
        {
            ParticleSystem.MainModule particles = Instantiate(destroyParticleEffect, transform.position, Quaternion.identity).GetComponentInChildren<ParticleSystem>().main;
            particles.startColor = new Color(segmentSpriteRenderer.color.r, segmentSpriteRenderer.color.g, segmentSpriteRenderer.color.b, 1f);
        }
    }

    public override void SetWorldGridPosition(WorldGridPosition _worldGridPosition) 
    { 
        base.SetWorldGridPosition(_worldGridPosition);

        transform.position = new Vector2(_worldGridPosition.GetPosition().x, _worldGridPosition.GetPosition().y);
        transform.transform.eulerAngles = _worldGridPosition.GetVector3Rotation();
    }

    public int GetSnakeSegmentIndex() { return segmentIndex; }
    public void SetSnakeSegmentIndex(int _segmentIndex) {  segmentIndex = _segmentIndex; }

    public Snake GetParentSnake() { return snake; }
}
