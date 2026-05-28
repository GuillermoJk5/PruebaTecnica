using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    // =========================
    // PREFAB
    // =========================

   
   

    [Header("Prefab")]
    public GameObject piecePrefab;

    // =========================
    // GRID SIZE
    // =========================

    [Header("Grid Bounds")]
    public Vector3Int minBounds = Vector3Int.zero;
    public Vector3Int maxBounds = new Vector3Int(5, 5, 5);

    [System.Serializable]
    public class FloorVisualData
    {
        public int yLevel;
        public GameObject defaultBlockPrefab;
    }

    [Header("Floor Visuals")]
    public List<FloorVisualData> floorVisuals = new();

    [System.Serializable]
    public class PieceVisualOverride
    {
        public Vector3Int position;
        public GameObject customBlockPrefab;
    }

    [Header("Overrides")]
    public List<PieceVisualOverride> overrides = new();

    private Dictionary<Vector3Int, Pieza> grid = new();

    // =========================
    // CONSTRAINT SYSTEM
    // =========================

    private ConstraintSystem constraints;

    void Awake()
    {
        constraints = new ConstraintSystem();
    }

    void Start()
    {
        GenerateLevel();
    }

    // =========================
    // GENERATION
    // =========================

    public void GenerateLevel()
    {
        ClearLevel();

        for (int y = 0; y <= maxBounds.y; y++)
        {
            for (int z = 0; z <= maxBounds.z; z++)
            {
                for (int x = 0; x <= maxBounds.x; x++)
                {
                    Spawn(new Vector3Int(x, y, z));
                }
            }
        }
    }

    void Spawn(Vector3Int pos)
    {
        GameObject obj = Instantiate(piecePrefab, pos, Quaternion.identity);
        Pieza pieza = obj.GetComponent<Pieza>();

        PlacePiece(pieza, pos);

        var dir = constraints.GetDirection(pos);
        constraints.Apply(pos, dir);

        pieza.SetGridPosition(pos);
        pieza.SetBlockPrefab(ResolvePrefab(pos));
        pieza.Initialize((Pieza.MoveDirection)dir);
    }

    // =========================
    // GRID CORE (ANTES RULEMANAGER)
    // =========================

    public bool IsInsidePlayableArea(Vector3 worldPos)
    {
        Vector3Int p = Vector3Int.RoundToInt(worldPos);

        Vector3Int min = Vector3Int.one * -2;
        Vector3Int max = Vector3Int.one * 6;

        return p.x >= min.x && p.x <= max.x &&
               p.y >= min.y && p.y <= max.y &&
               p.z >= min.z && p.z <= max.z;
    }

    public bool IsValidCell(Vector3Int pos)
    {
        return pos.x >= minBounds.x && pos.x <= maxBounds.x &&
               pos.y >= minBounds.y && pos.y <= maxBounds.y &&
               pos.z >= minBounds.z && pos.z <= maxBounds.z;
    }

    public bool IsOccupied(Vector3Int pos)
    {
        return grid.ContainsKey(pos);
    }

    public void PlacePiece(Pieza pieza, Vector3Int pos)
    {
        if (!IsValidCell(pos)) return;

        if (IsOccupied(pos))
        {
            Destroy(grid[pos].gameObject);
            grid.Remove(pos);
        }

        grid[pos] = pieza;
        pieza.transform.position = pos;
    }

    public Pieza GetPieceAt(Vector3Int pos)
    {
        grid.TryGetValue(pos, out Pieza p);
        return p;
    }

    public void RemovePiece(Vector3Int pos)
    {
        if (grid.ContainsKey(pos))
        {
            grid.Remove(pos);
        }
    }

    public void ClearLevel()
    {
        foreach (var p in FindObjectsOfType<Pieza>())
        {
            Destroy(p.gameObject);
        }

        grid.Clear();
        constraints.Clear();
    }

    // =========================
    // MOVEMENT CHECK (REEMPLAZA RULEMANAGER CHECKMOVE)
    // =========================

    public bool CanMove(Vector3 worldPos, Vector3 dir, float distance)
    {
        if (!IsInside(worldPos)) {  Debug.Log("FueraLimites");
            return false; }
          

        if (Physics.Raycast(worldPos, dir, distance))
            return false;

        return true;
    }

    bool IsInside(Vector3 pos)
    {
        Vector3Int p = Vector3Int.RoundToInt(pos);

        return p.x >= minBounds.x && p.x <= maxBounds.x &&
               p.y >= minBounds.y && p.y <= maxBounds.y &&
               p.z >= minBounds.z && p.z <= maxBounds.z;
    }

    // =========================
    // CONSTRAINT SYSTEM
    // =========================

    private class ConstraintSystem
    {
        private Dictionary<Vector2Int, State> rowYZ = new();
        private Dictionary<Vector2Int, State> colXZ = new();
        private Dictionary<Vector2Int, State> depthXY = new();

        public enum State
        {
            None,
            OnlyRight, OnlyLeft,
            OnlyUp, OnlyDown,
            OnlyFront, OnlyBack
        }

        public enum MoveDirection
        {
            Up, Down, Right, Left, Front, Back
        }

        public MoveDirection GetDirection(Vector3Int pos)
        {
            List<MoveDirection> options = new()
            {
                MoveDirection.Up,
                MoveDirection.Down,
                MoveDirection.Right,
                MoveDirection.Left,
                MoveDirection.Front,
                MoveDirection.Back
            };

            Vector2Int row = new(pos.y, pos.z);
            if (rowYZ.TryGetValue(row, out var r))
            {
                if (r == State.OnlyRight) options.Remove(MoveDirection.Left);
                if (r == State.OnlyLeft) options.Remove(MoveDirection.Right);
            }

            Vector2Int col = new(pos.x, pos.z);
            if (colXZ.TryGetValue(col, out var c))
            {
                if (c == State.OnlyUp) options.Remove(MoveDirection.Down);
                if (c == State.OnlyDown) options.Remove(MoveDirection.Up);
            }

            Vector2Int depth = new(pos.x, pos.y);
            if (depthXY.TryGetValue(depth, out var d))
            {
                if (d == State.OnlyFront) options.Remove(MoveDirection.Back);
                if (d == State.OnlyBack) options.Remove(MoveDirection.Front);
            }

            if (options.Count == 0)
                return MoveDirection.Up;

            return options[Random.Range(0, options.Count)];
        }

        public void Apply(Vector3Int pos, MoveDirection dir)
        {
            Vector2Int row = new(pos.y, pos.z);
            Vector2Int col = new(pos.x, pos.z);
            Vector2Int depth = new(pos.x, pos.y);

            switch (dir)
            {
                case MoveDirection.Right: rowYZ[row] = State.OnlyRight; break;
                case MoveDirection.Left: rowYZ[row] = State.OnlyLeft; break;
                case MoveDirection.Up: colXZ[col] = State.OnlyUp; break;
                case MoveDirection.Down: colXZ[col] = State.OnlyDown; break;
                case MoveDirection.Front: depthXY[depth] = State.OnlyFront; break;
                case MoveDirection.Back: depthXY[depth] = State.OnlyBack; break;
            }
        }

        public void Clear()
        {
            rowYZ.Clear();
            colXZ.Clear();
            depthXY.Clear();
        }
    }

    GameObject ResolvePrefab(Vector3Int pos)
    {
        // PRIORIDAD 1: override individual
        foreach (var o in overrides)
        {
            if (o.position == pos && o.customBlockPrefab != null)
                return o.customBlockPrefab;
        }

        // PRIORIDAD 2: floor default
        foreach (var f in floorVisuals)
        {
            if (f.yLevel == pos.y)
                return f.defaultBlockPrefab;
        }

        return null;
    }
}