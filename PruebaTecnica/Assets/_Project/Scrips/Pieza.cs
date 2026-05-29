using UnityEngine;

public class Pieza : MonoBehaviour
{
    // =========================
    // GRID
    // =========================

    private Vector3Int gridPosition;

    public Vector3Int GridPosition => gridPosition;

    // =========================
    // DIRECTION
    // =========================

    private MoveDirection direction;

    public enum MoveDirection
    {
        Up,
        Right,
        Down,
        Left,
        Front,
        Back
    }

    // =========================
    // REFERENCES
    // =========================

    [Header("References")]
    [SerializeField] private Transform visualRoot;

    [SerializeField] private Transform signalsRoot;

    // =========================
    // VISUAL
    // =========================

    private GameObject blockPrefab;

    private GameObject visualInstance;

    // =========================
    // MOVEMENT
    // =========================

    [SerializeField] private float moveSpeed = 5f;

    private bool isMoving = false;

    private Vector3 targetPosition;

    private Vector3 moveDir;

    // =========================
    // INIT
    // =========================

    public void Initialize(MoveDirection dir)
    {
        direction = dir;

        ApplyDirectionToSignals();
    }

    public void SetGridPosition(Vector3Int pos)
    {
        gridPosition = pos;
    }

    // =========================
    // INPUT
    // =========================

    void OnMouseDown()
    {
        if (isMoving)
            return;

        moveDir = GetDirectionVector();

        targetPosition = CalculateTargetPosition();

        isMoving = true;
    }

    // =========================
    // UPDATE
    // =========================

   
    void Update()
    {
        if (!isMoving) { 
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
            LevelGenerator.Instance.MovePiece(
                 this,
                 gridPosition,
                 LevelGenerator.Instance.WorldToCell(targetPosition)
            );


            if (!LevelGenerator.Instance.IsInsidePlayableArea(transform.position))
            {
                Die();
            }
        }
    }

    // =========================
    // TARGET CALCULATION
    // =========================

    Vector3 CalculateTargetPosition()
    {
        Vector3 current = transform.position;

        while (true)
        {
            Vector3 next = current + moveDir;

            Vector3Int cell = LevelGenerator.Instance.WorldToCell(next);

            // ❌ SOLO bloquea si hay pieza (dentro del grid)
            if (LevelGenerator.Instance.IsInsideGrid(cell))
            {
                if (!LevelGenerator.Instance.CanMoveThrough(cell))
                {
                    return current;
                }
            }

            // ❌ SI SALE DEL PLAYABLE AREA → cortar
            if (!LevelGenerator.Instance.IsInsidePlayableArea(next))
            {
                return current;
            }

            current = next;
        }
    }

    // =========================
    // DIRECTION
    // =========================

    Vector3 GetDirectionVector()
    {
        return signalsRoot.right.normalized;
    }

    // =========================
    // DIE
    // =========================

    void Die()
    {
        gameObject.SetActive(false);
    }

    // =========================
    // VISUAL ROTATION
    // =========================

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

    // =========================
    // VISUAL PREFAB
    // =========================

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
}