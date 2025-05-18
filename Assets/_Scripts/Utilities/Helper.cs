using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Helper クラスは、UI 操作、待機処理、キャンバス要素の位置取得など、共通のユーティリティ機能を提供します。
/// </summary>
public static class Helper
{
    /// <summary>
    /// 各方向ベクトルを提供するリスト
    /// </summary>
    public static List<Vector3> Directions { get; private set; } = new List<Vector3>
    {
        new Vector3(1, 0, 0),   // 右
        new Vector3(-1, 0, 0),  // 左
        new Vector3(0, 1, 0),   // 上
        new Vector3(0, -1, 0),  // 下
        new Vector3(1, 1, 0),   // 右上
        new Vector3(-1, 1, 0),  // 左上
        new Vector3(1, -1, 0),  // 右下
        new Vector3(-1, -1, 0)  // 左下
    };

    private static Dictionary<float, WaitForSeconds> waitDictionary = new Dictionary<float, WaitForSeconds>();

    // プライベート変数は camelCase を使用
    private static Camera camera;

    /// <summary>
    /// メインカメラを取得します。
    /// </summary>
    public static Camera Camera
    {
        get
        {
            if (camera == null)
                camera = Camera.main;
            return camera;
        }
    }

    /// <summary>
    /// 指定した待機時間の WaitForSeconds を返します。
    /// 同じ時間の待機オブジェクトはキャッシュされ再利用されます。
    /// </summary>
    /// <param name="time">待機時間（秒）</param>
    /// <returns>WaitForSeconds オブジェクト</returns>
    public static WaitForSeconds GetWait(float time)
    {
        if (waitDictionary.TryGetValue(time, out var wait))
            return wait;
        waitDictionary[time] = new WaitForSeconds(time);
        return waitDictionary[time];
    }

    private static PointerEventData eventDataCurrentPosition;
    private static List<RaycastResult> results;

    /// <summary>
    /// 現在のマウス位置が UI 上にあるか判定します。
    /// </summary>
    /// <returns>UI 上に存在する場合は true</returns>
    public static bool IsOverUi()
    {
        eventDataCurrentPosition = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    /// <summary>
    /// 現在のマウス位置に指定した型 T のコンポーネントを持つ UI が存在するか判定します。
    /// </summary>
    /// <typeparam name="T">チェック対象のコンポーネント型</typeparam>
    /// <returns>存在する場合は true</returns>
    public static bool IsOverUi<T>() where T : Component
    {
        eventDataCurrentPosition = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        foreach (var result in results)
        {
            if (result.gameObject.GetComponent<T>() != null)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 現在のマウス位置に指定された GameObject があるか判定します。
    /// </summary>
    /// <param name="targetObject">チェック対象の GameObject</param>
    /// <returns>存在する場合は true</returns>
    public static bool IsOverUi(GameObject targetObject)
    {
        eventDataCurrentPosition = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        foreach (var result in results)
        {
            if (result.gameObject == targetObject)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// RectTransform 要素のワールド座標を取得します。
    /// </summary>
    /// <param name="element">対象の RectTransform</param>
    /// <returns>ワールド座標</returns>
    public static Vector2 GetWorldPositionOfCanvasElement(RectTransform element)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(element, element.position, Camera, out var result);
        return result;
    }

    /// <summary>
    /// 指定された Transform の全子オブジェクトを削除します。
    /// </summary>
    /// <param name="t">対象の Transform</param>
    public static void DeleteChildren(this Transform t)
    {
        foreach (Transform child in t)
            UnityEngine.Object.Destroy(child.gameObject);
    }
}
