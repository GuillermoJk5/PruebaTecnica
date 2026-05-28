using System.Collections.Generic;
using UnityEngine; 


//Sistema nuevo de generación basado en restricciones por líneas.
//Sustituye RuleManager + lógica antigua de generación.

public class ConstraintLevelGenerator : MonoBehaviour
{
    // =========================
    // PREFAB
    // =========================

    [Header("Piece Prefab")]
    public GameObject piecePrefab;

    // =========================
    // GRID SIZE
    // =========================

    public Vector3Int gridSize = new Vector3Int(6, 6, 6);

    // =========================
    // ENUM
    // =========================

    public enum MoveDirection
    {
        Up,
        Down,
        Right,
        Left,
        Front,
        Back
    }

    public enum LineState
    {
        None,

        // X axis (Right / Left)
        OnlyRight,
        OnlyLeft,

        // Y axis (Up / Down)
        OnlyUp,
        OnlyDown,

        // Z axis (Front / Back)
        OnlyFront,
        OnlyBack
    }

    // =========================
    // LINE CONSTRAINTS
    // =========================

    private Dictionary<Vector2Int, LineState> rowYZ = new();
    private Dictionary<Vector2Int, LineState> colXZ = new();
    private Dictionary<Vector2Int, LineState> depthXY = new();

    // =========================
    // GENERATION ENTRY
    // =========================

    void Start()
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        Clear();

        List<Vector3Int> order = GenerateOrder();

        foreach (var pos in order)
        {
            SpawnAt(pos);
        }
    }

    // =========================
    // ORDER (puedes cambiarlo luego)
    // =========================

    List<Vector3Int> GenerateOrder()
    {
        List<Vector3Int> list = new();

        for (int y = 0; y < gridSize.y; y++)
        {
            for (int z = 0; z < gridSize.z; z++)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    list.Add(new Vector3Int(x, y, z));
                }
            }
        }

        return list;
    }

    // =========================
    // SPAWN CORE
    // =========================

    void SpawnAt(Vector3Int pos)
    {
        GameObject obj = Instantiate(piecePrefab, pos, Quaternion.identity);

        Pieza pieza = obj.GetComponent<Pieza>();

        pieza.SetGridPosition(pos);

        MoveDirection dir = GetValidDirection(pos);

        ApplyConstraint(pos, dir);

        pieza.SetBlockPrefab(null);
    //    pieza.Initialize();
    }

    // =========================
    // DIRECTION SELECTION
    // =========================

    MoveDirection GetValidDirection(Vector3Int pos)
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

        // ROW (Y,Z) -> X axis
        Vector2Int row = new(pos.y, pos.z);
        if (rowYZ.TryGetValue(row, out LineState rowState))
        {
            if (rowState == LineState.OnlyRight) options.Remove(MoveDirection.Left);
            if (rowState == LineState.OnlyLeft) options.Remove(MoveDirection.Right);
        }

        // COLUMN (X,Z) -> Y axis
        Vector2Int col = new(pos.x, pos.z);
        if (colXZ.TryGetValue(col, out LineState colState))
        {
            if (colState == LineState.OnlyUp) options.Remove(MoveDirection.Down);
            if (colState == LineState.OnlyDown) options.Remove(MoveDirection.Up);
        }

        // DEPTH (X,Y) -> Z axis
        Vector2Int depth = new(pos.x, pos.y);
        if (depthXY.TryGetValue(depth, out LineState depthState))
        {
            if (depthState == LineState.OnlyFront) options.Remove(MoveDirection.Back);
            if (depthState == LineState.OnlyBack) options.Remove(MoveDirection.Front);
        }

        if (options.Count == 0)
            return MoveDirection.Up;

        return options[Random.Range(0, options.Count)];
    }

    // =========================
    // APPLY CONSTRAINTS
    // =========================

    void ApplyConstraint(Vector3Int pos, MoveDirection dir)
    {
        Vector2Int row = new(pos.y, pos.z);
        Vector2Int col = new(pos.x, pos.z);
        Vector2Int depth = new(pos.x, pos.y);

        switch (dir)
        {
            // X axis
            case MoveDirection.Right:
                rowYZ[row] = LineState.OnlyRight;
                break;

            case MoveDirection.Left:
                rowYZ[row] = LineState.OnlyLeft;
                break;

            // Y axis
            case MoveDirection.Up:
                colXZ[col] = LineState.OnlyUp;
                break;

            case MoveDirection.Down:
                colXZ[col] = LineState.OnlyDown;
                break;

            // Z axis
            case MoveDirection.Front:
                depthXY[depth] = LineState.OnlyFront;
                break;

            case MoveDirection.Back:
                depthXY[depth] = LineState.OnlyBack;
                break;
        }
    }

    // =========================
    // CLEAN
    // =========================

    void Clear()
    {
        foreach (var obj in FindObjectsOfType<Pieza>())
        {
            Destroy(obj.gameObject);
        }

        rowYZ.Clear();
        colXZ.Clear();
        depthXY.Clear();
    }
}
