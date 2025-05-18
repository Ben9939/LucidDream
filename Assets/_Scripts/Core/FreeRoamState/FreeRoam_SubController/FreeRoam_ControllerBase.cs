using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// FreeRoam_ControllerBase は、自由移動状態における各サブステートコントローラーの基底クラスです。
/// 各コントローラーはこのクラスを継承し、サブステートのイベント購読と通知を管理します。
/// </summary>
public abstract class FreeRoam_ControllerBase : MonoBehaviour
{
    protected FreeRoam_SubStateBase currentSubState;
    protected abstract FreeRoamSubState ControlledSubState { get; }

    /// <summary>
    /// サブステートの初期化とイベント購読を行います。
    /// </summary>
    protected virtual IEnumerator Start()
    {
        yield return new WaitUntil(() => GameStateManager.Instance.GetCurrentState() is FreeRoamState);
        var freeRoamState = GameStateManager.Instance.GetCurrentState() as FreeRoamState;
        var subStateManager = freeRoamState.SubStateManager;

        currentSubState = subStateManager.GetSubState(ControlledSubState);
        SubscribeToSubState(currentSubState);
    }

    /// <summary>
    /// サブステート変更時のハンドラー。
    /// </summary>
    /// <param name="newSubState">新しいサブステート</param>
    private void HandleSubStateChanged(FreeRoam_SubStateBase newSubState)
    {
        if (currentSubState != null)
        {
            UnsubscribeFromSubState(currentSubState);
        }
        currentSubState = newSubState;
        SubscribeToSubState(currentSubState);
    }

    /// <summary>
    /// 指定したサブステートのイベントに購読します。
    /// </summary>
    /// <param name="subState">購読対象のサブステート</param>
    private void SubscribeToSubState(FreeRoam_SubStateBase subState)
    {
        if (subState == null) return;
        string callerName = this.GetType().Name;
        Debug.Log($"[Subscribe] {callerName} subscribes to events of {subState}.");
        subState.OnSubStateEnter += OnSubStateEnter;
        subState.OnSubStateUpdate += OnSubStateUpdate;
        subState.OnSubStateExit += OnSubStateExit;
    }

    /// <summary>
    /// 指定したサブステートのイベントから購読を解除します。
    /// </summary>
    /// <param name="subState">解除対象のサブステート</param>
    private void UnsubscribeFromSubState(FreeRoam_SubStateBase subState)
    {
        if (subState == null) return;
        subState.OnSubStateEnter -= OnSubStateEnter;
        subState.OnSubStateUpdate -= OnSubStateUpdate;
        subState.OnSubStateExit -= OnSubStateExit;
    }

    protected virtual void OnDestroy()
    {
        if (currentSubState != null)
        {
            UnsubscribeFromSubState(currentSubState);
        }
    }

    protected abstract void OnSubStateEnter();
    protected abstract void OnSubStateUpdate();
    protected abstract void OnSubStateExit();
}
