using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WorldTileObject : MonoBehaviour
{
    [SerializeField] Color color;
    [SerializeField] private GameObject gfx;
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    public void SetColor(Color _color)
    {
        gfx.SetActive(true);
        color = new Color(_color.r, _color.g, _color.b, .4f);

        foreach (SpriteRenderer spriteRenderer in spriteRenderers) 
        { 
            spriteRenderer.color = color;
        }
    }

    public void ResetTile()
    {
        gfx.SetActive(false);
    }
}
