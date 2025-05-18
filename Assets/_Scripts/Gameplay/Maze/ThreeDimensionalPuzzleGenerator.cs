using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// ThreeDimensionalPuzzleGenerator クラスは、2D の通路情報から 3D パズルを生成し、
/// Cube と障害物をシーン上に可視化します。
/// </summary>
public class ThreeDimensionalPuzzleGenerator : MonoBehaviour
{
    #region Public Fields

    [Header("Grid Settings")]
    public int GridSize = 5;
    public Vector2Int StartPoint = new Vector2Int(0, 0);
    public Vector2Int EndPoint = new Vector2Int(4, 4);

    [Header("Initialization Settings")]
    [Tooltip("Inspector で指定したカスタム 2D マトリックスを使用するか。false の場合、自動生成されます。")]
    public bool UseCustomMatrix = false;
    [Tooltip("カスタムマトリックス。各行は GridSize 個の数字。最初の文字列が最上行に対応します。")]
    public string[] CustomMatrixRows;

    [Header("Visualization Settings")]
    public GameObject CubePrefab;
    [Tooltip("Cube 間の間隔")]
    public float CubeSpacing = 1f;

    [Header("Obstacle Settings")]
    public GameObject ObstaclePrefab;
    [Tooltip("障害物の親コンテナ。指定がなければ自動生成されます。")]
    public Transform ObstacleContainer;

    #endregion

    #region Private Fields

    // 0～(GridSize-1) を使う 3D 配列
    private int[,,] puzzle3D;
    // 0～(GridSize-1) を使う 2D 配列
    private int[,] path2D;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        // 配列の初期化
        puzzle3D = new int[GridSize, GridSize, GridSize];
        path2D = new int[GridSize, GridSize];

        // 障害物コンテナが指定されていなければ自動生成
        if (ObstacleContainer == null)
        {
            GameObject container = new GameObject("ObstacleContainer");
            ObstacleContainer = container.transform;
        }

        GeneratePuzzle();
        VisualizePuzzle();
        VisualizeObstacles();

