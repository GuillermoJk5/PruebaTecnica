using System.Collections.Generic;
using UnityEngine;

public class RuleManager : MonoBehaviour
{
    public static RuleManager Instance;

    [Header("Grid Bounds")]
    [SerializeField] private Vector3Int minBounds = Vector3Int.zero;
    [SerializeField] private Vector3Int maxBounds = new Vector3Int(5, 5, 5);

    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 1f;

    private Dictionary<Vector3Int, Pieza> grid = new Dictionary<Vector3Int, Pieza>();

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

    public Vector3 GridToWorld(Vector3Int pos)
    {
        return new Vector3(
            pos.x * cellSize,
            pos.y * cellSize,
            pos.z * cellSize
        );
    }

    // =========================
    // PIECE PLACEMENT
    // =========================

    public bool PlacePiece(Pieza pieza, Vector3Int pos)
    {
        if (!IsValidCell(pos)) return false;
        if (IsOccupied(pos)) return false;

        grid[pos] = pieza;

        pieza.SetGridPosition(pos);
        pieza.transform.position = GridToWorld(pos);

        return true;
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

    // =========================
    // SIMULATION (ANTI DEADLOCK)
    // =========================

    class SimPiece
    {
        public Vector3Int pos;
        public Vector3Int dir;
        public bool active = true;
    }

    List<SimPiece> GetSimulationSnapshot()
    {
        List<SimPiece> sim = new();

        foreach (var kvp in grid)
        {
            Pieza pieza = kvp.Value;

            sim.Add(new SimPiece
            {
                pos = pieza.GridPosition,
                dir = VectorToGridDir(pieza)
            });
        }

        return sim;
    }

    Vector3Int VectorToGridDir(Pieza p)
    {
        Vector3 d = p.transform.right.normalized;

        return new Vector3Int(
            Mathf.RoundToInt(d.x),
            Mathf.RoundToInt(d.y),
            Mathf.RoundToInt(d.z)
        );
    }

    void SimulateStep(List<SimPiece> pieces)
    {
        Dictionary<Vector3Int, SimPiece> occupied = new();

        foreach (var p in pieces)
        {
            if (!p.active) continue;

            Vector3Int next = p.pos + p.dir;

            // fuera del mundo ? desaparece
            if (!IsInsideWorld(next))
            {
                p.active = false;
                continue;
            }

            // colisión ? no avanza
            if (occupied.ContainsKey(next))
                continue;

            p.pos = next;
            occupied[next] = p;
        }
    }

    bool AnyMovement(List<SimPiece> before, List<SimPiece> after)
    {
        for (int i = 0; i < before.Count; i++)
        {
            if (before[i].pos != after[i].pos)
                return true;
        }

        return false;
    }

    List<SimPiece> Clone(List<SimPiece> original)
    {
        List<SimPiece> copy = new();

        foreach (var p in original)
        {
            copy.Add(new SimPiece
            {
                pos = p.pos,
                dir = p.dir,
                active = p.active
            });
        }

        return copy;
    }

    public bool HasValidSimulation(int steps = 3)
    {
        var state = GetSimulationSnapshot();

        for (int i = 0; i < steps; i++)
        {
            var before = Clone(state);

            SimulateStep(state);

            if (AnyMovement(before, state))
                return true;
        }

        return false;
    }
}