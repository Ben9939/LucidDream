using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// MazeTilemapRenderer クラスは、迷宮データに基づいて Tilemap 上にタイルを描画します。
/// 指定した TileType に応じて、Tilemap にタイルをセットし、オブジェクトの表示・非表示を制御します。
/// </summary>
public class MazeTilemapRenderer : MonoBehaviour
{
    public static MazeTilemapRenderer Instance { get; private set; }

    public enum TileType
    {
        WallTile,      // Wall
        BoundTile1,    // Boundary Type 1
        BoundTile2,    // Boundary Type 2
        BoundTile3,    // Boundary Type 3
        SceneObject    // Scene Object
    }

    [Header("Tilemap Settings")]
    [SerializeField] private Tilemap tilemap;    // Reference to the Tilemap
    [SerializeField] private Tile tile;          // General-purpose Tile
    [SerializeField] private TileType tileType;  // Current Tile type
    [SerializeField] private bool objectsVisible = true; // Visibility of scene objects

    private Dictionary<int, List<Vector2Int>> tilePositions = new Dictionary<int, List<Vector2Int>>(); // 各タイル種別の座標を保持
    private bool isInitialized = false; // 初期化済みかどうか

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // シーン切替時に破棄しない場合は有効化
            Debug.Log("[MazeTilemapRenderer] Singleton instance created.");
        }
        else
        {
            //Destroy(gameObject);
            Debug.LogWarning("[MazeTilemapRenderer] Another instance detected and destroyed.");
        }
    }

    private void OnDisable()
    {
        isInitialized = false;
        Debug.LogWarning("[MazeTilemapRenderer] Renderer disabled. Resetting initialization state.");
    }

    /// <summary>
    /// Maze 配列のデータを元に、タイルの座標を記録します（初回のみ実行）。
    /// </summary>
    /// <param name="maze">迷宮データ配列</param>
    public void InitializeMaze(int[,] maze)
    {
        if (isInitialized)
        {
            Debug.LogWarning("[MazeTilemapRenderer] Already initialized. Skipping initialization.");
            return;
        }

        if (maze == null)
        {
            Debug.LogError("[MazeTilemapRenderer] Maze data is null. Initialization aborted.");
            return;
        }

        tilePositions.Clear();
        int rows = maze.GetLength(0);
        int cols = maze.GetLength(1);
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int cellType = maze[y, x];
                if (!tilePositions.ContainsKey(cellType))
                {
                    tilePositions[cellType] = new List<Vector2Int>();
                    Debug.Log($"[MazeTilemapRenderer] Detected new tile type: {cellType}");
                }
                tilePositions[cellType].Add(new Vector2Int(x, y));
            }
        }
        isInitialized = true;
        Debug.Log("[MazeTilemapRenderer] Maze initialization complete!");
    }

    /// <summary>
    /// 指定された TileType に基づき、タイルを描画します。
    /// </summary>
    public void RenderMaze()
    {
        if (!isInitialized)
        {
            Debug.LogError("[MazeTilemapRenderer] Not initialized. Call InitializeMaze() first!");
            return;
        }

        Debug.Log($"[MazeTilemapRenderer] Rendering maze for TileType: {tileType}");

        switch (tileType)
        {
            case TileType.WallTile:
                RenderTiles(1);
                break;
            case TileType.BoundTile1:
                RenderTiles(2);
                break;
            case TileType.BoundTile2:
                RenderTiles(4);
                break;
            case TileType.BoundTile3:
                RenderTiles(5);
                break;
            case TileType.SceneObject:
                RenderTiles(6);
                break;
            default:
                Debug.LogWarning($"[MazeTilemapRenderer] Unknown TileType: {tileType}. No tiles rendered.");
                break;
        }
    }

    /// <summary>
    /// 指定された cellType に対応するタイルを描画し、オブジェクトの表示状態を反映します。
    /// </summary>
    /// <param name="cellType">描画するタイルのタイプ</param>
    private void RenderTiles(int cellType)
    {
        if (!tilePositions.ContainsKey(cellType))
        {
            Debug.LogWarning($"[MazeTilemapRenderer] Tile type {cellType} has no recorded positions. Skipping rendering.");
            return;
        }

        Debug.Log($"[MazeTilemapRenderer] Rendering {tilePositions[cellType].Count} tiles for type {cellType}.");

        foreach (var position in tilePositions[cellType])
        {
            Vector3Int cellPosition = new Vector3Int(position.x, position.y, 0);
            tilemap.SetTile(cellPosition, this.tile);
            tilemap.SetTileFlags(cellPosition, TileFlags.None); // Unlock tile flags to modify color
            tilemap.SetColor(cellPosition, objectsVisible ? Color.white : new Color(1, 1, 1, 0));
        }
    }
}
