using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HandTracking クラスは、UDPReceive から取得した JSON データを解析し、
/// 手のランドマークから手の状態や手の位置を算出する処理を行います。
/// また、平滑化処理を用いて手の位置の更新を行い、handInteractor オブジェクトの位置を更新します。
/// </summary>
public class HandTracking : MonoBehaviour
{
    // UDPReceive コンポーネント（データ受信用）
    public UDPReceive udpReceive;
    // 手の位置を可視化するオブジェクト
    public GameObject handInteractor;
    // 平滑化された手の中心位置
    public Vector2 smoothedPosition;
    // 手が開いているか閉じているかの状態（true: open, false: closed）
    public bool handState;

    // 手のランドマークデータを表すクラス
    [System.Serializable]
    public class Landmark
    {
        // 各ランドマークの座標 [x, y, z]
        public float[] coordinates;
    }

    // 手のデータ全体を表すクラス
    [System.Serializable]
    public class HandData
    {
        // 手のランドマークのリスト
        public List<Landmark> landmarks;
    }

    /// <summary>
    /// 手のトラッキングの更新処理を実行します。
    /// UDPReceive から受信した JSON データを解析し、手のランドマークから
    /// 掌の中心位置の算出、手の開閉状態の判定、手の位置更新を行います。
    /// </summary>
    public void HandleHandTrackingUpdate()
    {
        // UDPReceive から JSON データを取得
        string data = udpReceive.data;

        // JSON データの解析
        HandData handData;
        try
        {
            handData = JsonUtility.FromJson<HandData>(data);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing JSON: {ex.Message}");
            return;
        }

        if (handData == null || handData.landmarks == null || handData.landmarks.Count == 0)
        {
            return;
        }

        // ランドマークデータを Vector2 のリストに変換
        List<Vector2> landmarks = ConvertLandmarksToVector2(handData.landmarks);
        // 掌中心位置を算出
        Vector2 palmCenter = CalculatePalmCenter(landmarks);
        // 手の開閉状態を判定
        handState = DetectHandOpen(landmarks);
        // 手の位置を更新
        UpdateHandPosition(palmCenter);
    }

    /// <summary>
    /// ランドマークのリストを Vector2 のリストに変換します。
    /// </summary>
    /// <param name="landmarks">ランドマークのリスト</param>
    /// <returns>変換後の Vector2 リスト</returns>
    private List<Vector2> ConvertLandmarksToVector2(List<Landmark> landmarks)
    {
        List<Vector2> vectors = new List<Vector2>();
        foreach (var lm in landmarks)
        {
            // 座標が2つ以上あることを確認
            if (lm.coordinates.Length >= 2)
            {
                float x = lm.coordinates[0];
                float y = lm.coordinates[1];
                vectors.Add(new Vector2(x, y));
            }
        }
        return vectors;
    }

    /// <summary>
    /// ランドマークから掌中心位置を算出します。
    /// landmark[0]（手首）と landmark[9]（中指 MCP）の中点を利用します。
    /// </summary>
    /// <param name="landmarks">Vector2 に変換済みのランドマークリスト</param>
    /// <returns>掌中心位置</returns>
    private Vector2 CalculatePalmCenter(List<Vector2> landmarks)
    {
        if (landmarks.Count < 10)
        {
            Debug.LogError("Landmarks count is insufficient to calculate palm center.");
            return Vector2.zero;
        }
        return (landmarks[0] + landmarks[9]) / 2;
    }

