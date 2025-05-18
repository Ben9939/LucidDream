using EasyTransition;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using static DialogData;
using static SceneMapManager;

/// <summary>
/// DialogController クラスは、対話データに基づいたダイアログ表示、
/// 選択肢処理、およびイベント処理を管理するクラスです。
/// ダイアログのタイピング演出や、選択肢による分岐、CG のフェードイン処理などを実装します。
/// </summary>
public class DialogController : MonoBehaviour
{
    [Header("Dialog UI Elements")]
    [SerializeField] private GameObject dialogBox;    // 対話ボックス
    [SerializeField] private GameObject dialogText;     // 対話テキスト表示オブジェクト

    [Header("Dialog Settings")]
    [SerializeField] private int lettersPersecond;      // 1秒あたりの文字表示速度

    [Header("Related Objects")]
    [SerializeField] private GameObject player;         // プレイヤーオブジェクト
    [SerializeField] private GameObject QrCodeLoader;     // QRコードローダーオブジェクト
    [SerializeField] private GameObject OneEye;           // OneEye オブジェクト

    [Header("Options Settings")]
    [SerializeField] private GameObject optionsPanel;     // 選択肢パネル

    [Header("Font Settings")]
    [SerializeField] private Font font;                   // 対話テキスト用フォント

    [Header("Transition Settings")]
    public TransitionManager manager;                   // シーン遷移管理

    [Header("Option Text List")]
    public List<Text> optionText;                       // 選択肢テキストリスト

    // CG 表示用オブジェクト（スクリプト内で生成・操作）
    private GameObject gameCG;

    // 選択肢関連の内部状態
    private bool isAwaitingChoice = false;
    private int selectedOptionIndex = -1;

    // 対話開始／終了イベント
    public event Action OnShowDialog;
    public event Action OnHideDialog;

    public static DialogController Instance { get; private set; }

    // 内部状態管理
    private bool isInDialog = false;
    private NpcDialog dialog;       // 現在の対話データ（NPC対話など）
    private bool isTyping;          // タイピング中フラグ
    private DialogData dialogData;  // 全体の対話データ
    private int optionIndex = 0;    // 選択されたオプションにより次の対話を決定するためのインデックス
    private int currentLine = 0;    // 現在表示中の対話行番号

    /// <summary>
    /// Awake：シングルトンの設定を行います。
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 対話を表示します（対話完了時のコールバックなし）。
    /// </summary>
    /// <param name="dialogs">対話データ</param>
    public IEnumerator ShowDialog(DialogData dialogs)
    {
        yield return new WaitForEndOfFrame();
        isInDialog = true;
        OnShowDialog?.Invoke();

        dialogData = dialogs;
        dialog = dialogs.dialogs[optionIndex];

        dialogBox.SetActive(true);
        dialogText.SetActive(true);

        StartCoroutine(TypeDialog(dialog.lines[0].line, Color.black));
    }

    /// <summary>
    /// 対話を表示します（対話完了時のコールバックあり）。
    /// </summary>
    /// <param name="dialogs">対話データ</param>
    /// <param name="onDialogComplete">対話完了時のコールバック</param>
    public IEnumerator ShowDialog(DialogData dialogs, Action onDialogComplete)
    {
        yield return new WaitForEndOfFrame();
        isInDialog = true;
        OnShowDialog?.Invoke();

        dialogData = dialogs;
        dialog = dialogs.dialogs[optionIndex];

        dialogBox.SetActive(true);
        dialogText.SetActive(true);

        StartCoroutine(TypeDialog(dialog.lines[0].line, Color.black));
        onDialogComplete?.Invoke();
    }

    /// <summary>
    /// メモリダイアログ（白文字）の表示を行います。
    /// </summary>
    /// <param name="dialogs">対話データ</param>
    public IEnumerator ShowMemoryDialog(DialogData dialogs)
    {
        yield return new WaitForEndOfFrame();
        isInDialog = true;
        OnShowDialog?.Invoke();

        dialogData = dialogs;
        dialog = dialogs.dialogs[optionIndex];

        dialogText.SetActive(true);

        StartCoroutine(TypeDialog(dialog.lines[0].line, Color.white));
    }

