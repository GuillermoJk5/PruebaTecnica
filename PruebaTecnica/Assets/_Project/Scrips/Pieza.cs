using UnityEngine;

public class Pieza : MonoBehaviour
{
    // ==========================================
    // ENUMS & CONFIGURATIONS
    // ==========================================
    public enum MoveDirection
    {
        Up,
        Right,
        Down,
        Left,
        Front,
        Back
    }

    // ==========================================
    // PROPERTIES & DATA ACCESORS
    // ==========================================
    private Vector3Int gridPosition;
    public Vector3Int GridPosition => gridPosition;

    // ==========================================
    // INSPECTOR PROPERTIES
    // ==========================================
    [Header("References")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Transform signalsRoot;

    [Header("Movement Config")]
    [SerializeField] private float moveSpeed = 5f;

    // ==========================================
    // INTERNAL STATES & FIELDS
    // ==========================================
    private GameObject blockPrefab;
    private MoveDirection direction;
    private bool isMoving = false;
    private Vector3 moveDir;
    private Vector3 targetPosition;
    private GameObject visualInstance;

    // ==========================================
    // UNITY LIFECYCLE
    // ==========================================
    void Update()
    {
        if (!isMoving)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            isMoving = false;

            Vector3Int finalCell = LevelGenerator.Instance.WorldToCell(targetPosition);

            // 1. LIMITES ABSOLUTOS (BUFFER EXTERIOR)
            Vector3Int minLimit = LevelGenerator.Instance.minBounds - Vector3Int.one * LevelGenerator.Instance.playableBuffer;
            Vector3Int maxLimit = LevelGenerator.Instance.maxBounds + Vector3Int.one * LevelGenerator.Instance.playableBuffer;

            if (finalCell.x <= minLimit.x || finalCell.x >= maxLimit.x ||
                finalCell.y <= minLimit.y || finalCell.y >= maxLimit.y ||
                finalCell.z <= minLimit.z || finalCell.z >= maxLimit.z)
            {
                // ¡Limpieza total! Borramos la pieza de su posición de origen en el cubo
                LevelGenerator.Instance.RemovePieceFromGrid(gridPosition);
                Die();
                return;
            }

            // 2. MOVIMIENTO HACIA EL AIRE (FUERA DEL CUBO PERO DENTRO DEL BUFFER)
            if (!LevelGenerator.Instance.IsInsideGrid(finalCell))
            {
                // En cuanto cruza el límite del cubo visible, liberamos su celda origen 
                // para que las piezas de atrás avancen sin problemas.
                LevelGenerator.Instance.RemovePieceFromGrid(gridPosition);
                Die();
            }
            // 3. MOVIMIENTO INTERNO NORMAL (DENTRO DEL CUBO)
            else
            {
                // Se movió a un hueco libre dentro del puzle: actualizamos el grid normalmente
                LevelGenerator.Instance.MovePiece(this, gridPosition, finalCell);
            }
        }
    }

    // ==========================================
    // CORE LOGIC & INITIALIZATION (Alphabetical)
    // ==========================================
    public void Initialize(MoveDirection dir)
    {
        direction = dir;
        ApplyDirectionToSignals();
    }

    public void SetGridPosition(Vector3Int pos)
    {
        gridPosition = pos;
    }

    // ==========================================
    // INPUT METODS (Alphabetical)
    // ==========================================
    void OnMouseDown()
    {
        if (isMoving)
            return;

        moveDir = GetDirectionVector();
        targetPosition = CalculateTargetPosition();
        isMoving = true;
    }

    // ==========================================
    // MOVEMENT & CALCULATION METODS (Alphabetical)
    // ==========================================
    Vector3 CalculateTargetPosition()
    {
        Vector3 current = transform.position;

        while (true)
        {
            Vector3 next = current + moveDir;
            Vector3Int cell = LevelGenerator.Instance.WorldToCell(next);

            // 1. Si está dentro del cubo de juego, comprobamos choques normales
            if (LevelGenerator.Instance.IsInsideGrid(cell))
            {
                if (!LevelGenerator.Instance.CanMoveThrough(cell))
                {
                    return current; // Se frena con otra pieza
                }
            }
            // 2. Si ya salió del cubo de juego, está en el aire...
            else
            {
                // Solo detenemos el cálculo si choca contra el límite del "universo" (PlayableArea)
                if (!LevelGenerator.Instance.IsInsidePlayableArea(next))
                {
                    return current;
                }
            }

            current = next;
        }
    }

    Vector3 GetDirectionVector()
    {
        return signalsRoot.right.normalized;
    }

    // ==========================================
    // VISUAL MANAGEMENT METODS (Alphabetical)
    // ==========================================
    void ApplyDirectionToSignals()
    {
        if (signalsRoot == null)
            return;

        signalsRoot.localRotation =
            Quaternion.Euler(GetRotation(direction));
    }

    Vector3Int GetRotation(MoveDirection dir)
    {
        switch (dir)
        {
            case MoveDirection.Front:
                return new Vector3Int(0, 0, 0);

            case MoveDirection.Right:
                return new Vector3Int(0, 90, 0);

            case MoveDirection.Back:
                return new Vector3Int(0, 180, 0);

            case MoveDirection.Left:
                return new Vector3Int(0, 270, 0);

            case MoveDirection.Up:
                return new Vector3Int(0, 0, 90);

            case MoveDirection.Down:
                return new Vector3Int(0, 0, 270);
        }

        return Vector3Int.zero;
    }

    public void SetBlockPrefab(GameObject prefab)
    {
        blockPrefab = prefab;
        SpawnVisual();
    }

    void SpawnVisual()
    {
        if (visualRoot == null || blockPrefab == null)
            return;

        if (visualInstance != null)
        {
            Destroy(visualInstance);
        }

        visualInstance = Instantiate(
            blockPrefab,
            visualRoot.position,
            visualRoot.rotation,
            visualRoot
        );
    }

    // ==========================================
    // LIFECYCLE DESTRUCTION METODS (Alphabetical)
    // ==========================================
    void Die()
    {
        LevelGenerator.Instance.OnPieceDestroyed();
        gameObject.SetActive(false);
    }
}