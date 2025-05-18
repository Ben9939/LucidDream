using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FreeRoam_ExplorationController は、自由移動状態の探索サブステートを管理します。
/// 探索中のプレイヤー操作やその他関連コントローラーの更新処理を実行します。
/// </summary>
public class FreeRoam_ExplorationController : FreeRoam_ControllerBase
{
    /// <summary>
    /// 管理対象のサブステートは Exploration です。
    /// </summary>
    protected override FreeRoamSubState ControlledSubState => FreeRoamSubState.Exploration;

    /// <summary>
    /// 探索状態に入るときの処理（必要に応じて実装）。
    /// </summary>
    protected override void OnSubStateEnter()
    {
        // 探索状態への入場処理を実装可能
    }

    /// <summary>
    /// 探索状態中の更新処理。プレイヤー、NPC、オブジェクト等の更新を行います。
    /// </summary>
    protected override void OnSubStateUpdate()
    {
        PlayerController.Instance.HandleUpdate();
        ButtonWithTooltip.Instance.HandleUpdate();
        NPCController.UpdateAllNPCs();
        ObjectController.UpdateAllObjects();
    }

    /// <summary>
    /// 探索状態から出るときの処理（必要に応じて実装）。
    /// </summary>
    protected override void OnSubStateExit()
    {
        // 探索状態からの退出処理を実装可能
    }

    /// <summary>
    /// メニューへの切り替えを要求するハンドラ。
    /// </summary>
    public void HandleMenuOnClick()
    {
        currentSubState.RequestSubStateChange(FreeRoamSubState.Menu);
    }
}