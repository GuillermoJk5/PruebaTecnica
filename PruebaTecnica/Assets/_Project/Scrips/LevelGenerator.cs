using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Base Piece Prefab")]
    public GameObject piecePrefab;

    [System.Serializable]
    public class PieceSpawnData
    {
        public Vector3Int position;

        [Header("Visual")]
        public GameObject blockPrefab;
    }

    [Header("Level Pieces")]
    public List<PieceSpawnData> pieces =
        new List<PieceSpawnData>();

    void Start()
    {
        Debug.Log("LevelGenerator ejecutando");

        GenerateLevel();
    }

    void GenerateLevel()
    {
        foreach (var data in pieces)
        {
            SpawnPiece(data);
        }
    }

    void SpawnPiece(PieceSpawnData data)
    {
        GameObject obj =
            Instantiate(piecePrefab);

        Pieza pieza =
            obj.GetComponent<Pieza>();

        if (pieza == null)
        {
            Debug.LogError(
                "Prefab no tiene script Pieza"
            );

            return;
        }

        // GRID
        GridManager.Instance.PlacePiece(
            pieza,
            data.position
        );

        // VISUAL
        pieza.SetBlockPrefab(
            data.blockPrefab
        );

        // RANDOM DIRECTION
        pieza.Initialize();

        Debug.Log(
            "Spawning piece at: " +
            data.position
        );
    }
}