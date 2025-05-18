using System.Collections;
using UnityEngine;

/// <summary>
/// CircleMovement クラスは、指定した中心点を基準に図形の頂点周辺を円運動させる処理を実装します。
/// また、一定間隔でボタン等の点滅処理を行います。
/// </summary>
public class CircleMovement : MonoBehaviour
{
    public GameObject[] images;        // 円運動するオブジェクト群
    public GameObject centerImages;      // 中心となるオブジェクト
    public float sideLength;             // 三角形の辺の長さ
    public float blinkInterval;          // 点滅間隔

    private Vector2 centerPoint;         // 中心座標
    private Vector2[] vertices = new Vector2[3]; // 三角形の頂点座標
    private float[] rotationSpeeds;      // 各オブジェクトの回転速度

    void Start()
    {
        centerPoint = centerImages.transform.position;
        InitializeRotationSpeeds();
        SetInitialAngles();
        CalculateTriangleVertices();
        StartCoroutine(BlinkStartButton());
    }

    private void Update()
    {
        // 各オブジェクトが対応する頂点を中心に円運動を行う
        for (int i = 0; i < images.Length; i++)
        {
            Vector2 center = vertices[i];
            float angle = Time.time * rotationSpeeds[i];
            float x = center.x + sideLength / 2f * Mathf.Cos(angle);
            float y = center.y + sideLength / 2f * Mathf.Sin(angle);
            images[i].transform.position = new Vector3(x, y, 0f);
        }
    }

    private IEnumerator BlinkStartButton()
    {
        while (true)
        {
            // （例）ボタンの表示切替処理をここに実装
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    /// <summary>
    /// 中心点を基準に三角形の頂点座標を計算します。
    /// </summary>
    void CalculateTriangleVertices()
    {
        vertices = new Vector2[3];
        // １つ目の頂点は中心から右に辺の半分だけずらした位置
        vertices[0] = new Vector2(centerPoint.x + sideLength / 2f, centerPoint.y);

        // ２つ目の頂点（120度）
        float angleRad = Mathf.PI * 2f / 3f;
        vertices[1] = new Vector2(centerPoint.x + sideLength / 2f * Mathf.Cos(angleRad),
                                   centerPoint.y + sideLength / 2f * Mathf.Sin(angleRad));

        // ３つ目の頂点（240度）
        angleRad *= 2f;
        vertices[2] = new Vector2(centerPoint.x + sideLength / 2f * Mathf.Cos(angleRad),
                                   centerPoint.y + sideLength / 2f * Mathf.Sin(angleRad));
    }

    /// <summary>
    /// 各オブジェクトの回転速度をランダムに初期化します。
    /// </summary>
    void InitializeRotationSpeeds()
    {
        rotationSpeeds = new float[images.Length];
        for (int i = 0; i < images.Length; i++)
        {
            rotationSpeeds[i] = Random.Range(-2.5f, 2.5f);
        }
    }

    /// <summary>
    /// 各オブジェクトの初期位置を設定します。
    /// </summary>
    void SetInitialAngles()
    {
        for (int i = 0; i < images.Length; i++)
        {
            Vector2 center = vertices[i];
            float initialAngle = GetInitialAngle(i);
            float x = center.x + sideLength / 2f * Mathf.Cos(initialAngle);
            float y = center.y + sideLength / 2f * Mathf.Sin(initialAngle);
            images[i].transform.position = new Vector3(x, y, 0f);
        }
    }

    /// <summary>
    /// インデックスに応じた初期角度（ラジアン）を取得します。
    /// </summary>
    /// <param name="index">オブジェクトのインデックス</param>
    /// <returns>初期角度（ラジアン）</returns>
    float GetInitialAngle(int index)
    {
        switch (index)
        {
            case 0:
                return Mathf.Deg2Rad * 195f;
            case 1:
                return Mathf.Deg2Rad * 345f;
            case 2:
                return Mathf.Deg2Rad * 90f;
            default:
                return 0f;
        }
    }
}