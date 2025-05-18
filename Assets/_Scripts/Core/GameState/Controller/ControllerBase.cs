using UnityEngine;

/// <summary>
/// ゲーム状態のイベントにサブスクライブし、状態遷移を管理する基底クラス。
/// </summary>
public abstract class ControllerBase : MonoBehaviour
{
    [Header("Credit Data")]
    [SerializeField]
    private string creditInfo;

    // 現在のゲーム状態を保持する変数
    private GameStateBase currentState;

    /// <summary>
    /// 初期化処理
    /// GameStateManagerから現在の状態を取得し、イベントにサブスクライブする。
    /// </summary>
    private void Start()
    {
        // GameStateManagerから現在の状態を取得する
        currentState = GameStateManager.Instance.GetCurrentState();

        // ゲーム状態開始のデバッグログ出力
        Debug.Log($"{currentState.SceneName} GameStateBase started");

        // 取得した状態に対してイベントのサブスクライブを行う
        SubscribeToState(currentState);
    }

    /// <summary>
    /// コンポーネント破棄時の処理
    /// イベントからのサブスクライブ解除を実施する。
    /// </summary>
    private void OnDestroy()
    {
        if (currentState != null)
        {
            UnsubscribeFromState(currentState);
            Debug.Log($"{currentState.SceneName} GameStateBase ended");
        }
    }

    /// <summary>
    /// 指定されたゲーム状態のイベントにサブスクライブする。
    /// </summary>
    /// <param name="state">サブスクライブ対象のゲーム状態</param>
    public virtual void SubscribeToState(GameStateBase state)
    {
        string callerName = GetType().Name;
        Debug.Log($"[Subscribe] {callerName} subscribed to {state.SceneName} GameStateBase events.");

        // 各イベントに対して処理を登録
        state.OnStateEnter += Initialize;
        state.OnStateUpdate += OnUpdate;
        state.OnStateExit += Cleanup;
    }

    /// <summary>
    /// 指定されたゲーム状態のイベントからサブスクライブ解除する。
    /// </summary>
    /// <param name="state">サブスクライブ解除対象のゲーム状態</param>
    public virtual void UnsubscribeFromState(GameStateBase state)
    {
        Debug.Log($"[Unsubscribe] Unsubscribed from {state.SceneName} GameStateBase events.");

        // 各イベントから処理を解除
        state.OnStateEnter -= Initialize;
        state.OnStateUpdate -= OnUpdate;
        state.OnStateExit -= Cleanup;
    }

    /// <summary>
    /// ゲーム状態開始時の初期化処理
    /// サブクラスで必要に応じてオーバーライドする。
    /// </summary>
    protected virtual void Initialize() { }

    /// <summary>
    /// ゲーム状態更新時の処理
    /// サブクラスで必要に応じてオーバーライドする。
    /// </summary>
    protected virtual void OnUpdate() { }

    /// <summary>
    /// ゲーム状態終了時のクリーンアップ処理
    /// サブクラスで必要に応じてオーバーライドする。
    /// </summary>
    protected virtual void Cleanup() { }

    /// <summary>
    /// 現在の状態から次の状態への遷移を要求する。
    /// </summary>
    /// <param name="nextState">遷移先のゲーム状態</param>
    protected void SwitchState(GameState nextState)
    {
        if (currentState != null)
        {
            currentState.RequestStateChange(nextState);
        }
    }

}
