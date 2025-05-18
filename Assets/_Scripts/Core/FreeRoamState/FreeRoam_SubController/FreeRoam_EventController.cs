using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FreeRoam_EventController は、自由移動状態のイベントサブステートを管理します。
/// イベント発生時の状態遷移に伴う処理を実装します。
/// </summary>
public class FreeRoam_EventController : FreeRoam_ControllerBase
{
    /// <summary>
    /// 管理対象のサブステートは Event です。
    /// </summary>
    protected override FreeRoamSubState ControlledSubState => FreeRoamSubState.Event;

    /// <summary>
    /// イベント状態に入るときの処理。開始ログを出力します。
    /// </summary>
    protected override void OnSubStateEnter()
    {
        Debug.Log($"{gameObject.name} - Entering {currentSubState.GetType().Name} state.");
    }

    /// <summary>
    /// イベント状態中の更新処理。更新ログを出力します。
    /// </summary>
    protected override void OnSubStateUpdate()
    {
        Debug.Log($"{gameObject.name} - Updating {currentSubState.GetType().Name} state.");
    }

    /// <summary>
    /// イベント状態から出るときの処理。終了ログを出力します。
    /// </summary>
    protected override void OnSubStateExit()
    {
        Debug.Log($"{gameObject.name} - Exiting {currentSubState.GetType().Name} state.");
    }
}
