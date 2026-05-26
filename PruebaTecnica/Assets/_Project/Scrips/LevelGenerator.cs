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
    public List<PieceSpawnData> pieces = new List<PieceSpawnData>();

    [Header("Generation Settings")]
    public int maxAttempts = 50;

    void Start()
    {
        GenerateLevel();
    }

    // =========================
    // MAIN GENERATION FLOW
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
        foreach (var data in pieces)
        {
            SpawnPiece(data);
        }
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

        // 1. Colocar en grid
        RuleManager.Instance.PlacePiece(pieza, data.position);

        // 2. Visual
        pieza.SetBlockPrefab(data.blockPrefab);

        // 3. Dirección aleatoria (IMPORTANTE: antes de simular)
        pieza.Initialize();
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
    }
}