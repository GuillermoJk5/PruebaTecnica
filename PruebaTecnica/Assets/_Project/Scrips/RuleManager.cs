using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Grid Bounds")]
    [SerializeField]
    private Vector3Int minBounds =
        Vector3Int.zero;

    [SerializeField]
    private Vector3Int maxBounds =
        new Vector3Int(5, 5, 5);

    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 1f;

    private Dictionary<Vector3Int, Pieza> grid =
        new Dictionary<Vector3Int, Pieza>();

    void Awake()
    {
        Instance = this;
    }

    // =========================
    // VALIDATION
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

    // =========================
    // CONVERSION
    // =========================

    public Vector3 GridToWorld(Vector3Int pos)
    {
        return new Vector3(
            pos.x * cellSize,
            pos.y * cellSize,
            pos.z * cellSize
        );
    }

    // =========================
    // PLACE PIECE
    // =========================

    public bool PlacePiece(Pieza pieza, Vector3Int pos)
    {
        Debug.Log("TRY PLACE: " + pos);

        if (!IsValidCell(pos))
        {
            Debug.Log("REJECTED CELL");
            return false;
        }

        if (IsOccupied(pos))
        {
            Debug.Log("ALREADY OCCUPIED");
            return false;
        }

        grid[pos] = pieza;

        pieza.SetGridPosition(pos);

        pieza.transform.position =
            GridToWorld(pos);

        Debug.Log("PLACED AT: " + pos);

        return true;
    }


    // =========================
    // MOVIMIENTO
    // =========================

    public float pieceSpeed = 3f;
    public float checkDistance = 0.6f;

    public enum MoveResult
    {
        CanMove,
        BlockedByPiece,
        OutOfBounds
    }

    public MoveResult CheckMove(Vector3 worldPos, Vector3 dir)
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