        PrintPuzzle3D();
    }

    #endregion

    #region Puzzle Generation

    /// <summary>
    /// パズルを生成します。  
    /// カスタムマトリックスが有効ならそれを使用し、そうでなければ自動生成した 2D 通路情報を 3D に変換します。
    /// </summary>
    public void GeneratePuzzle()
    {
        if (UseCustomMatrix && CustomMatrixRows != null && CustomMatrixRows.Length == GridSize)
        {
            // CustomMatrixRows[0] が最上行に対応しているため、y=0 (下) には CustomMatrixRows[GridSize-1]、y=GridSize-1 (上) には CustomMatrixRows[0] を割り当てる
            for (int y = 0; y < GridSize; y++)
            {
                string rowStr = CustomMatrixRows[GridSize - 1 - y];
                string[] tokens = rowStr.Split(new char[] { ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length != GridSize)
                {
                    Debug.LogError($"カスタムマトリックスの {GridSize - y} 行目の数字の数が不正です（{GridSize} 個必要）。");
                    continue;
                }
                for (int x = 0; x < GridSize; x++)
                {
                    if (int.TryParse(tokens[x], out int val))
                        path2D[x, y] = val;
                    else
                    {
                        Debug.LogError($"カスタムマトリックスの {GridSize - y} 行目 {x + 1} 番目の数字が解析できません。");
                        path2D[x, y] = 0;
                    }
                }
            }
        }
        else
        {
            path2D = Generate2DPath();
        }
        ConvertTo3D(path2D);
    }

    /// <summary>
    /// 自動生成した 2D 通路パスを作成します（インデックスは 0～GridSize-1）。
    /// 単純なランダムウォークでパスを生成します。
    /// </summary>
    private int[,] Generate2DPath()
    {
        int[,] grid = new int[GridSize, GridSize];
        List<Vector2Int> path = new List<Vector2Int>();

        path.Add(StartPoint);
        grid[StartPoint.x, StartPoint.y] = 1;

        while (path.Count > 0 && path[path.Count - 1] != EndPoint)
        {
            Vector2Int current = path[path.Count - 1];
            List<Vector2Int> neighbors = GetValidNeighbors(current, grid);
            if (neighbors.Count > 0)
            {
                Vector2Int next = neighbors[Random.Range(0, neighbors.Count)];
                path.Add(next);
                grid[next.x, next.y] = 1;
            }
            else
            {
                path.RemoveAt(path.Count - 1);
            }
        }
        return grid;
    }

    /// <summary>
    /// 現在位置の有効な隣接セルを取得します。
    /// </summary>
    private List<Vector2Int> GetValidNeighbors(Vector2Int current, int[,] grid)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] directions = {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbor = current + dir;
            if (neighbor.x >= 0 && neighbor.x < GridSize &&
                neighbor.y >= 0 && neighbor.y < GridSize &&
                grid[neighbor.x, neighbor.y] == 0 &&
                CountNeighborPaths(neighbor, grid) <= 1)
            {
                neighbors.Add(neighbor);
            }
        }
        return neighbors;
    }

    /// <summary>
    /// 指定位置の隣接セルに存在する通路の数をカウントします。
    /// </summary>
    private int CountNeighborPaths(Vector2Int pos, int[,] grid)
    {
        int count = 0;
        Vector2Int[] directions = {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbor = pos + dir;
            if (neighbor.x >= 0 && neighbor.x < GridSize &&
                neighbor.y >= 0 && neighbor.y < GridSize &&
                grid[neighbor.x, neighbor.y] == 1)
                count++;
        }
        return count;
    }

    /// <summary>
    /// 2D パス情報を 3D パズルに変換します。  
    /// 各 (x, y) セルで値が 1 の場合、ランダムな z 値に対応する位置を 1 に設定します。  
    /// その後、各 z 層に必ず 1 つ以上の Cube が存在するよう補強します。
    /// </summary>
    private void ConvertTo3D(int[,] path2D)
    {
        // まず 3D 配列をクリア
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                for (int z = 0; z < GridSize; z++)
                {
                    puzzle3D[x, y, z] = 0;
                }
            }
        }

        // 通路セル (x, y) をリストアップ
        List<Vector2Int> pathCells = new List<Vector2Int>();
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                if (path2D[x, y] == 1)
                {
                    pathCells.Add(new Vector2Int(x, y));
                }
            }
        }

        // 各通路セルについて、ランダムな z 値を選択して 1 に設定
        foreach (Vector2Int cell in pathCells)
        {
            int z = Random.Range(0, GridSize);
            puzzle3D[cell.x, cell.y, z] = 1;
        }

        // 各 z 層に Cube が存在するか確認し、なければ補強
        bool[] layerHasCube = new bool[GridSize];
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                for (int z = 0; z < GridSize; z++)
                {
                    if (puzzle3D[x, y, z] == 1)
                    {
                        layerHasCube[z] = true;
                    }
                }
            }
        }
        for (int z = 0; z < GridSize; z++)
        {
            if (!layerHasCube[z])
            {
                if (pathCells.Count > 0)
                {
                    Vector2Int chosen = pathCells[Random.Range(0, pathCells.Count)];
                    puzzle3D[chosen.x, chosen.y, z] = 1;
                    layerHasCube[z] = true;
                }
                else
                {
                    Debug.LogWarning($"Unable to reinforce layer z = {z} because no path cells are available.");
                }
            }
        }
    }

    #endregion

    #region Visualization

    /// <summary>
    /// 3D パズルの Cube をシーン上に生成します。  
    /// ロジック中心 (center, center, center) がローカル原点 (0,0,0) になるよう配置します。
    /// </summary>
    public void VisualizePuzzle()
    {
        // 既存の Cube をすべて削除
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        int center = GridSize / 2; // 例：GridSize=5 => center=2（0-based）
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                for (int z = 0; z < GridSize; z++)
                {
                    if (puzzle3D[x, y, z] == 1)
                    {
                        Vector3 localPos = new Vector3(x - center, y - center, z - center) * CubeSpacing;
                        GameObject cube = Instantiate(CubePrefab, transform);
                        cube.transform.localPosition = localPos;

                        MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
                        if (renderer != null)
                        {
                            // Set renderQueue for proper rendering order
                            renderer.material.renderQueue = 2000;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 障害物を生成します。  
    /// パズル内の Cube の位置を世界座標に投影し、空いているセルに障害物を配置します。
    /// </summary>
    public void VisualizeObstacles()
    {
        // 既存の障害物を削除
        for (int i = ObstacleContainer.childCount - 1; i >= 0; i--)
            Destroy(ObstacleContainer.GetChild(i).gameObject);

        // パズル全体の中心を pivot（transform.position の XY 部分）とする
        Vector2 pivot = new Vector2(transform.position.x, transform.position.y);
        int centerIndex = GridSize / 2; // 0-based center

        bool[,] occupied = new bool[GridSize, GridSize];

        // Cube の世界位置を調べ、対応するセルを占有済みに設定
        foreach (Transform cube in transform)
        {
            Vector3 worldPos = cube.position;
            Vector2 proj = new Vector2(worldPos.x, worldPos.y);
            Vector2 delta = proj - pivot;
            int cellX = Mathf.RoundToInt(delta.x / CubeSpacing) + centerIndex;
            int cellY = Mathf.RoundToInt(delta.y / CubeSpacing) + centerIndex;
            if (cellX >= 0 && cellX < GridSize && cellY >= 0 && cellY < GridSize)
            {
                occupied[cellX, cellY] = true;
            }
        }

        // 未占有のセルに障害物を生成
        for (int i = 0; i < GridSize; i++)
        {
            for (int j = 0; j < GridSize; j++)
            {
                if (!occupied[i, j])
                {
                    Vector2 cellDelta = new Vector2(i - centerIndex, j - centerIndex) * CubeSpacing;
                    Vector3 obstaclePos = new Vector3(pivot.x + cellDelta.x, pivot.y + cellDelta.y, transform.position.z);
                    Instantiate(ObstaclePrefab, obstaclePos, Quaternion.identity, ObstacleContainer);
                }
            }
        }
    }

    /// <summary>
    /// 生成した 3D パズルの内容をコンソールに出力します。
    /// </summary>
    public void PrintPuzzle3D()
    {
        Debug.Log($"Generated 3D Puzzle (index [0..{GridSize - 1}]):");
        for (int z = 0; z < GridSize; z++)
        {
            Debug.Log($"Layer Z = {z}:");
            for (int y = GridSize - 1; y >= 0; y--)
            {
                string row = "";
                for (int x = 0; x < GridSize; x++)
                {
                    row += (puzzle3D[x, y, z] == 1 ? "1 " : "0 ");
                }
                Debug.Log(row);
            }
        }
    }

    #endregion
}

