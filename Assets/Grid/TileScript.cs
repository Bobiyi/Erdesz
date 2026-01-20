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

    private bool occupied = false;
    private Plant occupant = null;

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
        _sprite.sortingOrder=-1;
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

    /// <summary>
    /// Returns true if this tile currently has a plant.
    /// </summary>
    public bool IsOccupied => occupied;

    /// <summary>
    /// Current occupying plant (null if none).
    /// </summary>
    public Plant Occupant => occupant;

    /// <summary>
    /// Snap the plant to the tile and mark occupancy. If tile is already occupied, returns immediately.
    /// </summary>
    public void SnapPlantToGrid(Plant p)
    {
        if (occupied) return;
        if (p == null) return;

        p.transform.SetParent(this.transform, worldPositionStays: false);
        p.transform.localPosition = Vector3.zero;
        p.transform.localRotation = Quaternion.identity;

        var rb = p.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        occupant = p;
        occupied = true;

        // Let the plant know which tile it's on so it can release itself later.
    }

    /// <summary>
    /// Clear the occupant reference (called by Plant.ReleaseFromTile or other removal logic).
    /// </summary>
    public void ClearOccupant()
    {
        occupant = null;
        occupied = false;
    }
}
