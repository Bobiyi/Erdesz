using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TileScript : MonoBehaviour,Tile
{

    [SerializeField] private GridManagerScript gridManager;

    private SpriteRenderer _sprite;
    private Color _originalColor;
    private bool _isHighlighted;

    void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        if(_sprite != null)
        {
            _originalColor = _sprite.color;
        }

        if(gridManager == null)
        {
            gridManager = FindFirstObjectByType<GridManagerScript>();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
        if(gridManager == null)
        {
            Debug.LogError("Couldnt find gridManager", this);
        }
    }


    public void Highlight()
    {
        if (_sprite == null) return;
        if (_isHighlighted) return;
        _isHighlighted = true;

        var c = _originalColor;
        c.a = Mathf.Clamp01(_originalColor.a * 0.5f);
        _sprite.color = c;
    }

    public void unHighlight()
    {
        if (_sprite == null) return;
        if (!_isHighlighted) return;
        _isHighlighted = false;

        _sprite.color = _originalColor;
    }

    void OnMouseEnter()
    {
        gridManager?.HighlightTiles(this);
    }

    private void OnMouseExit()
    {
        gridManager?.ClearHighlights();
    }
}
