using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// MainTitleControllerは、ゲームのタイトル画面の各フェーズ（Opening, Splash, Title, Tutorialなど）
/// の表示・遷移を管理するコントローラです。<br/>
/// - 各フェーズに応じたCanvasの表示制御<br/>
/// - フェードイン/フェードアウト等のエフェクト再生<br/>
/// - メニュー入力処理による状態遷移の実行<br/>
/// ロード、フェード、入力ロックなどの処理はコルーチンで実装されています。
/// </summary>
public class MainTitleController : ControllerBase
{
    #region Serialized Fields
    [Header("Canvas Objects")]
    [SerializeField] private GameObject openingCanvas;
    [SerializeField] private GameObject splashCanvas;
    [SerializeField] private GameObject mainTitleCanvas;
    [SerializeField] private GameObject tutorialCanvas;

    [Header("GreenText Settings")]
    [SerializeField] private GameObject greenText;
    [SerializeField] private float greenTextDisplayTime = 2f;
    [SerializeField] private float greenTextFadeTime = 1f; // Fade in time for GreenText

    [Header("Splash Parameters")]
    [Tooltip("Editable splash content supporting text and images")]
    [SerializeField] private SplashDataSO[] splashDataArray;
    [SerializeField] private float splashHoldTime = 3f;
    [SerializeField] private float fadeTime = 1.5f;

    [Header("Splash Image")]
    [SerializeField] private Image splashImage;

    [Header("Menu Settings")]
    [Tooltip("Text component for displaying main menu options and the 'PUSH ANY BUTTON' prompt")]
    [SerializeField] private Text menuOptionsText;

    #endregion

    #region Private Fields

    // 選單選項（順序：0 = Game Start，1 = Credits）
    private string[] menuOptions = new string[] { "Game Start", "Credits" };
    private int selectedOptionIndex = 0;

    private CanvasGroup splashGroup;
    private CanvasGroup mainTitleGroup;
    private CanvasGroup tutorialGroup;
    private CanvasGroup greenTextGroup;
    private Text uiText;
    private TMP_Text tmpText;
    private AudioManager audioManager;

    // 各フェーズをenumで定義
    private enum TitlePhase
    {
        Opening,
        Splash,
        TitleFadeIn,
        WaitForPushAnyButton,  // 「PUSH ANY BUTTON」提示状態
        Title,                 // 選單表示状態
        TitleFadeOut,
        TutorialFadeIn,
        Tutorial,
        Done
    }
    private TitlePhase currentPhase = TitlePhase.Opening;

    // 静的フラグ：最初のみ完全なOpeningシーケンスを実行し、その後はTitle画面のみ表示
    private static bool hasShownOpening = false;

    // 入力ロック：状態遷移完了前に連続入力を防ぐためのロック
    private bool inputLocked = false;
    private float inputLockDuration = 0.2f; // 必要に応じて調整

    #endregion

    #region MonoBehaviour Methods

    /// <summary>
    /// ゲーム状態開始時の処理を実行します。
    /// ゲーム状態を取得し、必要に応じたCanvasの初期状態を設定します。<br/>
    /// Openingシーケンス未実行の場合は再生し、実行済みの場合はTitle画面へ遷移します。
    /// </summary>
    protected override void Initialize()
    {
        GameStateBase currentState = GameStateManager.Instance.GetCurrentState();

        if (currentState is MainTitleState mainTitleState)
        {
            SubscribeToState(mainTitleState);
        }
        // Canvasの初期表示設定
        openingCanvas.SetActive(!hasShownOpening);
        splashCanvas.SetActive(false);
        mainTitleCanvas.SetActive(false);
        tutorialCanvas.SetActive(false);

        // 各CanvasのCanvasGroupを取得または追加
        splashGroup = GetOrAddCanvasGroup(splashCanvas);
        mainTitleGroup = GetOrAddCanvasGroup(mainTitleCanvas);
        tutorialGroup = GetOrAddCanvasGroup(tutorialCanvas);
        greenTextGroup = GetOrAddCanvasGroup(greenText);

        // Splash用のテキストコンポーネントを取得
        uiText = splashCanvas.GetComponentInChildren<Text>();
        tmpText = splashCanvas.GetComponentInChildren<TMP_Text>();

        audioManager = AudioManager.Instance;

        // Openingシーケンスの再生状況に応じて処理を分岐
        if (!hasShownOpening)
        {
            StartCoroutine(PlayOpeningSequence());
            hasShownOpening = true;
        }
        else
        {
            audioManager.PlayBGM(BGMSoundData.BGM.Title);
            mainTitleCanvas.SetActive(true);
            mainTitleGroup.alpha = 1f;
            currentPhase = TitlePhase.WaitForPushAnyButton;
            UpdateOptionText();
        }
       
    }

