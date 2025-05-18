using UnityEngine;

/// <summary>
/// FreeRoam_MenuState は、自由移動状態のメニューサブステートを管理します。
/// メニュー画面の表示や操作に関する処理を実装します。
/// </summary>
public class FreeRoam_MenuState : FreeRoam_SubStateBase
{
    /// <summary>
    /// コンストラクタ。メニューサブステートの初期化を行います。
    /// </summary>
    /// <param name="subStateManager">サブステートマネージャ</param>
    /// <param name="parentState">親となる FreeRoamState</param>
    public FreeRoam_MenuState(FreeRoam_SubStateManager subStateManager, FreeRoamState parentState)
        : base(subStateManager, parentState)
    {
        CurrentState = FreeRoamSubState.Menu;
    }

    public override void EnterState()
    {
        base.EnterState();
        Debug.Log("Enter MenuState");
    }

    public override void UpdateState()
    {
        base.UpdateState();
    }

    public override void ExitState()
    {
        base.ExitState();
        Debug.Log("Exit MenuState");
    }
}