    /// <summary>
    /// 対話中の更新処理を行います（入力処理）。
    /// Space キーで対話を進行し、選択肢入力中は上下キーで選択処理を行います。
    /// </summary>
    public void HandleUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isTyping && !isAwaitingChoice)
        {
            ContinueDialog();
        }
        else if (isAwaitingChoice)
        {
            HandleChoiceInput();
        }
    }

    /// <summary>
    /// 対話を進行させる処理です。
    /// 次の行を表示し、イベントや選択肢がある場合はそれらの処理を実行します。
    /// </summary>
    private void ContinueDialog()
    {
        ++currentLine;
        if (currentLine < dialog.lines.Count)
        {
            // 対話行に設定されたイベントを判定
            Event eventType = Event.None;
            string dialogEventString = dialog.lines[currentLine].dialogEvent;
            Enum.TryParse(dialogEventString, out eventType);
            HandleCheckEventCondition(eventType);

            if (dialog.lines[currentLine].scenarioTrigger)
            {
                // ScenarioManager.Instance.IncreaseScenarioIndex();
            }
            if (eventType == Event.None || eventType == Event.ShowCG)
            {
                StartCoroutine(TypeDialog(dialog.lines[currentLine].line, Color.black));
            }
            else
            {
                currentLine = 0;
                isInDialog = false;
            }

            if (dialog.lines[currentLine].options.Count != 0)
            {
                optionsPanel.SetActive(true);
                DisplayOptions(dialog.lines[currentLine].options);
                isAwaitingChoice = true;
            }
        }
        else
        {
            ResetUiElements();
            ResetDialogState();
            OnHideDialog?.Invoke();
        }
    }

    /// <summary>
    /// 選択肢入力を処理します。
    /// 上下キーで選択肢を移動し、Space キーで選択確定します。
    /// </summary>
    private void HandleChoiceInput()
    {
        int previousIndex = selectedOptionIndex;
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            AudioManager.Instance.PlaySE(SESoundData.SE.Cursor);
            selectedOptionIndex = Mathf.Max(0, selectedOptionIndex - 1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            AudioManager.Instance.PlaySE(SESoundData.SE.Cursor);
            selectedOptionIndex = Mathf.Min(dialog.lines[currentLine].options.Count - 1, selectedOptionIndex + 1);
        }

        UpdateOptionText(previousIndex, selectedOptionIndex);

        // Space キーで選択確定
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (selectedOptionIndex != -1)
            {
                AudioManager.Instance.PlaySE(SESoundData.SE.Decide);
                isAwaitingChoice = false;
                ProcessSelectedOption();
                optionsPanel.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 選択肢テキストの表示内容を更新します。
    /// 選択中の項目には先頭に ">" を付加します。
    /// </summary>
    /// <param name="previousIndex">前回の選択肢インデックス</param>
    /// <param name="currentIndex">現在の選択肢インデックス</param>
    private void UpdateOptionText(int previousIndex, int currentIndex)
    {
        for (int i = 0; i < dialog.lines[currentLine].options.Count; i++)
        {
            if (i == currentIndex)
            {
                optionText[i].text = ">" + dialog.lines[currentLine].options[i];
            }
            else
            {
                optionText[i].text = dialog.lines[currentLine].options[i];
            }
        }
    }

    /// <summary>
    /// 選択されたオプションに基づき対話を進行させます。
    /// </summary>
    private void ProcessSelectedOption()
    {
        if (selectedOptionIndex >= 0 && selectedOptionIndex < dialog.lines[currentLine].options.Count)
        {
            optionIndex = dialog.lines[currentLine].nextDialogs[selectedOptionIndex];
            currentLine = 0;
            StartCoroutine(ShowDialog(dialogData));
        }
    }

    /// <summary>
    /// 選択肢を UI に表示します。
    /// </summary>
    /// <param name="options">選択肢の文字列リスト</param>
    private void DisplayOptions(List<string> options)
    {
        for (int i = 0; i < options.Count; i++)
        {
            optionText[i].text = options[i];
        }
    }

    /// <summary>
    /// 対話テキストを1文字ずつ表示するタイピング演出を実行します。
    /// </summary>
    /// <param name="line">表示する文字列</param>
    /// <param name="textColor">文字色</param>
    /// <returns>コルーチン</returns>
    public IEnumerator TypeDialog(string line, Color textColor)
    {
        isTyping = true;
        Text textComponent = dialogText.GetComponent<Text>();
        textComponent.text = "";
        textComponent.color = textColor;

        foreach (var letter in line.ToCharArray())
        {
            textComponent.text += letter;
            AudioManager.Instance.PlaySE(SESoundData.SE.Speak);
            yield return new WaitForSeconds(1f / lettersPersecond);
        }

        isTyping = false;
    }

    /// <summary>
    /// CG イベントの処理を行い、画像をフェードインで表示します。
    /// </summary>
    public IEnumerator ShowCgEvent()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "CG", dialog.lines[currentLine].CG);

        if (File.Exists(filePath))
        {
            byte[] imageData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            gameCG.GetComponent<Image>().sprite = sprite;
        }
        else
        {
            Debug.LogError($"File not found: {filePath}");
        }
        for (float alpha = 0f; alpha <= 1f; alpha += 0.05f)
        {
            Image cgImage = gameCG.GetComponent<Image>();
            Color currentColor = cgImage.color;
            currentColor.a = alpha;
            cgImage.color = currentColor;
            yield return new WaitForSeconds(0.05f);
        }
    }

    /// <summary>
    /// UI 要素を非表示にします。
    /// </summary>
    private void ResetUiElements()
    {
        dialogBox.SetActive(false);
        dialogText.SetActive(false);
    }

    /// <summary>
    /// 対話状態かどうかを返します。
    /// </summary>
    /// <returns>対話中なら true</returns>
    public bool GetisInDialog()
    {
        return isInDialog;
    }

    /// <summary>
    /// 対話イベントに応じた処理を実行します。
    /// Event が None の場合は何もしません。
    /// </summary>
    /// <param name="dialogEvent">対話イベント</param>
    private void HandleCheckEventCondition(Event dialogEvent)
    {
        if (dialogEvent == Event.None) return;

        var eventActions = new Dictionary<Event, Action>
        {
            { Event.ShowCG, () => StartCoroutine(ShowCgEvent()) }
        };

        if (eventActions.TryGetValue(dialogEvent, out var action))
        {
            action.Invoke();
        }
    }

    /// <summary>
    /// 対話状態をリセットし、UI を閉じます。
    /// </summary>
    private void ResetDialogState()
    {
        dialogBox.SetActive(false);
        dialogText.SetActive(false);
        isInDialog = false;
        currentLine = 0;
        optionIndex = 0;
    }
}
