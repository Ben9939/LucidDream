using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

/// <summary>
/// MazeSolver クラスは、画像から迷路を生成し、
/// A* アルゴリズムを用いてメイン経路とサブ経路（足跡ガイド）を計算、
/// 足跡の生成や経路の再計算、地図の切替処理、除錯用の Gizmos 表示などを行います。
/// </summary>
public class MazeSolver : MonoBehaviour
{
    #region 迷路・経路設定

    [Header("Maze & Path Settings")]
    [SerializeField] private List<Texture2D> inputImages;    // 迷宮画像のリスト
    [SerializeField] private int scale = 3;

    #endregion

    #region 足跡設定

    [Header("Footprint Settings")]
    [SerializeField] private Sprite leftFootSprite;          // 左足跡
    [SerializeField] private Sprite rightFootSprite;         // 右足跡
    [SerializeField] private GameObject footprintPrefab;     // 足跡の Prefab
    [SerializeField] private float footprintInterval = 0.1f;   // 足跡の生成間隔
    [SerializeField] private int maxVisibleFootprints = 10;    // 同時表示足跡の上限
    [SerializeField] private float footprintLifetime = 2f;     // 足跡が存在する時間（期限後にフェードアウト）

    #endregion

    #region 経路・指示設定

    [Header("Path & Guidance Settings")]
    [SerializeField] private float playerReachThreshold = 8f;   // プレイヤーがゴールに到達したと判定する閾値
    [SerializeField] private int maxSubPathPoints = 10;           // サブ経路として表示するノード数（制限付き）
    [SerializeField] private float pathRecastDistance = 5f;       // プレイヤーがメイン経路から離れた距離（再計算の閾値）

    #endregion

    #region その他パラメータ

    [Header("Other Parameters")]
    [SerializeField] private GameObject footprintsParent;         // 足跡の整理用親オブジェクト
    [SerializeField] private List<MazeTilemapRenderer> tilemapRenderers;  // Tilemap レンダラー群
    [SerializeField] private GameObject chestPrefab;              // ゴール到達時に配置するチェスト

    #endregion

    #region 内部状態

    private int[,] maze;              // 迷路データ
    private int mazeLength;           // 迷路のサイズ
    private Vector2 start;            // 迷路のスタート（値 2）
    private Vector2 goal;             // 迷路のゴール（値 3）

    private List<Vector2> mainPath;   // メイン経路（A* アルゴリズムによる全経路）
    private List<Vector2> subPath;    // サブ経路（足跡表示用の一部経路）

    private bool reachedGoal = false;
    private int currentMapIndex = 0;

    // 足跡生成用のカウンター
    private int footprintCount = 0;

    public static MazeSolver Instance { get; private set; }

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 1. 画像から迷路を生成し、世界座標に変換する
        maze = ImageToMaze.Instance.GenerateMaze(inputImages[currentMapIndex], scale);
        mazeLength = maze.GetLength(0);
        maze = MapMazeToWorldCoordinates(maze, mazeLength);

        // 2. 迷路内のスタートとゴールを特定する
        FindStartAndGoal(maze);
        Instantiate(chestPrefab, goal, Quaternion.identity);

        // 3. A* アルゴリズムでメイン経路を計算する
        mainPath = FindMainPath();
        if (mainPath == null || mainPath.Count == 0)
        {
            Debug.LogWarning("Main path could not be generated. Please check maze data.");
            return;
        }
        PrintMaze();

        // 4. 各 Tilemap レンダラーにより迷路を描画する
        foreach (var tilemapRenderer in tilemapRenderers)
        {
            tilemapRenderer.InitializeMaze(maze);
            tilemapRenderer.RenderMaze();
        }

        // 5. 足跡の親オブジェクトを生成する
        if (footprintsParent != null) Destroy(footprintsParent);
        footprintsParent = new GameObject("FootprintsParent");

        // 6. プレイヤーをメイン経路の最初の点に配置する
        PlayerController.Instance.SetPlayerPosition(mainPath[0]);

        // 7. サブ経路を初回更新する
        UpdateSubPath();

