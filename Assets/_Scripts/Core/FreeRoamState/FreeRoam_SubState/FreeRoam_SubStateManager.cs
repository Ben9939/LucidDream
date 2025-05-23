using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FreeRoam_SubStateManager クラスは、自由移動状態におけるサブステートの管理を行います。
/// 各サブステート（探索、対話、イベント、メニュー、カットイン）の切り替えや更新を管理します。
/// </summary>
public enum FreeRoamSubState { Exploration, Dialog, Event, Menu, CutIn }
public class FreeRoam_SubStateManager
{
    private FreeRoam_SubStateBase currentSubState;
    private Dictionary<FreeRoamSubState, FreeRoam_SubStateBase> subStates = new Dictionary<FreeRoamSubState, FreeRoam_SubStateBase>();

    /// <summary>
    /// コンストラクタ。親となる FreeRoamState を元に各サブステートを初期化します。
    /// </summary>
    /// <param name="parentState">親となる FreeRoamState</param>
    public FreeRoam_SubStateManager(FreeRoamState parentState)
    {
        // 必要に応じて Event や CutIn も初期化してください
        subStates[FreeRoamSubState.Exploration] = new FreeRoam_ExplorationState(this, parentState);
        subStates[FreeRoamSubState.Menu] = new FreeRoam_MenuState(this, parentState);
        subStates[FreeRoamSubState.Dialog] = new FreeRoam_DialogState(this, parentState);
        // 例: subStates[FreeRoamSubState.Event] = new FreeRoam_EventState(this, parentState);
        currentSubState = subStates[FreeRoamSubState.Exploration];
    }

    /// <summary>
    /// 指定したサブステートに切り替えます。
    /// </summary>
    /// <param name="newSubState">切り替えるサブステート</param>
    public void EnterSubState(FreeRoamSubState newSubState)
    {
        if (!subStates.TryGetValue(newSubState, out FreeRoam_SubStateBase newState))
        {
            Debug.LogError($"SubState {newSubState} does not exist!");
            return;
        }
        currentSubState = newState;
        currentSubState.OnSubStateFinished += SwitchSubState;
        currentSubState.EnterState();
    }

    /// <summary>
    /// サブステートを切り替えます。
    /// </summary>
    /// <param name="newSubState">切り替え先のサブステート</param>
    public void SwitchSubState(FreeRoamSubState newSubState)
    {
        if (!subStates.TryGetValue(newSubState, out FreeRoam_SubStateBase newState))
        {
            Debug.LogError($"SubState {newSubState} does not exist!");
            return;
        }

        if (currentSubState != null)
        {
            currentSubState.OnSubStateFinished -= SwitchSubState;
            currentSubState.ExitState();
        }
        currentSubState = newState;
        Debug.Log("Switch SubState to " + newState.ToString());
        currentSubState.OnSubStateFinished += SwitchSubState;
        currentSubState.EnterState();
    }

    /// <summary>
    /// 現在のサブステートの更新処理を実行します。
    /// </summary>
    public void UpdateSubState()
    {
        currentSubState?.UpdateState();
    }

    /// <summary>
    /// 現在のサブステートを終了します。
    /// </summary>
    public void ExitSubState()
    {
        if (currentSubState != null)
        {
            currentSubState.OnSubStateFinished -= SwitchSubState;
            currentSubState.ExitState();
            currentSubState = null; // 現在のサブステートをクリアする
        }
    }

    /// <summary>
    /// 現在のサブステートを返します。
    /// </summary>
    /// <returns>現在のサブステート</returns>
    public FreeRoam_SubStateBase GetCurrentSubState()
    {
        return currentSubState;
    }

    /// <summary>
    /// 指定されたサブステートを返します。
    /// </summary>
    /// <param name="targetSubState">取得するサブステート</param>
    /// <returns>指定されたサブステート</returns>
    public FreeRoam_SubStateBase GetSubState(FreeRoamSubState targetSubState)
    {
        return subStates[targetSubState];
    }
}
