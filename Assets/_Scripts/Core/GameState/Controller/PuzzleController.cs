using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PuzzleController は、パズルモードにおける各種初期化、パズルの配置、
/// パズルの正しい配置のチェック、ならびにパズル完成時の状態遷移などを管理するコントローラです。
/// 主に ItemManager からパズルプレハブを取得し、メインパネルのサイズに合わせてグリッド状に配置します。
/// また、すべてのパズルが正しく配置された場合に、ワークベンチのアニメーションを再生し、
/// 自由移動モード (FreeRoam) に遷移します。
/// </summary>
public class PuzzleController : ControllerBase
{
    #region Serialized Fields

    [Header("Serialized Fields - Canvas Objects")]
    [SerializeField] private PuzzleModeHandInteractionController handInteractor;
    [SerializeField] private GameObject handTrackingManager;
    [SerializeField] private GameObject mainPanel;          // メインパネル（必ず RectTransform を持つ）
    [SerializeField] private Animator workbenchAnimator;      // ワークベンチのアニメーション
    [SerializeField] private GameObject puzzleListParent;     // パズルのインスタンス化後の親オブジェクト
    [Header("ActiveScenarioState")]
    [SerializeField] private ScenarioStateSO activeScenarioState;
    [Header("OwnedItems")]
    [SerializeField] private OwnedItemsSO ownedItems;
    #endregion

    #region Private Fields

    // ItemManager から取得するパズルプレハブのリスト
    private List<GameObject> puzzleList;
    // インスタンス化したパズルのリスト
    private List<GameObject> instantiatedPuzzles;

    public static PuzzleController Instance;          // シングルトン参照
    public event Action FreeRoamSwitch;                 // 自由移動モードへ切り替えるイベント

    #endregion

    #region MonoBehaviour Methods

    /// <summary>
    /// 初期化処理。パズルに関する各種初期化処理を行います。
    /// ItemManager からパズルプレハブを取得し、メインパネルのサイズに合わせてグリッド状にパズルを配置します。
    /// また、PuzzleModeHandInteractionController へインスタンス化したパズルリストを渡します。
    /// </summary>
    protected override void Initialize()
    {
        Instance = this;

        // Puzzle の配置完了イベントに購読（すべてのパズルが配置されたら角度チェックを行う）
        if (handInteractor != null)
        {
            handInteractor.isAllPuzzlePlaced += CheckAllPuzzleAngle;
        }
        else
        {
            Debug.LogError("handInteractor が指定されていません！");
            return;
        }

        // ItemManager からパズルプレハブのリストを取得
        puzzleList = ownedItems.GetPuzzlePrefabs();
        if (puzzleList == null || puzzleList.Count == 0)
        {
            Debug.LogError("パズルリストが空または null です。ItemManager を確認してください。");
            return;
        }
        Debug.Log($"パズルリストの取得に成功：数 = {puzzleList.Count}");

        // インスタンス化したパズルのリストを初期化
        instantiatedPuzzles = new List<GameObject>();

        // メインパネルの RectTransform を取得し、サイズを確認
        RectTransform panelRectTransform = mainPanel?.GetComponent<RectTransform>();
        if (panelRectTransform == null)
        {
            Debug.LogError("mainPanel が指定されていないか、RectTransform がありません！");
            return;
        }
        float panelWidth = panelRectTransform.rect.width;
        float panelHeight = panelRectTransform.rect.height;
        Debug.Log($"mainPanel のサイズ：幅 = {panelWidth}, 高さ = {panelHeight}");
        if (panelWidth <= 0 || panelHeight <= 0)
        {
            Debug.LogError("mainPanel のサイズが無効です（幅または高さが 0 です）。");
            return;
        }

        // パズルの位置をグリッド状に配置し、パズルをインスタンス化する
        ArrangePuzzlesInGrid(panelWidth, panelHeight, puzzleList.Count);

        // インスタンス化したパズルリストを handInteractor に渡す
        handInteractor.PuzzleInstance(instantiatedPuzzles);
        AudioManager.Instance.PlayBGM(BGMSoundData.BGM.Puzzle);
    }

    /// <summary>
    /// 毎フレームの更新処理です。手部インタラクションと手部トラッキングの更新を行います。
    /// </summary>
    protected override async void OnUpdate()
    {
        // 手部インタラクションの更新
        if (handInteractor != null)
        {
            handInteractor.ModeUpdate();
        }

        // 手部トラッキングマネージャの更新
        if (handTrackingManager != null)
        {
            HandTracking handTrackingComponent = handTrackingManager.GetComponent<HandTracking>();
            if (handTrackingComponent != null)
            {
                handTrackingComponent.HandleHandTrackingUpdate();
            }
        }
        // 非同期メソッドとしての待機処理
        await Task.Yield();
    }