    /// <summary>
    /// 毎フレーム実行される状態更新処理です。
    /// 各フェーズに応じた入力処理や状態遷移を行います。
    /// </summary>
    protected override void OnUpdate()
    {
        switch (currentPhase)
        {
            case TitlePhase.WaitForPushAnyButton:
                // プレイヤーが任意のキーを押した場合、Titleフェーズへ移行
                if (Input.anyKeyDown)
                {
                    audioManager.PlaySE(SESoundData.SE.Cursor);
                    currentPhase = TitlePhase.Title;
                    UpdateOptionText();
                    StartCoroutine(LockInputCoroutine());
                }
                break;

            case TitlePhase.Title:
                if (!inputLocked)
                {
                    // 上下矢印キーで選択項目を更新
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        selectedOptionIndex = Mathf.Max(0, selectedOptionIndex - 1);
                        audioManager.PlaySE(SESoundData.SE.Cursor);
                        UpdateOptionText();
                    }
                    else if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        selectedOptionIndex = Mathf.Min(menuOptions.Length - 1, selectedOptionIndex + 1);
                        audioManager.PlaySE(SESoundData.SE.Cursor);
                        UpdateOptionText();
                    }
                    // スペースキーで選択項目を決定
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        inputLocked = true;
                        audioManager.PlaySE(SESoundData.SE.Decide);
                        OnMenuSelection();
                        StartCoroutine(LockInputCoroutine());
                    }
                }
                break;

            case TitlePhase.Tutorial:
                // Tutorialフェーズ中は任意のキー入力でシナリオを進める
                if (Input.anyKeyDown)
                {
                    ScenarioManager.Instance.AdvanceScenario();
                    currentPhase = TitlePhase.Done;
                }
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// ゲーム状態終了時の処理です。
    /// 必要に応じたクリーンアップ処理を行います。
    /// </summary>
    protected override void Cleanup()
    {
        GameStateBase currentState = GameStateManager.Instance.GetCurrentState();
        if (currentState is MainTitleState mainTitleState)
        {
            UnsubscribeFromState(mainTitleState);
        }
    }

    #endregion

    #region Coroutines

    /// <summary>
    /// 指定時間、入力をロックするコルーチンです。
    /// 連続入力による誤動作を防ぎます。
    /// </summary>
    private IEnumerator LockInputCoroutine()
    {
        inputLocked = true;
        yield return new WaitForSeconds(inputLockDuration);
        inputLocked = false;
    }

    /// <summary>
    /// Openingフェーズの音声再生、フェード処理、Splash内容の再生を行うコルーチンです。
    /// </summary>
    private IEnumerator PlayOpeningSequence()
    {
        // Opening音楽およびBGMを再生
        audioManager.PlayBGM(BGMSoundData.BGM.Nose);

        audioManager.PlaySE(SESoundData.SE.OpenCease);
        yield return new WaitForSeconds(audioManager.GetSEClipLength(SESoundData.SE.OpenCease) + 0.5f);

        audioManager.PlaySE(SESoundData.SE.CloseCease);
        yield return new WaitForSeconds(audioManager.GetSEClipLength(SESoundData.SE.CloseCease) + 0.5f);

        audioManager.PlaySE(SESoundData.SE.SwitchOn);
        yield return new WaitForSeconds(audioManager.GetSEClipLength(SESoundData.SE.SwitchOn) / 2);

        // GreenTextのフェード処理開始
        if (greenText != null)
        {
            StartCoroutine(FadeInGreenText());
        }

        yield return new WaitForSeconds(audioManager.GetSEClipLength(SESoundData.SE.SwitchOn) / 2 + 0.5f);
        yield return new WaitForSeconds(1f);

        openingCanvas.SetActive(false);

        // Splash内容の再生
        currentPhase = TitlePhase.Splash;
        for (int i = 0; i < splashDataArray.Length; i++)
        {
            yield return StartCoroutine(PlaySplash(splashDataArray[i]));
        }

        // Title画面へのフェードイン
        currentPhase = TitlePhase.TitleFadeIn;
        yield return StartCoroutine(FadeInCanvas(mainTitleGroup, mainTitleCanvas));

        currentPhase = TitlePhase.WaitForPushAnyButton;
        audioManager.PlayBGM(BGMSoundData.BGM.Title);
        UpdateOptionText();
    }

    /// <summary>
    /// Tutorialフェーズへの遷移を行うコルーチンです。
    /// Title画面のフェードアウト後、Tutorial画面のフェードインを実行します。
    /// </summary>
    private IEnumerator TransitionToTutorial()
    {
        currentPhase = TitlePhase.TitleFadeOut;
        yield return StartCoroutine(FadeOutCanvas(mainTitleGroup));

        currentPhase = TitlePhase.TutorialFadeIn;
        yield return StartCoroutine(FadeInCanvas(tutorialGroup, tutorialCanvas));

        currentPhase = TitlePhase.Tutorial;
    }

    /// <summary>
    /// GreenTextのフェードイン処理を行うコルーチンです。
    /// </summary>
    private IEnumerator FadeInGreenText()
    {
        greenText.SetActive(true);
        greenTextGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < greenTextFadeTime)
        {
            elapsed += Time.deltaTime;
            greenTextGroup.alpha = Mathf.Clamp01(elapsed / greenTextFadeTime);
            yield return null;
        }

        yield return new WaitForSeconds(greenTextDisplayTime);
        greenText.SetActive(false);
    }

    /// <summary>
    /// SplashDataに基づき、画像またはテキストのSplashを再生するコルーチンです。
    /// </summary>
    /// <param name="splashData">Splashデータ</param>
    private IEnumerator PlaySplash(SplashDataSO splashData)
    {
        if (!splashCanvas) yield break;

        // 画像が設定されている場合
        if (splashData.splashSprite != null && splashImage != null)
        {
            splashImage.sprite = splashData.splashSprite;
            splashImage.gameObject.SetActive(true);
            if (uiText != null) uiText.gameObject.SetActive(false);
            if (tmpText != null) tmpText.gameObject.SetActive(false);
        }
        else
        {
            // 画像がない場合はテキストを表示
            if (uiText != null)
            {
                uiText.text = splashData.message;
                uiText.gameObject.SetActive(true);
            }
            if (tmpText != null)
            {
                tmpText.text = splashData.message;
                tmpText.gameObject.SetActive(true);
            }
            if (splashImage != null)
            {
                splashImage.gameObject.SetActive(false);
            }
        }

        splashCanvas.SetActive(true);
        splashGroup.alpha = 0f;

        yield return StartCoroutine(FadeInCanvas(splashGroup, splashCanvas));
        yield return new WaitForSeconds(splashHoldTime);
        yield return StartCoroutine(FadeOutCanvas(splashGroup));

        splashCanvas.SetActive(false);
    }

    /// <summary>
    /// 指定されたCanvasGroupのフェードイン処理を行います。
    /// </summary>
    /// <param name="canvasGroup">フェード対象のCanvasGroup</param>
    /// <param name="canvasObj">対象のCanvasオブジェクト</param>
    private IEnumerator FadeInCanvas(CanvasGroup canvasGroup, GameObject canvasObj)
    {
        canvasObj.SetActive(true);
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeTime);
            yield return null;
        }
    }

    /// <summary>
    /// 指定されたCanvasGroupのフェードアウト処理を行い、対象Canvasを非表示にします。
    /// </summary>
    /// <param name="canvasGroup">フェード対象のCanvasGroup</param>
    private IEnumerator FadeOutCanvas(CanvasGroup canvasGroup)
    {
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1 - (elapsed / fadeTime));
            yield return null;
        }
        canvasGroup.gameObject.SetActive(false);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// 指定されたGameObjectにCanvasGroupコンポーネントが存在するか取得し、
    /// 存在しなければ新規に追加して返します。
    /// </summary>
    /// <param name="obj">対象のGameObject</param>
    /// <returns>CanvasGroupコンポーネント</returns>
    private CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        if (!obj) return null;
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = obj.AddComponent<CanvasGroup>();
        }
        cg.alpha = 0f;
        return cg;
    }

    #endregion

    #region Menu Handling

    /// <summary>
    /// メニュー表示のテキストを更新します。
    /// 現在のフェーズおよび選択状態に応じて、表示内容を切り替えます。
    /// </summary>
    private void UpdateOptionText()
    {
        if (menuOptionsText == null) return;

        if (currentPhase == TitlePhase.WaitForPushAnyButton)
        {
            menuOptionsText.text = "PUSH ANY BUTTON";
        }
        else if (currentPhase == TitlePhase.Title)
        {
            string display = "";
            for (int i = 0; i < menuOptions.Length; i++)
            {
                display += (i == selectedOptionIndex ? "> " : "  ") + menuOptions[i] + "\n";
            }
            menuOptionsText.text = display;
        }
    }

    /// <summary>
    /// メニュー選択時の処理を実行します。
    /// 選択された項目に応じて、状態遷移を行います。
    /// </summary>
    private void OnMenuSelection()
    {
        // 選択肢: 0 = Game Start、1 = Credits
        if (selectedOptionIndex == 0)
        {
            audioManager.StopBGM();
            StartCoroutine(TransitionToTutorial());
        }
        else if (selectedOptionIndex == 1)
        {
            SwitchToCredits();
        }
    }

    /// <summary>
    /// Credits表示時に直接状態を切り替えます。
    /// CreditScroll処理は行いません。
    /// </summary>
    private void SwitchToCredits()
    {
        GameStateManager.Instance.SwitchState(GameState.CreditScrolle);
    }

    #endregion
}
