using System.Collections.Generic;
using UnityEngine;

public class RuleManager : MonoBehaviour
{
    public static RuleManager Instance;

    // =========================
    // GRID DATA
    // =========================

    [Header("Grid Bounds")]
    [SerializeField] private Vector3Int minBounds = Vector3Int.zero;
    [SerializeField] private Vector3Int maxBounds = new Vector3Int(5, 5, 5);

    private Dictionary<Vector3Int, Pieza> grid = new Dictionary<Vector3Int, Pieza>();

    public Vector3Int MinBounds => minBounds;
    public Vector3Int MaxBounds => maxBounds;

    void Awake()
    {
        Instance = this;
    }

    // =========================
    // GRID CORE
    // =========================

    public bool IsValidCell(Vector3Int pos)
    {
        return pos.x >= minBounds.x &&
               pos.x <= maxBounds.x &&
               pos.y >= minBounds.y &&
               pos.y <= maxBounds.y &&
               pos.z >= minBounds.z &&
               pos.z <= maxBounds.z;
    }

    public bool IsOccupied(Vector3Int pos)
    {
        return grid.ContainsKey(pos);
    }

    public Dictionary<Vector3Int, Pieza> GetGrid() { return grid; }

    // =========================
    // PIECE PLACEMENT
    // =========================

    public bool PlacePiece(Pieza pieza, Vector3Int pos)
    {
        if (!IsValidCell(pos)) return false;
        if (IsOccupied(pos)) return false;

        grid[pos] = pieza;

        pieza.SetGridPosition(pos);
        pieza.transform.position = pos;

        return true;
    }

    public Pieza GetPieceAt(Vector3Int pos)
    {
        if (grid.TryGetValue(pos, out Pieza pieza))
        {
            return pieza;
        }

        return null;
    }

    public void RemovePiece(Vector3Int pos)
    {
        if (grid.ContainsKey(pos))
        {
            grid.Remove(pos);
        }
    }

    public void ClearGrid()
    {
        grid.Clear();
    }

    // =========================
    // MOVEMENT RULES
    // =========================

    public enum MoveResult
    {
        CanMove,
        BlockedByPiece,
        OutOfBounds
    }

    public MoveResult CheckMove(Vector3 worldPos, Vector3 dir, float checkDistance)
    {
        if (!IsInsideWorld(worldPos))
            return MoveResult.OutOfBounds;

        if (Physics.Raycast(worldPos, dir, checkDistance))
            return MoveResult.BlockedByPiece;

        return MoveResult.CanMove;
    }

    bool IsInsideWorld(Vector3 pos)
    {
        Vector3Int p = Vector3Int.RoundToInt(pos);

        return p.x >= minBounds.x - 2 && p.x <= maxBounds.x + 2 &&
               p.y >= minBounds.y - 2 && p.y <= maxBounds.y + 2 &&
               p.z >= minBounds.z - 2 && p.z <= maxBounds.z + 2;
    }

   
}