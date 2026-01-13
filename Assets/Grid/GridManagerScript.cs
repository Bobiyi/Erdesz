using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;

public class GridManagerScript : MonoBehaviour
{
    [Header("Tile source")]
    [SerializeField] private Transform tilesParent;
    [SerializeField] private float Tolerance = 0.01f;

    [Header("Prefab Grid Generation")]
    [Tooltip("If assigned, the manager will instantiate this prefab into a grid when the parent has no children.")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private int rows = 5;
    [SerializeField] private int columns = 9;

    [Tooltip("Manual cell size. If Use Prefab Size is enabled, this is used as a fallback.")]
    [SerializeField] private Vector2 cellSize = new Vector2(1f, 1f);

    [Tooltip("When true, measure the prefab's renderer/rect size and use that (plus Padding) as the cell size.")]
    [SerializeField] private bool usePrefabSize = true;
    [Tooltip("Extra spacing to add to the measured prefab size to prevent overlap.")]
    [SerializeField] private Vector2 padding = new Vector2(0.0f, 0.0f);

    [SerializeField] private List<List<Tile>> Tiles = new List<List<Tile>>();

    void Awake()
    {
        if (tilePrefab != null && (tilesParent == null || tilesParent.childCount == 0))
        {
            GenerateGridFromPrefab();
        }
        else
        {
            FillTiles();
        }
    }

    public void HighlightTiles(Tile tile)
    {
        if (tile == null) return;

        if(!TryGetTileIndices(tile, out int row, out int col))
        {
            Debug.LogWarning("HighlightTiles: provided tile not found in grid.", this);
            return;
        }
        ClearHighlights();
        HighlightRow(row);
        HighlightColumn(col);
    }

    public void ClearHighlights()
    {
        if (Tiles == null) return;

        for (int r = 0; r < Tiles.Count; r++)
        {
            var row = Tiles[r];
            if (row == null) continue;
            for (int c = 0; c < row.Count; c++)
            {
                try
                {
                    row[c]?.unHighlight();
                }
                catch(Exception ex)
                {
                    Debug.LogError($"ClearHighlights: error calling Unhighlight on tile at [{r},{c}]: {ex}");
                }
            }
        }
    }

    private void HighlightRow(int row)
    {
        if (Tiles == null || Tiles.Count == 0) return;
        if (row < 0 || row >= Tiles.Count) return;

        var rowList = Tiles[row];
        if (rowList == null) return;

        for(int c = 0; c<rowList.Count; c++)
        {
            var t = rowList[c];
            try
            {
                t?.Highlight();
            }
            catch (Exception ex)
            {
                Debug.LogError($"HighlightRow: error calling Highlight on tile at row {row} col {c}: {ex}");
            }
        }
    }

    private void HighlightColumn(int col)
    {
        if (Tiles == null || Tiles.Count == 0) return;
        if (col < 0) return;

        for(int r = 0; r < Tiles.Count;r++)
        {
            var rowList = Tiles[r];
            if (rowList == null) continue;
            if (col >= rowList.Count) continue;

            var t = rowList[col];
            try
            {
                t?.Highlight();
            } 
            catch(Exception ex)
            {
                Debug.LogError($"HighlightColumn: error calling Highlight on tile at col {col} row {r}: {ex}");
            }
        }
    }

    private void FillTiles()
    {
        var parent = tilesParent != null ? tilesParent : this.transform;
        var found = parent.GetComponentsInChildren<TileScript>(includeInactive: true);

        if (found == null || found.Length == 0)
        {
            Debug.LogWarning("no TileScript components found under" + parent.name, this);
            Tiles = new List<List<Tile>>();
            return;
        }

        Func<float, float> snap = v => Mathf.Round(v / Tolerance) * Tolerance;

        var xs = found.Select(t => snap(t.transform.localPosition.x)).Distinct().OrderBy(x => x).ToArray();
        var ys = found.Select(t => snap(t.transform.localPosition.y)).Distinct().OrderByDescending(y => y).ToArray();

        Tiles = new List<List<Tile>>(ys.Length);
        for (int r = 0; r < ys.Length; r++)
        {
            var row = new List<Tile>(new Tile[xs.Length]);
            Tiles.Add(row);
        }

        foreach (var tile in found)
        {
            var sx = snap(tile.transform.localPosition.x);
            var sy = snap(tile.transform.localPosition.y);

            int col = Array.IndexOf(xs, sx);
            int row = Array.IndexOf(ys, sy);

            if (row < 0 || col < 0)
            {
                Debug.Log($"TileManager: tile at {tile.transform.position} couldnt be mapped to grid indexes (sx={sx}, sy={sy})", this);
                continue;
            }

            Tiles[row][col] = tile;
        }

        Debug.Log($"GridManager : filled Tiles grid {ys.Length} rows x {xs.Length} cols from parent '{parent.name}'.");
    }

    private void GenerateGridFromPrefab()
    {
        if (tilePrefab == null)
        {
            Debug.LogWarning("tilePrefab is null - cannot generate grid.", this);
            return;
        }

        if (tilesParent == null) tilesParent = this.transform;

        // Determine actual cell size to avoid overlaps.
        Vector2 actualCellSize = cellSize;
        if (usePrefabSize)
        {
            // Temporarily instantiate one prefab to measure its rendered/rect size
            var measure = Instantiate(tilePrefab, tilesParent);
            measure.transform.localPosition = Vector3.zero;
            measure.transform.localRotation = Quaternion.identity;
            measure.transform.localScale = tilePrefab.transform.localScale;

            // Try common size providers: SpriteRenderer, MeshRenderer, RectTransform
            Vector3 measured = Vector3.zero;
            var sr = measure.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                measured = sr.bounds.size;
            }
            else
            {
                var mr = measure.GetComponentInChildren<MeshRenderer>();
                if (mr != null)
                {
                    measured = mr.bounds.size;
                }
                else
                {
                    var rt = measure.GetComponentInChildren<RectTransform>();
                    if (rt != null)
                    {
                        // RectTransform.rect is in local space; account for lossyScale
                        var rect = rt.rect;
                        measured = new Vector3(rect.width * rt.lossyScale.x, rect.height * rt.lossyScale.y, 0f);
                    }
                    else
                    {
                        // fall back to renderer-less objects: try collider
                        var col = measure.GetComponentInChildren<Collider>();
                        if (col != null) measured = col.bounds.size;
                        else
                        {
                            // nothing measured, keep fallback
                            measured = new Vector3(cellSize.x, cellSize.y, 0f);
                        }
                    }
                }
            }

            // destroy the temporary measurement instance
            DestroyImmediate(measure);

            actualCellSize = new Vector2(Mathf.Max(0.0001f, measured.x) + padding.x, Mathf.Max(0.0001f, measured.y) + padding.y);
        }

        Tiles = new List<List<Tile>>(rows);
        for (int r = 0; r < rows; r++)
        {
            var row = new List<Tile>(new Tile[columns]);
            Tiles.Add(row);
        }

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                var go = Instantiate(original: tilePrefab, parent: tilesParent);
                // Positioning: top-left origin (row 0 = top).
                var localPos = new Vector3(c * actualCellSize.x, -r * actualCellSize.y, 1f);
                go.transform.localPosition = localPos;
                go.gameObject.name = $"Tile([{r},{c}])";
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = tilePrefab.transform.localScale;

                var tileComp = go.GetComponent<TileScript>();
                if (tileComp != null)
                {
                    Tiles[r][c] = tileComp;
                }
                else
                {
                    Debug.LogWarning($"Instantiated prefab at row {r} col {c} has no TileScript", this);
                }
            }
        }

        Debug.Log($"GridManager: generated {rows}x{columns} grid from prefab '{tilePrefab.name}' using cell size {actualCellSize}.");
    }

    /// <summary>
    /// Try to find the row/col indices of a tile in the Tiles grid.
    /// Returns false if not found.
    /// </summary>
    private bool TryGetTileIndices(Tile tile, out int foundRow, out int foundCol)
    {
        foundRow = -1;
        foundCol = -1;
        if (tile == null || Tiles == null) return false;

        for (int r = 0; r < Tiles.Count; r++)
        {
            var rowList = Tiles[r];
            if (rowList == null) continue;

            for (int c = 0; c < rowList.Count; c++)
            {
                if (rowList[c] == tile)
                {
                    foundRow = r;
                    foundCol = c;
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the nearest tile to the provided world position, or null if none found within maxDistance.
    /// </summary>
    public Tile GetNearestTile(Vector3 worldPosition, float maxDistance = Mathf.Infinity)
    {
        if (Tiles == null) return null;

        Tile best = null;
        float bestSqr = maxDistance <= 0f ? 0f : maxDistance * maxDistance;

        // if maxDistance is Infinity, set bestSqr to Mathf.Infinity so any distance is valid
        if (float.IsInfinity(bestSqr)) bestSqr = float.PositiveInfinity;

        for (int r = 0; r < Tiles.Count; r++)
        {
            var row = Tiles[r];
            if (row == null) continue;
            for (int c = 0; c < row.Count; c++)
            {
                var t = row[c] as TileScript;
                if (t == null) continue;
                var tp = t.transform.position;
                float sqr = (tp - worldPosition).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = t;
                }
            }
        }

        return best;
    }
}
