using EasyTransition;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゲームの各状態を表す列挙型
/// </summary>
public enum GameState
{
    None,
    MainTitle,
    FreeRoam,
    Battle,
    Loading,
    Puzzle,
    Ending,
    Event,
    Intro,
    ReadQrCode,
    CreditScrolle
}

/// <summary>
/// 管理整個遊戲的狀態機，負責狀態切換和事件管理。
/// </summary>
public class GameStateManager : Singleton<GameStateManager>
{
    /// <summary>
    /// 現在の状態を保持する変数
    /// </summary>
    public GameStateBase currentState = null;

    /// <summary>
    /// ゲームリセットが必要な場合に true となるフラグ
    /// </summary>
    public static bool HaveToRestGame = false;

    // 前回の状態を記録する変数
    private GameState previousGameState;
    public GameState PreviousGameState { get { return previousGameState; } }

    // 各状態のインスタンスを保持する辞書
    private Dictionary<GameState, GameStateBase> states = new Dictionary<GameState, GameStateBase>();

    [SerializeField] private TransitionSettings transitionSetting;

    [Header("Debug Settings")]
    [SerializeField] private GameState DebugStartupState = GameState.MainTitle;
    [SerializeField] private List<EnterFreeRoamStateDialogEvent> enterFreeRoamStateDialogEventList;

    /// <summary>
    /// 現在の状態変更時に発火するイベント
    /// </summary>
    public event Action<GameStateBase> CurrentStateChanged;

    /// <summary>
    /// Loading 状態を保持する変数
    /// </summary>
    public LoadingState LoadingState = null;

    private void Start()
    {
        InitializeStates();
        if (!states.ContainsKey(DebugStartupState))
        {
            Debug.LogWarning("Invalid DebugStartupState specified. Switching to MainTitle.");
            DebugStartupState = GameState.MainTitle;
        }
        StartCoroutine(EnterState(DebugStartupState));
        ScenarioManager.Instance.InitializeScenario();
    }

    private void Update()
    {
        currentState?.UpdateState();
    }

    /// <summary>
    /// すべてのゲーム状態を初期化する。
    /// </summary>
    private void InitializeStates()
    {
        states[GameState.MainTitle] = new MainTitleState(this, "MainTitleScene", GameState.MainTitle);
        states[GameState.FreeRoam] = new FreeRoamState(this, "FreeRoamScene", GameState.FreeRoam, enterFreeRoamStateDialogEventList);
        states[GameState.Battle] = new BattleState(this, "BattleScene", GameState.Battle);
        states[GameState.Puzzle] = new PuzzleState(this, "PuzzleScene", GameState.Puzzle);
        states[GameState.ReadQrCode] = new ReadQRCodeState(this, "ReadQRCodeScene", GameState.ReadQrCode);
        states[GameState.Event] = new EventState(this, "EventScene", GameState.Event);
        states[GameState.CreditScrolle] = new CreditScrolleState(this, "CreditScrolleScene", GameState.CreditScrolle);
    }

    /// <summary>
    /// 指定した状態へ遷移するためのコルーチン
    /// </summary>
    /// <param name="newState">遷移先の状態</param>
    public IEnumerator EnterState(GameState newState)
    {
        if (!states.TryGetValue(newState, out GameStateBase newGameState))
        {
            Debug.LogError($"State {newState} does not exist!");
            yield return null;
        }

        previousGameState = currentState != null ? currentState.GameState : GameState.None;
        currentState = newGameState;
        // 新状態の完了イベントを購読
        currentState.OnStateFinished += HandleStateFinished;

        // 状態変更イベントを通知
        CurrentStateChanged?.Invoke(currentState);
        yield return null;
        currentState.EnterState();
    }

    /// <summary>
    /// 状態切り替えを実施する。
    /// </summary>
    /// <param name="newState">遷移先の状態</param>
    /// <param name="useTransition">過渡シーンを使用するか</param>
    public void SwitchState(GameState newState, bool useTransition = true)
    {
        GameStateBase targetState = null;

        // 現在の状態が LoadingState であり、かつターゲット状態と一致する場合は、対応する状態を直接使用
        if (currentState is LoadingState loadingState && loadingState.TargetState == newState)
        {
            Debug.Log(newState);
            if (!states.TryGetValue(newState, out targetState))
            {
                Debug.LogError($"State {newState} does not exist!");
                return;
            }
        }
        // 新状態が Loading を必要とする場合、新たに LoadingState を生成する
        else if (RequiresLoading(newState))
        {
            targetState = new LoadingState(this, "LoadingScene", newState, GameState.Loading);
            LoadingState = (LoadingState)targetState;
        }
        // それ以外は既存の辞書から取得
        else
        {
            if (!states.TryGetValue(newState, out targetState))
            {
                Debug.LogError($"State {newState} does not exist!");
                return;
            }
        }

        // 前回の状態を記録し、現在の状態を終了する
        previousGameState = currentState.GameState;
        currentState?.ExitState();
        currentState = targetState;
        currentState.OnStateFinished += HandleStateFinished;
        CurrentStateChanged?.Invoke(currentState);

        // シーン名が設定されている場合、シーン遷移を実施する
        if (!string.IsNullOrEmpty(currentState.SceneName))
        {
            if (useTransition)
                StartCoroutine(LoadSceneAsync(currentState));
            else
                StartCoroutine(LoadSceneWithoutTransition(currentState));
        }
        else
        {
            currentState.EnterState();
        }
    }

    /// <summary>
    /// Loading が必要な状態かどうかを判断する
    /// </summary>
    /// <param name="state">対象の状態</param>
    /// <returns>必要なら true、不要なら false</returns>
    private bool RequiresLoading(GameState state)
    {
        return state == GameState.Battle || state == GameState.Puzzle || state == GameState.ReadQrCode;
    }

    /// <summary>
    /// 状態完了イベントハンドラー
    /// </summary>
    /// <param name="nextState">次の状態</param>
    private void HandleStateFinished(GameState nextState)
    {
        SwitchState(nextState);
    }

    /// <summary>
    /// 過渡シーン付きでシーンを非同期に読み込み、状態の初期化を行う
    /// </summary>
    /// <param name="state">対象の状態</param>
    private IEnumerator LoadSceneAsync(GameStateBase state)
    {
        yield return StartCoroutine(
            TransitionManager.Instance().TransitionAndLoadSceneAsync(
                sceneName: state.SceneName,
                startDelay: 0,
                transitionSettings: transitionSetting,
                onSceneLoadedAndBeforeTransitionEnds: (Action)(() =>
                {
                    // シーン読み込み後、過渡処理終了前に状態初期化
                    state.EnterState();
                    Debug.Log($"[{state.SceneName}] 已經執行了 EnterState()");
                })
            )
        );
    }

    /// <summary>
    /// 過渡シーンを使用せずにシーンを非同期に読み込み、状態の初期化を行う
    /// </summary>
    /// <param name="state">対象の状態</param>
    private IEnumerator LoadSceneWithoutTransition(GameStateBase state)
    {
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(state.SceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        state.EnterState();
    }

    /// <summary>
    /// 現在の状態を取得するためのメソッド
    /// </summary>
    public GameStateBase GetCurrentState()
    {
        return currentState;
    }
}
