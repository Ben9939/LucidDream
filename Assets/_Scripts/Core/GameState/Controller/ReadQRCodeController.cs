using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// ReadQRCodeControllerは、QRコードの読み取りモードにおいて、
/// 指定された正解と受信データを比較し、正解の場合はHideアニメーションを再生後、
/// リソースを解放してFreeRoam状態へ遷移するコントローラです。
/// また、ランダムな瞬きアニメーションを実行し、ユーザーに視覚的なフィードバックを提供します。
/// </summary>
public class ReadQRCodeController : ControllerBase
{
    [SerializeField] private GameObject eyesGameObject;
    [SerializeField] private UDPReceive uDPReceiver;
    [SerializeField] private string correctAnswer;
    [Header("ActiveScenarioState")]
    [SerializeField] private ScenarioStateSO activeScenarioState;
    private Task runTimeTask;

    // 非同期処理中の瞬きアニメーション実行中かどうかを示すフラグ
    private bool isBlinking = false;
    // オブジェクトが破棄されたかどうかを判定するフラグ
    private bool isDestroyed = false;

    /// <summary>
    /// 毎フレーム呼び出される更新処理です。
    /// 物件が破棄されている場合は処理を中断し、瞬きアニメーションの実行や正解判定を行います。
    /// 正解の場合、Hideアニメーションを再生し、リソース解放および状態遷移を実施します。
    /// </summary>
    protected override async void OnUpdate()
    {
        // 既にオブジェクトが破棄されている場合は何もせず終了
        if (isDestroyed) return;

        // 瞬きアニメーション中でなければランダムな瞬きを実行
        if (!isBlinking)
        {
            await RandomBlinkAsync();
        }

        // 正解判定：受信データが正解と一致するか確認
        if (IsAnswerCorrect(correctAnswer))
        {
            runTimeTask = PlayAnimation("Hide");
            await runTimeTask;
            Debug.Log("AnswerCorrect");

            // UDPReceiveコンポーネントが存在するか確認し、リソースを解放する
            UDPReceive udpReceiver = GetComponent<UDPReceive>();
            if (udpReceiver != null)
            {
                udpReceiver.ReleaseResources();
            }
            // GameStateManagerが存在する場合は状態をFreeRoamに切り替える
            if (GameStateManager.Instance != null)
            {
                activeScenarioState.Active();
                GameStateManager.Instance.SwitchState(GameState.FreeRoam);
            }
        }
    }

    /// <summary>
    /// クリーンアップ処理です。Pythonスクリプトを停止します。
    /// </summary>
    protected override void Cleanup()
    {
        PythonProcessManager.Instance.StopPythonScript();
    }

    /// <summary>
    /// 指定されたアニメーションを再生し、その終了まで待機します。
    /// アニメーションが存在しない場合は警告を出して処理をスキップします。
    /// </summary>
    /// <param name="animationName">再生するアニメーションの名前</param>
    private async Task PlayAnimation(string animationName)
    {
        // eyesGameObjectが存在しなければ警告を出して終了
        if (eyesGameObject == null)
        {
            Debug.LogWarning("eyesGameObjectがnullです。アニメーション: " + animationName + " をスキップします。");
            return;
        }
        Animator animator = eyesGameObject.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("eyesGameObjectにAnimatorコンポーネントが見つかりません。アニメーション: " + animationName + " をスキップします。");
            return;
        }
        // 指定されたアニメーションを再生
        animator.Play(animationName);
        // 一フレーム待機してアニメーション状態を更新
        await Task.Yield();
        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        await Task.Delay(System.TimeSpan.FromSeconds(animationLength));
    }

    /// <summary>
    /// ランダムな間隔で瞬きアニメーション("Blink")を実行します。
    /// </summary>
    private async Task RandomBlinkAsync()
    {
        isBlinking = true;
        // 3～5秒間の待機後に瞬き処理を開始
        await Task.Delay(System.TimeSpan.FromSeconds(Random.Range(3f, 5f)));

        // 1～2回の瞬きを実行
        int blinkCount = Random.Range(1, 3);
        for (int i = 0; i < blinkCount; i++)
        {
            await PlayAnimation("Blink");
        }
        isBlinking = false;
    }

    /// <summary>
    /// 受信データが正解と一致するか確認します。
    /// UDPReceiveコンポーネントから取得したデータと正解文字列を比較します。
    /// </summary>
    /// <param name="correctAnswer">正解の文字列</param>
    /// <returns>一致する場合はtrue、しない場合はfalse</returns>
    private bool IsAnswerCorrect(string correctAnswer)
    {
        // UDPReceiveコンポーネントを取得
        UDPReceive udpReceiver = GetComponent<UDPReceive>();
        if (udpReceiver == null)
        {
            Debug.LogWarning("UDPReceiveコンポーネントが存在しません。");
            return false;
        }
        return string.Equals(udpReceiver.data.ToString(), correctAnswer);
    }

    /// <summary>
    /// QRコード読み取りモードを終了します。
    /// Hideアニメーションを再生し、リソース解放と状態切り替えを実施します。
    /// </summary>
    public async void ExitReadQRCodeMode()
    {
        if (eyesGameObject != null)
        {
            runTimeTask = PlayAnimation("Hide");
            await runTimeTask;
        }

        UDPReceive udpReceiver = GetComponent<UDPReceive>();
        if (udpReceiver != null)
        {
            udpReceiver.ReleaseResources();
        }

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SwitchState(GameState.FreeRoam);
        }
    }

    /// <summary>
    /// オブジェクトが破棄される際に呼び出され、以降の非同期処理でのアクセスを防ぐためのフラグを設定します。
    /// </summary>
    private void OnDestroy()
    {
        isDestroyed = true;
    }
}
