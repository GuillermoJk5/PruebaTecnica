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
    // INITIALIZE
    // =========================

    public void Initialize()
    {
      
        GenerateRandomDirection();
        Debug.Log("DIR: " + direction);
        ApplyDirectionToSignals();
        Debug.Log("DIR: " + direction);
    }

    // =========================
    // GRID SETTERS
    // =========================

    public void SetGridPosition(Vector3Int pos)
    {
        gridPosition = pos;
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
        if (signalsRoot == null)
        {
            Debug.LogWarning("SignalsRoot no asignado");
            return;
        }

        signalsRoot.localRotation = Quaternion.Euler(GetRotation(direction));
    }

    Vector3Int GetRotation(MoveDirection dir)
    {
        switch (dir)
        {
            case MoveDirection.Up:
                return new Vector3Int(0, 0, 0); 

            case MoveDirection.Right:
                return new Vector3Int(0, 90, 0);

            case MoveDirection.Down:
                return new Vector3Int(0, 180, 0);

            case MoveDirection.Left:
                return new Vector3Int(0, 270, 0);

            case MoveDirection.Front:
                return new Vector3Int(0, 0, 90);

            case MoveDirection.Back:
                return new Vector3Int(0, 0, 270);
            default:
                Debug.LogWarning("Error Switch");
                return new Vector3Int(0, 0, 0);
        }
    }

    public void ApplyRandomVisualDirection()
    {
        ApplyDirectionToSignals();
    }

    // =========================
    // VISUAL SYSTEM
    // =========================

    public void SetBlockPrefab(GameObject newPrefab)
    {
        blockPrefab = newPrefab;

        SpawnVisual();
    }

    void SpawnVisual()
    {
        if (visualRoot == null)
        {
            Debug.LogWarning("VisualRoot no asignado");
            return;
        }

        if (blockPrefab == null)
        {
            Debug.LogWarning("BlockPrefab no asignado");
            return;
        }

        if (terrainInstance != null)
        {
            Destroy(terrainInstance);
        }

        terrainInstance = Instantiate(
            blockPrefab,
            visualRoot.position,
            visualRoot.rotation,
            visualRoot
        );
    }

    // =========================
    // GRID
    // =========================

    public void PlaceOnGrid(Vector3Int pos)
    {
        GridManager.Instance.PlacePiece(this, pos);
    }

    private void OnMouseDown()
    {
        TryMove();
    }

    void TryMove()
    {
        Vector3Int dir = DirectionToVector(direction);
        Debug.Log("Mover");
        GridManager.Instance.SlidePiece(this, DirectionToVector(direction));

    }

    Vector3Int DirectionToVector(MoveDirection dir)
    {
        switch (dir)
        {
            case MoveDirection.Up:
                return new Vector3Int(0, 0, 1);

            case MoveDirection.Down:
                return new Vector3Int(0, 0, -1);

            case MoveDirection.Right:
                return new Vector3Int(1, 0, 0);

            case MoveDirection.Left:
                return new Vector3Int(-1, 0, 0);

            case MoveDirection.Front:
                return new Vector3Int(0, 1, 0);

            case MoveDirection.Back:
                return new Vector3Int(0, -1, 0);

            default:
                return Vector3Int.zero;
        }
    }

}