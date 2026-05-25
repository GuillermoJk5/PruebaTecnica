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
    // MOVE PIECE
    // =========================
  public void SlidePiece(Pieza pieza, Vector3Int dir)
  {
      Vector3Int currentPos = pieza.GridPosition;
      Vector3Int nextPos = currentPos + dir;
  
      // avanzar mientras sea válido y no esté ocupado
      while (IsValidCell(nextPos) && !IsOccupied(nextPos))
      {
          currentPos = nextPos;
          nextPos = currentPos + dir;
      }
  
      // si salió del grid ? destruir / desactivar
      if (!IsValidCell(nextPos))
      {
          grid.Remove(pieza.GridPosition);
  
          pieza.gameObject.SetActive(false);
          return;
      }
  
      // si chocó ? colocar en última posición válida
      grid.Remove(pieza.GridPosition);
  
      grid[currentPos] = pieza;
  
      pieza.SetGridPosition(currentPos);
  
      pieza.transform.position = GridToWorld(currentPos);
  }


  ////  public float moveSpeed = 10f;
  ////  public float cellMoveTime = 0.08f;
  ////
  ////  public void SlidePiece(Pieza pieza, Vector3Int dir)
    //{
    //    StartCoroutine(SlideRoutine(pieza, dir));
    //}
  ////
  ////  private IEnumerator SlideRoutine(Pieza pieza, Vector3Int dir)
    //{
    //    Vector3Int currentPos = pieza.GridPosition;
    //    Vector3Int nextPos = currentPos + dir;
    //
    //    // vamos guardando el último válido
    //    Vector3Int lastValidPos = currentPos;
    //
    //    while (true)
    //    {
    //        // si sale del grid ? destruir con efecto
    //        if (!IsValidCell(nextPos))
    //        {
    //            grid.Remove(pieza.GridPosition);
    //
    //            yield return StartCoroutine(
    //                PieceExitEffect(pieza)
    //            );
    //
    //            pieza.gameObject.SetActive(false);
    //            yield break;
    //        }
    //
    //        // si hay obstáculo ? parar
    //        if (IsOccupied(nextPos))
    //            break;
    //
    //        lastValidPos = nextPos;
    //
    //        Vector3 targetWorld =
    //            GridToWorld(nextPos);
    //
    //        yield return StartCoroutine(
    //            MoveToPosition(pieza.transform, targetWorld)
    //        );
    //
    //        currentPos = nextPos;
    //        nextPos = currentPos + dir;
    //    }
    //
    //    // actualizar grid final
    //    grid.Remove(pieza.GridPosition);
    //    grid[lastValidPos] = pieza;
    //
    //    pieza.SetGridPosition(lastValidPos);
    //}
  ////
  ////  private IEnumerator MoveToPosition(Transform obj, Vector3 target)
    //{
    //    Vector3 start = obj.position;
    //    float t = 0f;
    //
    //    while (t < 1f)
    //    {
    //        t += Time.deltaTime * moveSpeed;
    //
    //        obj.position = Vector3.Lerp(
    //            start,
    //            target,
    //            t
    //        );
    //
    //        yield return null;
    //    }
    //
    //    obj.position = target;
    //}
  ////
  ////  private IEnumerator PieceExitEffect(Pieza pieza)
  // {
  //     Transform t = pieza.transform;
  //
  //     Vector3 startScale = t.localScale;
  //
  //     float time = 0f;
  //
  //     while (time < 0.25f)
  //     {
  //         time += Time.deltaTime * 4f;
  //
  //         t.localScale = Vector3.Lerp(
  //             startScale,
  //             Vector3.zero,
  //             time
  //         );
  //
  //         t.Rotate(0, 720 * Time.deltaTime, 0);
  //
  //         yield return null;
  //     }
  // }

}