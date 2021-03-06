using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public Camera mainCamera;
    public float cellSize = 1f;
    private readonly Dictionary<int, Dictionary<int, GameObject>> objects = new Dictionary<int, Dictionary<int, GameObject>>();
    private readonly List<Vector2Int> neighbors = new List<Vector2Int>();
    private float lastChanged;
    private int screenWidth;
    private int screenHeight;

    private Vector2Int bottomLeft;
    private Vector2Int topRight;

    public GridController()
    {
        neighbors.Add(new Vector2Int(1, 0));
        neighbors.Add(new Vector2Int(0, -1));
        neighbors.Add(new Vector2Int(-1, 0));
        neighbors.Add(new Vector2Int(0, 1));
    }

    private void Update()
    {
        var recalcBorder = false;
        if (Screen.width != screenWidth)
        {
            recalcBorder = true;
            screenWidth = Screen.width;
        }
        if (Screen.height != screenHeight)
        {
            recalcBorder = true;
            screenHeight = Screen.height;
        }

        if (recalcBorder)
        {
            bottomLeft = CellFromScreenCoord(0, 0);
            topRight = CellFromScreenCoord(screenWidth - 1, screenHeight - 1);
        }
    }

    public Vector2Int GetTopRightCornerCell()
    {
        return topRight;
    }

    public Vector2Int GetBottomLeftCornerCell()
    {
        return topRight;
    }

    private Vector2Int CellFromScreenCoord(int x, int y)
    {
        return WorldToCell(mainCamera.ScreenToWorldPoint(new Vector3(x, y, 0)));
    }

    public bool WithinScreen(Vector2Int cell)
    {
        if (cell.x < bottomLeft.x || cell.x > topRight.x)
        {
            return false;
        }

        if (cell.y < bottomLeft.y || cell.y > topRight.y)
        {
            return false;
        }

        return true;
    }

    public bool HasChangedSince(float t)
    {
        return lastChanged > t;
    }

    private float CoordToCell(float coord)
    {
        return coord / cellSize;
    }

    public Vector2Int MouseToCell()
    {
        return WorldToCell(mainCamera.ScreenToWorldPoint(Input.mousePosition));
    }

    public Vector3 MouseToCellCenter(float z = 0)
    {
        return CellToCellCenter(MouseToCell(), z);
    }

    public Vector3 MouseToCellCorner(float z = 0)
    {
        return CellToCellCorner(MouseToCell(), z);
    }

    public Vector3 WorldToCellCorner(Vector3 world)
    {
        var x = Mathf.Floor(CoordToCell(world.x)) * cellSize;
        var y = Mathf.Floor(CoordToCell(world.y)) * cellSize;
        
        return new Vector3(x, y, world.z);
    }

    public Vector3 WorldToCellCenter(Vector3 world)
    {
        var halfCell = cellSize / 2f;
        var x = Mathf.Floor(CoordToCell(world.x)) * cellSize + halfCell;
        var y = Mathf.Floor(CoordToCell(world.y)) * cellSize + halfCell;
        
        return new Vector3(x, y, world.z);
    }

    public Vector2Int WorldToCell(Vector3 world)
    {
        return new Vector2Int((int) Mathf.Floor(CoordToCell(world.x)), (int) Mathf.Floor(CoordToCell(world.y)));
    }

    public Vector3 CellToCellCorner(Vector2Int cell, float z = 0)
    {
        return new Vector3(cell.x * cellSize, cell.y * cellSize, z);
    }

    public Vector3 CellToCellCenter(Vector2Int cell, float z = 0)
    {
        var halfCell = cellSize / 2f;
        return new Vector3(cell.x * cellSize + halfCell, cell.y * cellSize + halfCell, z);
    }

    public GameObject GetObjectAt(Vector2Int cell)
    {
        Dictionary<int, GameObject> inner;
        if (!objects.TryGetValue(cell.x, out inner))
        {
            return null;
        }

        GameObject obj;
        if (!inner.TryGetValue(cell.y, out obj))
        {
            return null;
        }

        return obj;
    }

    public bool HasObjectAt(Vector2Int cell)
    {
        return GetObjectAt(cell);
    }

    public GameObject SetObjectAt(Vector2Int cell, GameObject obj)
    {
        Dictionary<int, GameObject> inner;
        if (!objects.TryGetValue(cell.x, out inner))
        {
            if (!obj)
            {
                return null;
            }
            inner = new Dictionary<int, GameObject>();
            objects.Add(cell.x, inner);
        }

        GameObject existing;
        inner.TryGetValue(cell.y, out existing);
        if (!obj)
        {
            inner.Remove(cell.y);
        }
        else
        {
            inner.Add(cell.y, obj);
        }

        lastChanged = Time.time;
        return existing;
    }

    public GameObject DeleteObjectAt(Vector2Int cell)
    {
        return SetObjectAt(cell, null);
    }

    public List<Vector2Int> GetNeighborsOf(Vector2Int cell)
    {
        var freeNeighbors = new List<Vector2Int>();
        for (var i = 0; i < neighbors.Count; i++)
        {
            var pos = cell + neighbors[i];
            if (WithinScreen(pos))
            {
                freeNeighbors.Add(pos);
            }
        }

        return freeNeighbors;
    }
}
