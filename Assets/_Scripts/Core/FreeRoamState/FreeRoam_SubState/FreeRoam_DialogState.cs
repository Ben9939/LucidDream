using UnityEngine;

/// <summary>
/// FreeRoam_DialogState は、自由移動状態の対話サブステートを管理します。
/// 対話中の処理や状態更新を実装します。
/// </summary>
public class FreeRoam_DialogState : FreeRoam_SubStateBase
{
    /// <summary>
    /// コンストラクタ。対話サブステートの初期化を行います。
    /// </summary>
    /// <param name="subStateManager">サブステートマネージャ</param>
    /// <param name="parentState">親となる FreeRoamState</param>
    public FreeRoam_DialogState(FreeRoam_SubStateManager subStateManager, FreeRoamState parentState)
        : base(subStateManager, parentState)
    {
        CurrentState = FreeRoamSubState.Dialog;
    }

    public override void EnterState()
    {
        base.EnterState();
    }

    public override void UpdateState()
    {
        base.UpdateState();
    }

    public override void ExitState()
    {
        base.ExitState();
    }
}
