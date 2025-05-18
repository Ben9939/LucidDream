using System;
using UnityEngine;

/// <summary>
/// FreeRoam_SubStateBase は、自由移動状態におけるサブステートの基底クラスです。
/// 各サブステートはこのクラスを継承し、独自の処理を実装します。
/// </summary>
public abstract class FreeRoam_SubStateBase
{
    protected FreeRoam_SubStateManager subStateManager;
    protected FreeRoamState parentState;
    public event Action<FreeRoamSubState> OnSubStateFinished;
    public event Action OnSubStateEnter;
    public event Action OnSubStateUpdate;
    public event Action OnSubStateExit;
    public FreeRoamSubState CurrentState { get; protected set; }

    /// <summary>
    /// コンストラクタ。サブステートマネージャと親ステートを初期化します。
    /// </summary>
    /// <param name="subStateManager">サブステートマネージャ</param>
    /// <param name="parentState">親となる FreeRoamState</param>
    public FreeRoam_SubStateBase(FreeRoam_SubStateManager subStateManager, FreeRoamState parentState)
    {
        this.subStateManager = subStateManager;
        this.parentState = parentState;
    }

    /// <summary>
    /// サブステートに入る処理を実行します。
    /// </summary>
    public virtual void EnterState()
    {
        OnSubStateEnter?.Invoke();
    }

    /// <summary>
    /// サブステートの更新処理を実行します。
    /// </summary>
    public virtual void UpdateState()
    {
        OnSubStateUpdate?.Invoke();
    }

    /// <summary>
    /// サブステートから出る処理を実行します。
    /// </summary>
    public virtual void ExitState()
    {
        OnSubStateExit?.Invoke();
    }

    /// <summary>
    /// サブステートの変更を要求します。
    /// </summary>
    /// <param name="nextState">次に遷移するサブステート</param>
    public void RequestSubStateChange(FreeRoamSubState nextState)
    {
        OnSubStateFinished?.Invoke(nextState);
    }
}
