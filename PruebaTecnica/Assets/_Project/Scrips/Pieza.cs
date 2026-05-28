using UnityEngine;

public class Pieza : MonoBehaviour
{
    private Vector3Int gridPosition;
    private MoveDirection direction;

    public Vector3Int GridPosition => gridPosition;
    public MoveDirection Direction => direction;

    [Header("References")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Transform signalsRoot;

    private GameObject blockPrefab;
    private GameObject terrainInstance;

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
    // INIT (AHORA SIMPLE)
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
    // MOVEMENT (igual que antes)
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

    // private void Update()
    // {
    //     if (!isMoving) return;
    //
    //     Debug.DrawRay(transform.position, moveDir * 2f, Color.red);
    //
    //     var level = FindFirstObjectByType<LevelGenerator>();
    //
    //     if (!level.CanMove(transform.position, moveDir, 1f))
    //     {
    //         isMoving = false;
    //         Debug.Log("Choque");
    //         return;
    //     }
    //
    //     transform.position += moveDir * moveSpeed * Time.deltaTime;
    //
    //     if (!level.IsInsidePlayableArea(transform.position))
    //     {
    //         Die();
    //     }
    // }
    private LevelGenerator level;

    void Start()
    {
        level = FindFirstObjectByType<LevelGenerator>();
    }

    private void Update()
    {
        if (!isMoving) return;

        Debug.DrawRay(transform.position, moveDir * 2f, Color.red);

        if (!level.CanMove(transform.position, moveDir, 0.5f))
        {
            isMoving = false;
            Debug.Log("Choque");
            return;
        }

        transform.position += moveDir * moveSpeed * Time.deltaTime;

        if (!level.IsInsidePlayableArea(transform.position))
        {
            Die();
        }
    }

    void Die()
    {
        gameObject.SetActive(false);
    }

    // =========================
    // VISUAL
    // =========================

    void ApplyDirectionToSignals()
    {
        if (signalsRoot == null) return;

        signalsRoot.localRotation =
            Quaternion.Euler(GetRotation(direction));
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
        }
        return Vector3Int.zero;
    }

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