    /// <summary>
    /// ランドマークから手が開いているか閉じているかを判定します。
    /// 指先と掌中心との距離に基づき、開閉状態を判定します。
    /// </summary>
    /// <param name="landmarks">Vector2 に変換済みのランドマークリスト</param>
    /// <returns>手が開いている場合 true、そうでなければ false</returns>
    private bool DetectHandOpen(List<Vector2> landmarks)
    {
        if (landmarks.Count < 21) // landmarks に 21 個の点があることを確認
        {
            Debug.LogError("Landmarks count is insufficient to detect hand state.");
            return false;
        }

        // 指先のインデックス（拇指:4、食指:8、中指:12、無名指:16、小指:20）
        Vector2 thumbTip = landmarks[4];
        Vector2 indexTip = landmarks[8];
        Vector2 middleTip = landmarks[12];
        Vector2 ringTip = landmarks[16];
        Vector2 pinkyTip = landmarks[20];

        // 掌中心（landmark[0] と landmark[9] の中点）
        Vector2 palmCenter = (landmarks[0] + landmarks[9]) / 2;

        // 各指先と掌中心の距離を算出
        float thumbToPalm = Vector2.Distance(thumbTip, palmCenter);
        float indexToPalm = Vector2.Distance(indexTip, palmCenter);
        float middleToPalm = Vector2.Distance(middleTip, palmCenter);
        float ringToPalm = Vector2.Distance(ringTip, palmCenter);
        float pinkyToPalm = Vector2.Distance(pinkyTip, palmCenter);

        // 閾値の設定（調整が必要）
        float openThreshold = 50f;
        float closedThreshold = 30f;

        // 手が開いているか判定（全指先の距離が openThreshold を超える場合）
        bool isHandOpen = thumbToPalm > openThreshold &&
                          indexToPalm > openThreshold &&
                          middleToPalm > openThreshold &&
                          ringToPalm > openThreshold &&
                          pinkyToPalm > openThreshold;
        // 手が閉じているか判定（全指先の距離が closedThreshold 未満の場合）
        bool isHandClosed = thumbToPalm < closedThreshold &&
                            indexToPalm < closedThreshold &&
                            middleToPalm < closedThreshold &&
                            ringToPalm < closedThreshold &&
                            pinkyToPalm < closedThreshold;

        return isHandOpen && !isHandClosed;
    }

    /// <summary>
    /// 掌中心位置に基づき、手の位置を更新します。
    /// 画面座標を Unity のワールド座標にマッピングし、平滑化処理を実施します。
    /// </summary>
    /// <param name="palmCenter">掌中心位置</param>
    private void UpdateHandPosition(Vector2 palmCenter)
    {
        // 画面座標から Unity ワールド座標へのマッピング（シーンに合わせて調整）
        float mappedX = (7 - (palmCenter.x / 1280) * 14);
        float mappedY = (5 - (palmCenter.y / 720) * 10);
        Vector2 newPosition = new Vector2(mappedX, mappedY);

        // 平滑化処理
        smoothedPosition = GetSmoothedPosition(newPosition);

        // handInteractor の位置を更新
        handInteractor.transform.localPosition = smoothedPosition;
    }

    // 平滑化のための位置履歴
    private Queue<Vector2> positionHistory = new Queue<Vector2>();
    private int historySize = 10; // 履歴に保持するフレーム数
    private Vector2 previousSmoothedPosition;

    /// <summary>
    /// 新たな位置と過去の履歴から、平滑化された位置を算出します。
    /// </summary>
    /// <param name="newPosition">新たな位置</param>
    /// <returns>平滑化された位置</returns>
    private Vector2 GetSmoothedPosition(Vector2 newPosition)
    {
        // 新位置を履歴に追加
        positionHistory.Enqueue(newPosition);
        // 履歴サイズを維持
        if (positionHistory.Count > historySize)
        {
            positionHistory.Dequeue();
        }
        // 平均値を算出
        Vector2 sum = Vector2.zero;
        foreach (var pos in positionHistory)
        {
            sum += pos;
        }
        Vector2 averagePosition = sum / positionHistory.Count;
        // 前回の平滑化位置と平均値の補間でさらなる平滑化
        float interpolationFactor = 0.5f;
        Vector2 smoothedWithInterpolation = Vector2.Lerp(previousSmoothedPosition, averagePosition, interpolationFactor);
        previousSmoothedPosition = smoothedWithInterpolation;
        return smoothedWithInterpolation;
    }
}
