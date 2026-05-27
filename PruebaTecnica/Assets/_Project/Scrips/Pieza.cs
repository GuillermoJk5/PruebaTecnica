using UnityEngine;

public class Pieza : MonoBehaviour
{
    // =========================
    // GRID DATA
    // =========================

    private Vector3Int gridPosition;
    private MoveDirection direction;

    public Vector3Int GridPosition => gridPosition;
    public MoveDirection Direction => direction;

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
    private GameObject terrainInstance;

    // =========================
    // ENUM
    // =========================

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
    // INIT
    // =========================

    public void Initialize()
    {
        GenerateRandomDirection();
        ApplyDirectionToSignals();
    }

    public void SetGridPosition(Vector3Int pos)
    {
        gridPosition = pos;
    }

    // =========================
    // MOVE INPUT
    // =========================

    private bool isMoving = false;
    private Vector3 moveDir;
    private float moveSpeed = 3f;

    private void OnMouseDown()
    {
        moveDir = GetMoveDirection();
        isMoving = true;
    }

    Vector3 GetMoveDirection()
    {
        return signalsRoot.right.normalized;
    }

    // =========================
    // UPDATE MOVEMENT
    // =========================

    private void Update()
    {
        if (!isMoving) return;

        var result = RuleManager.Instance.CheckMove(
            transform.position,
            moveDir,
            0.5f //checkDistance
        );

        switch (result)
        {
            case RuleManager.MoveResult.CanMove:
                transform.position += moveDir * moveSpeed * Time.deltaTime;
                break;

            case RuleManager.MoveResult.BlockedByPiece:
                isMoving = false;
                break;

            case RuleManager.MoveResult.OutOfBounds:
                Die();
                break;
        }
    }

    void Die()
    {
        gameObject.SetActive(false);
    }

    // =========================
    // DIRECTION
    // =========================

    void GenerateRandomDirection()
    {
        direction = (MoveDirection)Random.Range(0, 6);
    }

    void ApplyDirectionToSignals()
    {
        if (signalsRoot == null) return;

        signalsRoot.localRotation = Quaternion.Euler(GetRotation(direction));
    }

    Vector3Int GetRotation(MoveDirection dir)
    {
        switch (dir)
        {
            case MoveDirection.Front: return new Vector3Int(0, 0, 0);
            case MoveDirection.Right: return new Vector3Int(0, 90, 0);
            case MoveDirection.Back: return new Vector3Int(0, 180, 0);
            case MoveDirection.Left: return new Vector3Int(0, 270, 0);
            case MoveDirection.Up: return new Vector3Int(0, 0, 90);
            case MoveDirection.Down: return new Vector3Int(0, 0, 270);
            default: return Vector3Int.zero;
        }
    }

    // =========================
    // VISUAL
    // =========================

    public void SetBlockPrefab(GameObject newPrefab)
    {
        blockPrefab = newPrefab;
        SpawnVisual();
    }

    void SpawnVisual()
    {
        if (visualRoot == null || blockPrefab == null) return;

        if (terrainInstance != null)
            Destroy(terrainInstance);

        terrainInstance = Instantiate(
            blockPrefab,
            visualRoot.position,
            visualRoot.rotation,
            visualRoot
        );
    }
}