        // 8. 足跡生成のコルーチンを開始する
        StartCoroutine(GenerateFootprintsCoroutine());
    }

    private void OnEnable()
    {
        if (footprintsParent != null)
            Destroy(footprintsParent);
        footprintsParent = new GameObject("FootprintsParent");

        if (maze == null || mainPath == null || subPath == null)
        {
            Debug.LogWarning("MazeSolver is not properly initialized. Ensure Start() has executed.");
            return;
        }
    }

    private void OnDisable()
    {
        // すべてのコルーチンを停止し、足跡をクリアする
        StopAllCoroutines();
        if (footprintsParent != null)
        {
            Destroy(footprintsParent);
            footprintsParent = null;
        }
        reachedGoal = false;
        Debug.Log("All footprints have been reset.");
    }

    #endregion

    #region 迷路・経路計算

    /// <summary>
    /// 迷路配列を世界座標にマッピングします。
    /// </summary>
    /// <param name="maze">元の迷路配列</param>
    /// <param name="mazeSize">迷路のサイズ</param>
    /// <returns>世界座標にマッピングされた迷路配列</returns>
    int[,] MapMazeToWorldCoordinates(int[,] maze, int mazeSize)
    {
        int[,] worldMaze = new int[maze.GetLength(0), maze.GetLength(1)];
        for (int y = maze.GetLength(1) - 1; y >= 0; y--)
        {
            for (int x = 0; x < maze.GetLength(0); x++)
            {
                int cellType = maze[y, x];
                int worldX = x;
                int worldY = mazeSize - 1 - y;
                int mappedX = Mathf.FloorToInt(worldX);
                int mappedY = Mathf.FloorToInt(worldY);
                worldMaze[mappedY, mappedX] = cellType;
            }
        }
        return worldMaze;
    }

    /// <summary>
    /// 迷路内のスタート（2）とゴール（3）を検出します。
    /// </summary>
    /// <param name="maze">迷路配列</param>
    void FindStartAndGoal(int[,] maze)
    {
        for (int y = 0; y < maze.GetLength(0); y++)
        {
            for (int x = 0; x < maze.GetLength(1); x++)
            {
                if (maze[y, x] == 2)
                    start = new Vector2(x, y);
                if (maze[y, x] == 3)
                    goal = new Vector2(x, y);
            }
        }
    }

    /// <summary>
    /// A* アルゴリズムを使用してメイン経路を計算します（スタートからゴールまで）。
    /// </summary>
    /// <returns>計算された経路のリスト</returns>
    List<Vector2> FindMainPath()
    {
        var openList = new PriorityQueue<Vector2>();
        var cameFrom = new Dictionary<Vector2, Vector2>();
        var gScore = new Dictionary<Vector2, int>();
        var fScore = new Dictionary<Vector2, int>();

        openList.Enqueue(start, 0);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, goal);

        while (openList.Count > 0)
        {
            Vector2 current = openList.Dequeue();
            if (current == goal)
                return ReconstructPath(cameFrom, current);

            foreach (var neighbor in GetNeighbors(current))
            {
                int tentativeGScore = gScore[current] + 1;
                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, goal);
                    if (!openList.Contains(neighbor))
                        openList.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 指定された位置からゴールまでの経路を A* アルゴリズムで計算します。
    /// </summary>
    /// <param name="startPos">開始位置</param>
    /// <returns>計算された経路のリスト</returns>
    List<Vector2> FindMainPathFromPosition(Vector2 startPos)
    {
        var openList = new PriorityQueue<Vector2>();
        var cameFrom = new Dictionary<Vector2, Vector2>();
        var gScore = new Dictionary<Vector2, int>();
        var fScore = new Dictionary<Vector2, int>();

        openList.Enqueue(startPos, 0);
        gScore[startPos] = 0;
        fScore[startPos] = Heuristic(startPos, goal);

        while (openList.Count > 0)
        {
            Vector2 current = openList.Dequeue();
            if (current == goal)
                return ReconstructPath(cameFrom, current);

            foreach (var neighbor in GetNeighbors(current))
            {
                int tentativeGScore = gScore[current] + 1;
                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, goal);
                    if (!openList.Contains(neighbor))
                        openList.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 経路を再構築します。
    /// </summary>
    /// <param name="cameFrom">各ノードの親情報</param>
    /// <param name="current">ゴールノード</param>
    /// <returns>再構築された経路</returns>
    List<Vector2> ReconstructPath(Dictionary<Vector2, Vector2> cameFrom, Vector2 current)
    {
        var path = new List<Vector2> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();
        return path;
    }

    /// <summary>
    /// マンハッタン距離をヒューリスティック値として返します。
    /// </summary>
    int Heuristic(Vector2 a, Vector2 b)
    {
        return (int)(Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y));
    }

    /// <summary>
    /// 指定されたノードの上下左右の隣接ノードを取得します（歩行可能なノードのみ）。
    /// </summary>
    List<Vector2> GetNeighbors(Vector2 node)
    {
        List<Vector2> neighbors = new List<Vector2>();
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        foreach (var dir in directions)
        {
            Vector2 neighbor = node + dir;
            if (IsWalkable(neighbor))
                neighbors.Add(neighbor);
        }
        return neighbors;
    }

    /// <summary>
    /// 指定された座標が歩行可能かどうかを判定します。
    /// 周囲8方向に壁（1 または 4）がある場合は不可とします。
    /// </summary>
    bool IsWalkable(Vector2 pos)
    {
        if (pos.x < 0 || pos.y < 0 || pos.y >= maze.GetLength(0) || pos.x >= maze.GetLength(1))
            return false;

        if (maze[(int)pos.y, (int)pos.x] == 1 || maze[(int)pos.y, (int)pos.x] == 4)
            return false;

        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int nx = (int)pos.x + dx;
                int ny = (int)pos.y + dy;
                if (nx < 0 || ny < 0 || ny >= maze.GetLength(0) || nx >= maze.GetLength(1))
                    continue;
                if (maze[ny, nx] == 1 || maze[ny, nx] == 4)
                    return false;
            }
        }
        return true;
    }

    #endregion

    #region サブ経路（足跡ガイド）の計算

    /// <summary>
    /// メイン経路上からプレイヤーに最も近い点を探し、そこから最大 maxSubPathPoints 個のノードをサブ経路として更新します。
    /// その後、簡単な平滑化処理を行います。
    /// </summary>
    void UpdateSubPath()
    {
        Vector2 playerPos = PlayerController.Instance.GetPlayerPosition();
        Vector2 closest = FindClosestPathPoint(mainPath, playerPos);
        int startIndex = mainPath.IndexOf(closest);

        subPath = new List<Vector2>();
        int count = Mathf.Min(maxSubPathPoints, mainPath.Count - startIndex);
        for (int i = 0; i < count; i++)
        {
            subPath.Add(mainPath[startIndex + i]);
        }
        subPath = SmoothPath(subPath);
    }

    /// <summary>
    /// メイン経路からプレイヤーに最も近い点を探します。
    /// </summary>
    Vector2 FindClosestPathPoint(List<Vector2> path, Vector2 playerPos)
    {
        float minDist = float.MaxValue;
        Vector2 closest = path[0];
        foreach (var p in path)
        {
            float d = Vector2.Distance(playerPos, p);
            if (d < minDist)
            {
                minDist = d;
                closest = p;
            }
        }
        return closest;
    }

    /// <summary>
    /// サブ経路を簡単に平滑化します。
    /// 隣接ノード間の角度が急であれば、中間点を両端の中点に調整します。
    /// </summary>
    List<Vector2> SmoothPath(List<Vector2> rawPath)
    {
        List<Vector2> smoothed = new List<Vector2>(rawPath);
        for (int i = 1; i < smoothed.Count - 1; i++)
        {
            Vector2 prev = smoothed[i - 1];
            Vector2 curr = smoothed[i];
            Vector2 next = smoothed[i + 1];

            Vector2 v1 = (curr - prev).normalized;
            Vector2 v2 = (next - curr).normalized;
            if (Mathf.Abs(Vector2.Dot(v1, v2)) < 0.1f)
            {
                smoothed[i] = (prev + next) / 2f;
            }
        }
        return smoothed;
    }

    #endregion

    #region 足跡生成

    /// <summary>
    /// 足跡生成のコルーチンです。
    /// サブ経路に沿って足跡を生成し、一定数に達した場合は足跡が全て消滅するまで待機します。
    /// また、プレイヤーがゴールに到達していれば、次の地図に切り替えます。
    /// </summary>
    IEnumerator GenerateFootprintsCoroutine()
    {
        while (!reachedGoal)
        {
            UpdateSubPath();

            if (CheckIfPlayerReachedGoal())
            {
                LoadNextMap();
                yield break;
            }

            if (subPath == null || subPath.Count < 2)
            {
                yield return null;
                continue;
            }

            List<GameObject> currentRoundFootprints = new List<GameObject>();

            for (int i = 0; i < subPath.Count - 1; i++)
            {
                if (currentRoundFootprints.Count >= maxVisibleFootprints)
                {
                    yield return StartCoroutine(WaitUntilFootprintsDisappear(currentRoundFootprints));
                    currentRoundFootprints.Clear();
                }

                GameObject fp = GenerateFootprint(subPath[i], subPath[i + 1]);
                currentRoundFootprints.Add(fp);
                yield return new WaitForSeconds(footprintInterval);
            }

            if (currentRoundFootprints.Count > 0)
            {
                yield return StartCoroutine(WaitUntilFootprintsDisappear(currentRoundFootprints));
            }

            Vector2 playerPos = PlayerController.Instance.GetPlayerPosition();
            bool needRecalc = true;
            foreach (var p in mainPath)
            {
                if (Vector2.Distance(playerPos, p) <= pathRecastDistance)
                {
                    needRecalc = false;
                    break;
                }
            }
            if (needRecalc)
            {
                var newPath = FindMainPathFromPosition(playerPos);
                if (newPath != null && newPath.Count > 0)
                {
                    mainPath = newPath;
                    Debug.Log("Main path recalculated from player's position.");
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// 指定された2点間に足跡を生成し、フェードインおよび遅延フェードアウトの処理を開始します。
    /// </summary>
    /// <param name="position">生成位置</param>
    /// <param name="nextPosition">次の位置（回転計算用）</param>
    /// <returns>生成された足跡の GameObject</returns>
    GameObject GenerateFootprint(Vector2 position, Vector2? nextPosition = null)
    {
        Vector2 spawnPos = position + new Vector2(0.5f, 0.5f);
        GameObject fp = Instantiate(footprintPrefab, spawnPos, Quaternion.identity);
        if (footprintsParent == null)
        {
            Debug.LogError("FootprintsParent is null. Check initialization.");
            return fp;
        }
        fp.transform.SetParent(footprintsParent.transform);

        SpriteRenderer sr = fp.GetComponent<SpriteRenderer>();
        sr.sprite = (footprintCount % 2 == 0) ? leftFootSprite : rightFootSprite;
        footprintCount++;

        if (nextPosition.HasValue)
        {
            Vector2 dir = (nextPosition.Value - position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            fp.transform.rotation = Quaternion.Euler(0, 0, angle + 270);
        }

        StartCoroutine(FadeIn(fp));
        StartCoroutine(DelayedFadeOut(fp));
        return fp;
    }

    /// <summary>
    /// 指定された足跡リスト内の全足跡が消滅するまで待機します。
    /// </summary>
    /// <param name="footprints">足跡のリスト</param>
    IEnumerator WaitUntilFootprintsDisappear(List<GameObject> footprints)
    {
        bool allGone = false;
        while (!allGone)
        {
            allGone = true;
            foreach (var fp in footprints)
            {
                if (fp != null)
                {
                    allGone = false;
                    break;
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// 足跡のフェードイン効果を実行します。
    /// </summary>
    IEnumerator FadeIn(GameObject fp)
    {
        SpriteRenderer sr = fp.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;
        Color c = sr.color;
        c.a = 0;
        sr.color = c;

        float duration = 0.5f;
        float elapsed = 0;
        while (elapsed < duration)
        {
            if (fp == null) yield break;
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / duration);
            sr.color = c;
            yield return null;
        }
        c.a = 1;
        sr.color = c;
    }

    /// <summary>
    /// 指定された足跡の生成後、一定時間待ってからフェードアウトおよび消滅させます。
    /// </summary>
    IEnumerator DelayedFadeOut(GameObject fp)
    {
        yield return new WaitForSeconds(footprintLifetime);
        StartCoroutine(FadeOutAndDestroy(fp));
    }

    /// <summary>
    /// 足跡のフェードアウト効果を実行し、最終的に足跡オブジェクトを破棄します。
    /// </summary>
    IEnumerator FadeOutAndDestroy(GameObject fp)
    {
        if (fp == null) yield break;
        SpriteRenderer sr = fp.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;
        Color c = sr.color;
        float duration = 0.5f;
        float elapsed = 0;
        while (elapsed < duration)
        {
            if (fp == null) yield break;
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(1 - (elapsed / duration));
            sr.color = c;
            yield return null;
        }
        Destroy(fp);
    }

    #endregion

    #region 除錯および地図切替

    /// <summary>
    /// プレイヤーがゴールに到達したかどうかを判定します。
    /// </summary>
    /// <returns>到達していれば true</returns>
    bool CheckIfPlayerReachedGoal()
    {
        Vector2 playerPos = PlayerController.Instance.GetPlayerPosition();
        if (Vector2.Distance(playerPos, goal) <= playerReachThreshold)
        {
            reachedGoal = true;
            Debug.Log("Player reached the goal!");
            return true;
        }
        return false;
    }

    /// <summary>
    /// 次の地図に切り替えます。
    /// 画像から新たな迷路を生成し、スタート地点・経路・足跡生成を再初期化します。
    /// </summary>
    private void LoadNextMap()
    {
        currentMapIndex++;
        if (currentMapIndex >= inputImages.Count)
        {
            Debug.Log("All maps have been completed!");
            return;
        }
        Debug.Log($"Switching to map {currentMapIndex}");

        maze = ImageToMaze.Instance.GenerateMaze(inputImages[currentMapIndex], scale);
        maze = MapMazeToWorldCoordinates(maze, mazeLength);
        mazeLength = maze.GetLength(0);

        FindStartAndGoal(maze);
        mainPath = FindMainPath();
        reachedGoal = false;

        StopAllCoroutines();
        StartCoroutine(GenerateFootprintsCoroutine());
    }

    /// <summary>
    /// 迷路をコンソールに出力します（除錯用）。
    /// </summary>
    void PrintMaze()
    {
        string mazeString = "";
        for (int y = 0; y < maze.GetLength(1); y++)
        {
            for (int x = 0; x < maze.GetLength(1); x++)
            {
                mazeString += maze[y, x] + " ";
            }
            mazeString += "\n";
        }
        Debug.Log(mazeString);
    }

    #endregion

    #region Gizmos 除錯可視化

    private void OnDrawGizmos()
    {
        // メイン経路を赤い線で描画
        if (mainPath != null && mainPath.Count > 1)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < mainPath.Count - 1; i++)
            {
                Vector3 p1 = new Vector3(mainPath[i].x + 0.5f, mainPath[i].y + 0.5f, 0);
                Vector3 p2 = new Vector3(mainPath[i + 1].x + 0.5f, mainPath[i + 1].y + 0.5f, 0);
                Gizmos.DrawLine(p1, p2);
            }
        }

        // サブ経路を青い球体で描画
        if (subPath != null && subPath.Count > 0)
        {
            Gizmos.color = Color.blue;
            foreach (var p in subPath)
            {
                Vector3 pos = new Vector3(p.x + 0.5f, p.y + 0.5f, 0);
                Gizmos.DrawSphere(pos, 0.2f);
            }
        }
    }

    #endregion
}

/// <summary>
/// PriorityQueue クラスは、A* アルゴリズム用にプライオリティ付きキューを実装します。
/// </summary>
/// <typeparam name="T">キューの要素の型</typeparam>
public class PriorityQueue<T>
{
    private List<(T item, int priority)> elements = new List<(T, int)>();

    /// <summary>
    /// キュー内の要素数を返します。
    /// </summary>
    public int Count => elements.Count;

    /// <summary>
    /// 要素とそのプライオリティをキューに追加します。
    /// </summary>
    public void Enqueue(T item, int priority)
    {
        elements.Add((item, priority));
        elements.Sort((x, y) => x.priority.CompareTo(y.priority));
    }

    /// <summary>
    /// キューから最もプライオリティの高い要素を取り出します。
    /// </summary>
    public T Dequeue()
    {
        var item = elements[0];
        elements.RemoveAt(0);
        return item.item;
    }

    /// <summary>
    /// キュー内に指定した要素が存在するかを返します。
    /// </summary>
    public bool Contains(T item)
    {
        return elements.Exists(x => EqualityComparer<T>.Default.Equals(x.item, item));
    }
}
