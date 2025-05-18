using System;
using UnityEngine;

/// <summary>
/// プレイヤーのデータを管理するScriptableObjectです。  
/// ・立ち絵の情報、現在のシーン境界、及びプレイヤーの位置情報を保持します。  
/// ・位置が変更された場合は、OnPositionChangedイベントを発火します。  
/// ・ResetDataで実行時状態を初期状態に戻します。
/// </summary>
[CreateAssetMenu(menuName = "Unit/PlayerData")]
public class PlayerDataSO : UnitDataSO, IPortraitHolder
{
    [Header("立ち絵資料")]
    public GameObject portraitPrefab;
    public GameObject PortraitPrefab => portraitPrefab;

    [Header("場景資料")]
    public SceneMapManager.SceneBoundaryName CurrentBoundary = SceneMapManager.SceneBoundaryName.None;

    // プライベート変数はcamelCase（アンダースコアを使用しない）
    [SerializeField] private Vector2 currentPosition;
    public event Action<Vector2> OnPositionChanged;

    /// <summary>
    /// プレイヤーの現在位置を取得または設定します。  
    /// 位置が変更された際にはOnPositionChangedイベントが発火します。
    /// </summary>
    public Vector2 CurrentPosition
    {
        get => currentPosition;
        set
        {
            if (currentPosition != value)
            {
                currentPosition = value;
                OnPositionChanged?.Invoke(currentPosition);
            }
        }
    }

    /// <summary>
    /// 実行時のプレイヤーデータを初期状態にリセットします。  
    /// 位置は原点に、シーン境界はNoneに戻されます。
    /// </summary>
    public override void ResetData()
    {
        base.ResetData();
        CurrentPosition = Vector2.zero;
        CurrentBoundary = SceneMapManager.SceneBoundaryName.None;
    }

    /// <summary>
    /// ゲーム終了時に呼び出され、データをリセットします。
    /// </summary>
    public void OnGameEnd()
    {
        ResetData();
    }
}

//using System;
//using UnityEngine;

//[CreateAssetMenu(menuName = "Unit/PlayerData")]
//public class PlayerDataSO : UnitDataSO, IPortraitHolder
//{
//    [Header("立繪資料")]
//    public GameObject portraitPrefab;
//    public GameObject PortraitPrefab => portraitPrefab;

//    [Header("場景資料")]
//    public SceneMapManager.SceneBoundaryName CurrentBoundary = SceneMapManager.SceneBoundaryName.None;

//    [SerializeField] private Vector2 _currentPosition;
//    public event Action<Vector2> OnPositionChanged;

//    public Vector2 CurrentPosition
//    {
//        get => _currentPosition;
//        set
//        {
//            if (_currentPosition != value)
//            {
//                _currentPosition = value;
//                OnPositionChanged?.Invoke(_currentPosition);
//            }
//        }
//    }

//    /// <summary>
//    /// 重置 PlayerData 的運行時狀態到初始狀態
//    /// </summary>
//    public override void ResetData()
//    {
//        base.ResetData();
//        CurrentPosition = Vector2.zero;
//        CurrentBoundary = SceneMapManager.SceneBoundaryName.None;
//    }
//    public void OnGameEnd()
//    {
//        ResetData();
//    }
//}
