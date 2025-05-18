using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// GridManager クラスは、指定した境界内におけるグリッド座標の管理を行います。
/// グリッド上の空いている位置の取得や、中心部分の排除などを実装しています。
/// </summary>
public class GridManager
{
    private List<Vector3> gridPositions = new List<Vector3>();
    private HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();
    private float cellSize;
    private Boundary boundary;
    private float centerExclusionRange = 1.5f; // 中心排除範囲

    /// <summary>
    /// コンストラクタ。指定した境界とセルサイズに基づいてグリッド座標を生成します。
    /// </summary>
    /// <param name="boundary">グリッドの境界</param>
    /// <param name="cellSize">セルのサイズ</param>
    public GridManager(Boundary boundary, float cellSize)
    {
        this.boundary = boundary;
        this.cellSize = cellSize;
        // 境界内の全グリッド座標を生成
        for (float x = boundary.min.x; x <= boundary.max.x; x += cellSize)
        {
            for (float y = boundary.min.y; y <= boundary.max.y; y += cellSize)
            {
                gridPositions.Add(new Vector3(x, y, 0));
            }
        }
        // 中心部の座標を占有済みに設定
        MarkCenterOccupied();
    }

    private void MarkCenterOccupied()
    {
        // 境界の中心点を計算
        Vector3 center = new Vector3((boundary.min.x + boundary.max.x) / 2,
                                     (boundary.min.y + boundary.max.y) / 2, 0);
        // 中心排除範囲内の座標を占有済みに設定
        foreach (var pos in gridPositions)
        {
            if (Vector3.Distance(pos, center) <= centerExclusionRange)
            {
                occupiedPositions.Add(pos);
            }
        }
    }

    /// <summary>
    /// 利用可能なグリッド座標をランダムに取得します。
    /// </summary>
    /// <returns>利用可能な座標（見つからなければ null）</returns>
    public Vector3? GetAvailableGridPosition()
    {
        Vector3 center = new Vector3((boundary.min.x + boundary.max.x) / 2,
                                     (boundary.min.y + boundary.max.y) / 2, 0);
        List<Vector3> available = gridPositions.Where(pos =>
            !occupiedPositions.Contains(pos) &&
            Vector3.Distance(pos, center) > centerExclusionRange
        ).ToList();

        if (available.Count > 0)
            return available[Random.Range(0, available.Count)];
        return null;
    }

    /// <summary>
    /// 指定した座標を占有済みに設定します。
    /// </summary>
    /// <param name="pos">占有する座標</param>
    public void OccupyGridPosition(Vector3 pos)
    {
        occupiedPositions.Add(pos);
    }

    /// <summary>
    /// Vector2 型の座標を占有済みに設定します。
    /// </summary>
    /// <param name="pos">占有する座標</param>
    public void OccupyGridPosition(Vector2 pos)
    {
        OccupyGridPosition(new Vector3(pos.x, pos.y, 0));
    }

    /// <summary>
    /// グリッドの占有状態をリセットし、中心部の排除処理を再実行します。
    /// </summary>
    public void ClearGrid()
    {
        occupiedPositions.Clear();
        MarkCenterOccupied();
    }
}

/// <summary>
/// Boundary 構造体は、グリッドの境界を表現します。
/// </summary>
public struct Boundary
{
    public Vector2 min;
    public Vector2 max;

    public Boundary(Vector2 min, Vector2 max)
    {
        this.min = min;
        this.max = max;
    }
}
