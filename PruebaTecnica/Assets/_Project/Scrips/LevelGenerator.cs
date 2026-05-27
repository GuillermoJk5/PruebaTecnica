using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    // =========================
    // PREFABS
    // =========================

    [Header("Base Piece Prefab")]
    public GameObject piecePrefab;

    // =========================
    // LEVEL DATA
    // =========================

    [System.Serializable]
    public class PieceSpawnData
    {
        public Vector3Int position;
        public GameObject customBlockPrefab;
    }

    [Header("Level Pieces")]
    public List<PieceSpawnData> pieces = new List<PieceSpawnData>();

    // =========================
    // FLOOR VISUALS
    // =========================

    [System.Serializable]
    public class FloorVisualData
    {
        public int yLevel;
        public GameObject defaultBlockPrefab;
    }

    [Header("Floor Visuals")]
    public List<FloorVisualData> floorVisuals =
        new List<FloorVisualData>();

    // =========================
    // SETTINGS
    // =========================

    [Header("Generation Settings")]
    public int maxAttempts = 50;

    // =========================
    // START
    // =========================

    void Start()
    {
        GenerateLevel();
    }

    // =========================
    // GENERATION FLOW
    // =========================

    void GenerateLevel()
    {
        int attempts = 0;
        bool validLevel = false;

        while (!validLevel && attempts < maxAttempts)
        {
            attempts++;

            ClearLevel();
            SpawnAllPieces();

            validLevel = RuleManager.Instance.HasValidSimulation();
        }

        if (!validLevel)
        {
            Debug.LogError("No se pudo generar un nivel válido tras varios intentos.");
            return;
        }

        Debug.Log($"Nivel generado correctamente en {attempts} intentos.");
    }

    // =========================
    // SPAWN SYSTEM
    // =========================

    void SpawnAllPieces()
    {
        SpawnFloorPieces();

        foreach (var data in pieces)
        {
            SpawnPiece(data);
        }
    }

    void SpawnFloorPieces()
    {
        Vector3Int min = RuleManager.Instance.MinBounds;
        Vector3Int max = RuleManager.Instance.MaxBounds;

        foreach (var floor in floorVisuals)
        {
            for (int x = min.x; x <= max.x; x++)
            {
                for (int z = min.z; z <= max.z; z++)
                {
                    Vector3Int pos = new Vector3Int(x, floor.yLevel, z);

                    SpawnFloorPiece(pos, floor.defaultBlockPrefab);
                }
            }
        }
    }

    void SpawnFloorPiece(Vector3Int position, GameObject prefab)
    {
        GameObject obj = Instantiate(piecePrefab);

        Pieza pieza = obj.GetComponent<Pieza>();

        if (pieza == null)
        {
            Debug.LogError("Prefab sin Pieza");
            return;
        }

        RuleManager.Instance.PlacePiece(pieza, position);

        pieza.SetBlockPrefab(prefab);
        pieza.Initialize();
    }

    void SpawnPiece(PieceSpawnData data)
    {
        GameObject obj = Instantiate(piecePrefab);

        Pieza pieza = obj.GetComponent<Pieza>();

        if (pieza == null)
        {
            Debug.LogError("El prefab no tiene el script Pieza");
            return;
        }

        if (RuleManager.Instance.IsOccupied(data.position))
        {
            Pieza oldPiece = RuleManager.Instance.GetPieceAt(data.position);

            if (oldPiece != null)
            {
                Destroy(oldPiece.gameObject);
            }

            RuleManager.Instance.RemovePiece(data.position);
        }

        RuleManager.Instance.PlacePiece(pieza, data.position);

        pieza.SetBlockPrefab(GetBlockPrefab(data));
        pieza.Initialize();
    }

    // =========================
    // UTIL
    // =========================

    GameObject GetBlockPrefab(PieceSpawnData data)
    {
        if (data.customBlockPrefab != null)
            return data.customBlockPrefab;

        foreach (var floor in floorVisuals)
        {
            if (floor.yLevel == data.position.y)
                return floor.defaultBlockPrefab;
        }

        Debug.LogWarning($"No hay prefab asignado para Y={data.position.y}");
        return null;
    }

    // =========================
    // CLEANUP
    // =========================

    void ClearLevel()
    {
        Pieza[] existingPieces = FindObjectsOfType<Pieza>();

        foreach (var p in existingPieces)
        {
            Destroy(p.gameObject);
        }

        RuleManager.Instance.ClearGrid();
    }
}