    /// <summary>
    /// クリーンアップ処理です。状態を終了する際に Python スクリプトを停止します。
    /// </summary>
    protected override void Cleanup()
    {
        PythonProcessManager.Instance.StopPythonScript();
        AudioManager.Instance.StopBGM();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// メインパネルのサイズに基づき、グリッド形式でパズルを配置しインスタンス化します。
    /// </summary>
    /// <param name="panelWidth">メインパネルの幅</param>
    /// <param name="panelHeight">メインパネルの高さ</param>
    /// <param name="puzzleCount">パズルの総数</param>
    private void ArrangePuzzlesInGrid(float panelWidth, float panelHeight, int puzzleCount)
    {
        // グリッドの行数と列数を計算
        int columns = Mathf.CeilToInt(Mathf.Sqrt(puzzleCount));
        int rows = Mathf.CeilToInt((float)puzzleCount);
        float cellWidth = panelWidth / columns;
        float cellHeight = panelHeight / rows;
        float offsetX = cellWidth / 2;
        float offsetY = cellHeight / 2;

        Debug.Log($"グリッドパラメータ - 列: {columns}, 行: {rows}, セル幅: {cellWidth}, セル高さ: {cellHeight}");

        for (int i = 0; i < puzzleList.Count; i++)
        {
            GameObject puzzlePrefab = puzzleList[i];
            if (puzzlePrefab == null)
            {
                Debug.LogWarning($"パズルプレハブ {i} が null です。スキップします...");
                continue;
            }
            Debug.Log($"パズル {i} をインスタンス化して配置します。名前: {puzzlePrefab.name}");

            // パズルをインスタンス化
            GameObject puzzleInstance = Instantiate(puzzlePrefab);
            instantiatedPuzzles.Add(puzzleInstance);

            // RectTransform を取得または追加
            RectTransform puzzleRectTransform = puzzleInstance.GetComponent<RectTransform>();
            if (puzzleRectTransform == null)
            {
                Debug.Log($"パズル {i} に RectTransform がありません。新規追加します。");
                puzzleRectTransform = puzzleInstance.AddComponent<RectTransform>();
            }
            // 親オブジェクトに設定（ローカル座標を維持）
            puzzleRectTransform.SetParent(puzzleListParent.transform, false);

            int row = i / columns;
            int col = i % columns;
            float xPos = (col * cellWidth) - (panelWidth / 2) + offsetX;
            float yPos = (row * cellHeight) - (panelHeight / 2) + offsetY;

            // パズルの位置、スケール、サイズを設定
            puzzleRectTransform.localScale = Vector3.one;
            puzzleRectTransform.anchoredPosition = new Vector2(xPos, -yPos);
            puzzleRectTransform.sizeDelta = new Vector2(cellWidth * 0.9f, cellHeight * 0.9f);

            // ランダムにパズルの回転角度（90, 180, 270, 360度）を設定
            float[] randomAngles = { 90f, 180f, 270f, 360f };
            float randomAngle = randomAngles[UnityEngine.Random.Range(0, randomAngles.Length)];
            puzzleInstance.transform.rotation = Quaternion.Euler(0, 0, randomAngle);
        }
    }

    /// <summary>
    /// すべてのパズルが正しい角度で配置されているかチェックし、条件を満たす場合はパズル完成処理を実行します。
    /// </summary>
    private void CheckAllPuzzleAngle()
    {
        // handInteractor からパズルのスタックを取得
        Stack<GameObject> puzzleStack = handInteractor.PuzzleStack;
        if (puzzleStack == null || puzzleStack.Count == 0 || puzzleStack.Count != puzzleList.Count)
        {
            return;
        }

        foreach (GameObject puzzle in puzzleStack)
        {
            if (puzzle == null)
            {
                Debug.LogWarning("パズルスタックに null の項目があります。スキップします...");
                continue;
            }
            // Z軸の回転角度の誤差が1度以内であるかチェック
            if (Mathf.Abs(puzzle.transform.rotation.eulerAngles.z) > 1f)
            {
                Debug.Log("すべてのパズルが正しく揃っていません。調整を続けてください！");
                return;
            }
        }
        PuzzleComplete();
    }

    /// <summary>
    /// すべてのパズルが正しく揃った場合の処理を実行します。
    /// ワークベンチアニメーションを再生し、シナリオ状態を切り替えます。
    /// </summary>
    private async void PuzzleComplete()
    {
        if (workbenchAnimator == null)
        {
            Debug.LogError("workbenchAnimator が指定されていません！");
            return;
        }

        // ワークベンチアニメーションの完了を示すフラグを設定
        workbenchAnimator.SetBool("isComplete", true);

        // アニメーションの完了を待つ
        await CompleteAnimationAsync();

        workbenchAnimator.SetBool("isComplete", false);
        activeScenarioState.Active();
        GameStateManager.Instance.SwitchState(GameState.FreeRoam);
    }

    /// <summary>
    /// Puzzleモードを終了し、自由移動モードに切り替えます。
    /// </summary>
    public void ExitPuzzleMode()
    {
        GameStateManager.Instance.SwitchState(GameState.FreeRoam);
    }

    /// <summary>
    /// ワークベンチアニメーションが完了するまで待機する非同期処理です。
    /// </summary>
    /// <returns>アニメーション完了までの Task</returns>
    private async Task CompleteAnimationAsync()
    {
        while (true)
        {
            AnimatorStateInfo stateInfo = workbenchAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 1.0f && !workbenchAnimator.IsInTransition(0))
            {
                break;
            }
            await Task.Yield();
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// handInteractor から発生するパズル配置完了イベントのコールバックです。
    /// 各パズルの角度をチェックし、正しければ PuzzleComplete を実行します。
    /// </summary>
    private void CheckAllPuzzleAngleCallback()
    {
        CheckAllPuzzleAngle();
    }

    #endregion
}
