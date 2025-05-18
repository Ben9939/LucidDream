using UnityEngine;

/// <summary>
/// FreeRoam_EventState は、自由移動状態のイベントサブステートを管理します。
/// イベント実行中の処理や状態遷移を実装します。
/// </summary>
public class FreeRoam_EventState : FreeRoam_SubStateBase
{
    /// <summary>
    /// コンストラクタ。イベントサブステートの初期化を行います。
    /// </summary>
    /// <param name="subStateManager">サブステートマネージャ</param>
    /// <param name="parentState">親となる FreeRoamState</param>
    public FreeRoam_EventState(FreeRoam_SubStateManager subStateManager, FreeRoamState parentState)
        : base(subStateManager, parentState)
    {
        CurrentState = FreeRoamSubState.Event;
    }

    public override void EnterState()
    {
        base.EnterState();
        Debug.Log("開始イベント状態");
    }

    public override void UpdateState()
    {
        base.UpdateState();
    }

    public override void ExitState()
    {
        base.ExitState();
        Debug.Log("終了イベント状態");
    }
}
