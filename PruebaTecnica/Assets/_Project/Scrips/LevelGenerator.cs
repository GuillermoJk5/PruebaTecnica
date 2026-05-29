using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    // ==========================================
    // CORES & SINGLETONS
    // ==========================================
    public static LevelGenerator Instance;
    private ConstraintSystem constraints;
    private Dictionary<Vector3Int, Pieza> grid = new();

    // ==========================================
    // INSPECTOR PROPERTIES
    // ==========================================
    [Header("Piece Prefab")]
    public GameObject piecePrefab;

    [Header("Grid Bounds")]
    public Vector3Int minBounds = Vector3Int.zero;
    public Vector3Int maxBounds = new Vector3Int(5, 5, 5);

    [Header("Playable Area")]
    public int playableBuffer = 2;

    [Header("Floor Visuals")]
    public List<FloorVisualData> floorVisuals = new();

    [Header("Overrides")]
    public List<PieceVisualOverride> overrides = new();

    private int activePiecesCount = 0;

    // ==========================================
    // DATA STRUCTURES & SERIALIZABLES
    // ==========================================
    [System.Serializable]
    public class FloorVisualData
    {
        public int yLevel;
        public GameObject defaultBlockPrefab;
    }

    [System.Serializable]
    public class PieceVisualOverride
    {
        public Vector3Int position;
        public GameObject customBlockPrefab;
    }

    // ==========================================
    // UNITY LIFECYCLE (Order of execution)
    // ==========================================
    void Awake()
    {
        Instance = this;
        constraints = new ConstraintSystem();
    }

    void Start()
    {
        GenerateLevel();
    }

    // ==========================================
    // CLEANUP METODS
    // ==========================================
    void ClearLevel()
    {
        foreach (var p in FindObjectsOfType<Pieza>())
        {
            Destroy(p.gameObject);
        }

        grid.Clear();
    }

    // ==========================================
    // GENERATION METODS (Alphabetical)
    // ==========================================
    public void GenerateLevel()
    {
        ClearLevel();

        // IMPORTANTE: Reseteamos los mapas de restricciones cada vez que se genera el nivel
        constraints.ResetConstraints();

        for (int y = minBounds.y; y <= maxBounds.y; y++)
        {
            for (int z = minBounds.z; z <= maxBounds.z; z++)
            {
                for (int x = minBounds.x; x <= maxBounds.x; x++)
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

        // Obtenemos dirección usando el enum unificado de Pieza
        Pieza.MoveDirection dir = constraints.GetDirection(pos);
        constraints.Apply(pos, dir);

        pieza.SetGridPosition(pos);
        pieza.SetBlockPrefab(ResolvePrefab(pos));
        pieza.Initialize(dir);
        activePiecesCount++;
    }

    // ==========================================
    // GRID CORE METODS (Alphabetical)
    // ==========================================
    public bool CanMoveThrough(Vector3Int cell)
    {
        // Si hay pieza → bloquea
        return !grid.ContainsKey(cell);
    }

    public bool IsInsideGrid(Vector3Int pos)
    {
        return pos.x >= minBounds.x &&
               pos.x <= maxBounds.x &&
               pos.y >= minBounds.y &&
               pos.y <= maxBounds.y &&
               pos.z >= minBounds.z &&
               pos.z <= maxBounds.z;
    }

    public void MovePiece(Pieza pieza, Vector3Int oldPos, Vector3Int newPos)
    {
        // 1. limpiar posición anterior SOLO si coincide
        if (grid.ContainsKey(oldPos) && grid[oldPos] == pieza)
        {
            grid.Remove(oldPos);
        }

        // 2. si la nueva posición está ocupada, se para
        if (grid.ContainsKey(newPos))
        {
            return;
        }

        // 3. asignar nueva posición en grid
        grid[newPos] = pieza;

        // 4. actualizar estado lógico de la pieza
        pieza.SetGridPosition(newPos);

        // 5. mover visualmente (si quieres sincronización inmediata)
        pieza.transform.position = newPos;
    }

    public void PlacePiece(Pieza pieza, Vector3Int pos)
    {
        if (grid.ContainsKey(pos))
        {
            Debug.Log("Se borra " + pos);
            Destroy(grid[pos].gameObject);
            grid.Remove(pos);
        }

        grid[pos] = pieza;

        pieza.transform.position = pos;
    }

    public void RemovePieceFromGrid(Vector3Int pos)
    {
        if (grid.ContainsKey(pos))
        {
            grid.Remove(pos);
        }
    }

    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        return Vector3Int.RoundToInt(worldPos);
    }

    // ==========================================
    // PLAYABLE AREA METODS (Alphabetical)
    // ==========================================
    public bool IsInsidePlayableArea(Vector3 pos)
    {
        Vector3Int p = Vector3Int.RoundToInt(pos);

        Vector3Int min = minBounds - Vector3Int.one * playableBuffer;
        Vector3Int max = maxBounds + Vector3Int.one * playableBuffer;

        return p.x >= min.x && p.x <= max.x &&
               p.y >= min.y && p.y <= max.y &&
               p.z >= min.z && p.z <= max.z;
    }

    // ==========================================
    // VISUALS METODS (Alphabetical)
    // ==========================================
    GameObject ResolvePrefab(Vector3Int pos)
    {
        // PRIORIDAD: override
        foreach (var o in overrides)
        {
            if (o.position == pos && o.customBlockPrefab != null)
                return o.customBlockPrefab;
        }

        // FLOOR
        foreach (var f in floorVisuals)
        {
            if (f.yLevel == pos.y)
                return f.defaultBlockPrefab;
        }

        return null;
    }

    // ==========================================
    // CONSTRAINT SYSTEM (Subclass)
    // ==========================================
    private class ConstraintSystem
    {
        // Guardamos la dirección asignada a cada línea. 
        // Si una línea ya tiene dirección, las demás piezas de esa línea deben coincidir en el eje de movimiento.
        private Dictionary<Vector2Int, Pieza.MoveDirection> lineXDirection = new(); // Clave: (Y, Z) -> Controla Front/Back
        private Dictionary<Vector2Int, Pieza.MoveDirection> lineYDirection = new(); // Clave: (X, Z) -> Controla Up/Down
        private Dictionary<Vector2Int, Pieza.MoveDirection> lineZDirection = new(); // Clave: (X, Y) -> Controla Right/Left

        // METODOS EN ORDEN ALFABÉTICO
        public void Apply(Vector3Int pos, Pieza.MoveDirection dir)
        {
            Vector2Int fixedYZ = new(pos.y, pos.z);
            Vector2Int fixedXZ = new(pos.x, pos.z);
            Vector2Int fixedXY = new(pos.x, pos.y);

            // Registramos la dirección de la línea si es la primera pieza en definirla
            if (dir == Pieza.MoveDirection.Front || dir == Pieza.MoveDirection.Back)
            {
                if (!lineXDirection.ContainsKey(fixedYZ)) lineXDirection[fixedYZ] = dir;
            }
            else if (dir == Pieza.MoveDirection.Up || dir == Pieza.MoveDirection.Down)
            {
                if (!lineYDirection.ContainsKey(fixedXZ)) lineYDirection[fixedXZ] = dir;
            }
            else if (dir == Pieza.MoveDirection.Right || dir == Pieza.MoveDirection.Left)
            {
                if (!lineZDirection.ContainsKey(fixedXY)) lineZDirection[fixedXY] = dir;
            }
        }

        private Vector3Int DirectionToVector(Pieza.MoveDirection dir)
        {
            // Mapeado geométrico exacto de los vectores según el método GetRotation() de tu script Pieza.cs
            return dir switch
            {
                Pieza.MoveDirection.Front => new Vector3Int(1, 0, 0),   // +X (Rotación 0,0,0)
                Pieza.MoveDirection.Back => new Vector3Int(-1, 0, 0),  // -X (Rotación 0,180,0)
                Pieza.MoveDirection.Left => new Vector3Int(0, 0, 1),   // +Z (Rotación 0,270,0)
                Pieza.MoveDirection.Right => new Vector3Int(0, 0, -1),  // -Z (Rotación 0,90,0)
                Pieza.MoveDirection.Up => new Vector3Int(0, 1, 0),   // +Y (Rotación 0,0,90)
                Pieza.MoveDirection.Down => new Vector3Int(0, -1, 0),  // -Y (Rotación 0,0,270)
                _ => Vector3Int.zero
            };
        }

        public Pieza.MoveDirection GetDirection(Vector3Int pos)
        {
            List<Pieza.MoveDirection> options = new()
            {
                Pieza.MoveDirection.Up, Pieza.MoveDirection.Down,
                Pieza.MoveDirection.Right, Pieza.MoveDirection.Left,
                Pieza.MoveDirection.Front, Pieza.MoveDirection.Back
            };

            Vector2Int fixedYZ = new(pos.y, pos.z);
            Vector2Int fixedXZ = new(pos.x, pos.z);
            Vector2Int fixedXY = new(pos.x, pos.y);

            // REGLA DE ORO: Si la fila/columna ya tiene una dirección de escape asignada por otra pieza,
            // eliminamos las direcciones que causarían un choque frontal o un bloqueo cruzado en esa línea.

            // Si la línea X ya se mueve en un sentido, las piezas de esa línea NO pueden ir en sentido opuesto
            if (lineXDirection.TryGetValue(fixedYZ, out var existingXDir))
            {
                options.Remove(existingXDir == Pieza.MoveDirection.Front ? Pieza.MoveDirection.Back : Pieza.MoveDirection.Front);
            }

            if (lineYDirection.TryGetValue(fixedXZ, out var existingYDir))
            {
                options.Remove(existingYDir == Pieza.MoveDirection.Up ? Pieza.MoveDirection.Down : Pieza.MoveDirection.Up);
            }

            if (lineZDirection.TryGetValue(fixedXY, out var existingZDir))
            {
                options.Remove(existingZDir == Pieza.MoveDirection.Right ? Pieza.MoveDirection.Left : Pieza.MoveDirection.Right);
            }

            // Filtrado inmediato por piezas vecinas ya existentes
            options.RemoveAll(dir => WouldCauseImmediateConflict(pos, dir));

            // Fallback seguro si se queda sin opciones (apuntar hacia el exterior más cercano)
            if (options.Count == 0)
            {
                return GetSafestFallback(pos);
            }

            return options[Random.Range(0, options.Count)];
        }

        private Pieza.MoveDirection GetSafestFallback(Vector3Int pos)
        {
            int distToMinX = pos.x - LevelGenerator.Instance.minBounds.x;
            int distToMaxX = LevelGenerator.Instance.maxBounds.x - pos.x;
            int distToMinZ = pos.z - LevelGenerator.Instance.minBounds.z;
            int distToMaxZ = LevelGenerator.Instance.maxBounds.z - pos.z;

            if (distToMinX <= distToMaxX && distToMinX <= distToMinZ && distToMinX <= distToMaxZ) return Pieza.MoveDirection.Back;
            if (distToMaxX <= distToMinZ && distToMaxX <= distToMaxZ) return Pieza.MoveDirection.Front;
            if (distToMinZ <= distToMaxZ) return Pieza.MoveDirection.Right; // Según el GetRotation de tu pieza, Right es -Z
            return Pieza.MoveDirection.Left;
        }

        public void ResetConstraints()
        {
            lineXDirection.Clear();
            lineYDirection.Clear();
            lineZDirection.Clear();
        }

        private bool WouldCauseImmediateConflict(Vector3Int pos, Pieza.MoveDirection dir)
        {
            Vector3Int target = pos + DirectionToVector(dir);
            if (!LevelGenerator.Instance.IsInsideGrid(target)) return false;
            if (!LevelGenerator.Instance.CanMoveThrough(target)) return true;
            return false;
        }
    }
}