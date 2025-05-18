using UnityEngine;

/// <summary>
/// FreeRoam_ExplorationState は、自由移動状態の探索サブステートを管理します。
/// 探索中の処理や状態更新を実装します。
/// </summary>
public class FreeRoam_ExplorationState : FreeRoam_SubStateBase
{
    /// <summary>
    /// コンストラクタ。探索サブステートの初期化を行います。
    /// </summary>
    /// <param name="subStateManager">サブステートマネージャ</param>
    /// <param name="parentState">親となる FreeRoamState</param>
    public FreeRoam_ExplorationState(FreeRoam_SubStateManager subStateManager, FreeRoamState parentState)
        : base(subStateManager, parentState)
    {
        CurrentState = FreeRoamSubState.Exploration;
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
