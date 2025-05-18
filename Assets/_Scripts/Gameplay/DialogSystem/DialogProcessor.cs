using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// SpeakerData は、対話に使用するキャラクター情報および実行時に生成する立ち絵の管理を行います。
/// </summary>
[System.Serializable]
public class SpeakerData
{
    [Header("Design Data")]
    public SpeakerName speakerName;
    public UnitDataSO unitData; 

    [System.NonSerialized]
    public GameObject portraitObject; // 実行時に生成される立ち絵オブジェクト
    [System.NonSerialized]
    public PortraitUI portraitUI;     // 立ち絵制御スクリプト

    /// <summary>
    /// unitData 内の PortraitPrefab を基に立ち絵を生成し、container の子オブジェクトとして配置します。
    /// 生成後、立ち絵は非表示となり、SpeakerName に応じて左右の位置を調整します。
    /// </summary>
    /// <param name="container">生成した立ち絵を配置する親 Transform</param>
    public void Initialize(Transform container)
    {
        if (unitData == null)
        {
            Debug.LogWarning($"[{speakerName}] unitData が null です。");
            return;
        }

        var portraitHolder = unitData as IPortraitHolder;
        if (portraitHolder == null)
        {
            Debug.LogWarning($"[{speakerName}] unitData を IPortraitHolder に変換できません。");
            return;
        }

        if (portraitHolder.PortraitPrefab == null)
        {
            Debug.LogWarning($"[{speakerName}] PortraitPrefab が null です。");
            return;
        }

        // 立ち絵を生成して container の子として設定
        portraitObject = GameObject.Instantiate(portraitHolder.PortraitPrefab, container);
        portraitUI = portraitObject.GetComponent<PortraitUI>();

        // 初期状態は非表示
        portraitObject.SetActive(false);

        // SpeakerName に応じた位置調整（例：Player は左側、その他は右側）
        RectTransform rt = portraitObject.GetComponent<RectTransform>();
        if (rt != null)
        {
            float xPos = 0f;
            float yPos = 0f;
            switch (speakerName)
            {
                case SpeakerName.Player:
                    xPos = -75f;
                    yPos = 25f;
                    break;
                case SpeakerName.None:
                    xPos = 0f;
                    yPos = 0f;
                    break;
                case SpeakerName.Slime:
                    xPos = 75f;
                    yPos = 0f;
                    break;
                default:
                    xPos = 75f;
                    yPos = 25f;
                    break;
            }
            rt.anchoredPosition = new Vector2(xPos, yPos);
        }
    }
}

/// <summary>
/// DialogProcessor は、対話グラフに基づき対話を進行させるロジックを実装します。
/// UI インターフェース（IDialogUI）を介して対話表示や選択肢処理を行います。
/// </summary>
public class DialogProcessor
{
    private DialogGraph graph;
    private IDialogUI ui;  // UI 介面
    private bool isWaitingForSpace = false;
    private int selectedOptionIndex = 0;
    private int optionCount = 0;
    public event Action onDialogEnd;
    private SpeakerName currentNPC = SpeakerName.None;

    /// <summary>
    /// コンストラクタ：UI 介面を注入します。
    /// </summary>
    /// <param name="dialogUI">対話表示に使用する UI</param>
    public DialogProcessor(IDialogUI dialogUI)
    {
        this.ui = dialogUI;
    }

    /// <summary>
    /// 対話グラフを設定します。必要に応じて Instantiate(newGraph) で複製することも検討してください。
    /// </summary>
    public void SetDialogGraph(DialogGraph newGraph)
    {
        this.graph = newGraph;
    }

    /// <summary>
    /// 対話を開始します。
    /// </summary>
    public async Task StartDialog()
    {
        // 対話開始前に現在再生中のBGMを保存する
        BGMSoundData.BGM previousBGM = AudioManager.Instance.CurrentBGM;

        // 対話用BGMを再生するかどうかのフラグ
        bool isDialogueBgmStarted = false;

        // 対話用BGMが設定されている場合のみ、BGMを切り替える
        if (graph.BGM != BGMSoundData.BGM.None)
        {
            AudioManager.Instance.PlayBGM(graph.BGM);
            isDialogueBgmStarted = true;
        }

        // DialogGraphが設定されていなければ警告を出して終了する
        if (graph == null)
        {
            Debug.LogWarning("Dialog graph が設定されていません。");
            return;
        }

        // 必要なBGM設定など、他の前処理があればここで実施

        // 対話中に登場する最初のNPC（プレイヤー以外かつNone以外）を決定する
        SpeakerName initialNPC = SpeakerName.None;
        foreach (Dialog_BaseNode node in graph.nodes)
        {
            if (node is Dialog_LineNode lineNode)
            {
                if (lineNode.speaker != SpeakerName.Player && lineNode.speaker != SpeakerName.None)
                {
                    initialNPC = lineNode.speaker;
                    break;
                }
            }
        }

        // プレイヤーの立ち絵を即座に表示する
        ui.ShowSpeaker(SpeakerName.Player);
        ui.SetSpeakerTalking(SpeakerName.Player, false);

        // 最初のNPCが存在する場合、その立ち絵も表示する
        if (initialNPC != SpeakerName.None)
        {
            ui.ShowSpeaker(initialNPC);
            ui.SetSpeakerTalking(initialNPC, false);
            currentNPC = initialNPC;
        }

        // 最初のStartNodeを探して、DialogGraphのcurrentNodeに設定する
        foreach (Dialog_BaseNode node in graph.nodes)
        {
            if (node is Dialog_StartNode)
            {
                graph.currentNode = node;
                break;
            }
        }

        // currentNodeが設定されていれば、対話の解析を開始する
        if (graph.currentNode != null)
        {
            await ParseNode();
        }

        // 対話終了後、対話用BGMが再生されていた場合は停止し、元のBGMに戻す
        if (isDialogueBgmStarted)
        {
            if (previousBGM != BGMSoundData.BGM.None)
            {
                AudioManager.Instance.PlayBGM(previousBGM);
            }
        }
    }



    /// <summary>
    /// 対話グラフのノードを解析して対話を進行します。
    /// </summary>
    private async Task ParseNode()
    {
        if (graph.currentNode == null) return;

        switch (graph.currentNode)
        {
            case Dialog_StartNode:
                NextNode("exit");
                break;
            case Dialog_LineNode lineNode:
                await HandleLineNode(lineNode);
                break;
            case Dialog_BranchNode branchNode:
                await HandleBranchNode(branchNode);
                break;
            case Dialog_EventNode eventNode:
                HandleEventNode(eventNode);
                break;
            case Dialog_EndNode:
                await HandleEndNode();
                return;
        }

        if (!(graph.currentNode is Dialog_EndNode))
        {
            await ParseNode();
        }
    }


    /// <summary>
    /// LineNode を処理し、対話テキストを逐次表示します。
    /// </summary>
    /// 
    private async Task HandleLineNode(Dialog_LineNode node)
    {
        if (node.GetCG() != null)
        {
            ui.HideCg();
            ui.ShowCg(node.GetCG());
        }
        // 話者に応じた立ち絵および講話アニメーションの設定
        if (node.speaker == SpeakerName.Player)
        {
            ui.ShowSpeaker(SpeakerName.Player);
            ui.SetSpeakerTalking(SpeakerName.Player, true);
        }
        else if (node.speaker != SpeakerName.None)
        {
            if (currentNPC != node.speaker)
            {
                if (currentNPC != SpeakerName.None)
                {
                    ui.HideSpeaker(currentNPC);
                }
                currentNPC = node.speaker;
            }
            ui.ShowSpeaker(currentNPC);
            ui.SetSpeakerTalking(node.speaker, true);
        }

        string text = node.GetDialogLine();
        ui.ClearLineText();
        ui.SetSpeakerName(node.speaker);

        // 文字を1文字ずつ表示
        foreach (char c in text)
        {
            ui.ShowLineText(c.ToString());
            AudioManager.Instance.PlaySE(SESoundData.SE.Speak);
            await Task.Delay(50);
        }

        // 講話終了後、講話アニメーションを停止
        if (node.speaker == SpeakerName.Player)
        {
            ui.SetSpeakerTalking(SpeakerName.Player, false);
        }
        else if (node.speaker != SpeakerName.None)
        {
            ui.SetSpeakerTalking(node.speaker, false);
        }

        await WaitForPlayerInput();
        NextNode("exit");
    }

    /// <summary>
    /// EventNode を処理します（イベントの発火後、次のノードへ遷移）
    /// </summary>
    private void HandleEventNode(Dialog_EventNode node)
    {
        node.Event?.Invoke();
        NextNode("exit");
    }

    /// <summary>
    /// BranchNode を処理し、選択肢を表示・入力待ちし、選択結果に応じた次のノードへ遷移します。
    /// </summary>
    private async Task HandleBranchNode(Dialog_BranchNode branchNode)
    {
        List<string> options = branchNode.GetOptionTexts();
        optionCount = options.Count;
        ui.ShowOptions(options);

        await WaitForPlayerInput();
        ui.HideOptions();

        UnityEvent optionEvent = branchNode.GetOptionEvent(selectedOptionIndex);
        string nextField = branchNode.GetNextNode(selectedOptionIndex);
        optionEvent?.Invoke();

        if (!string.IsNullOrEmpty(nextField))
        {
            NextNode(nextField);
        }
        else
        {
            Debug.LogError("選択肢に対応する遷移先がありません。");
        }
    }

    /// <summary>
    /// EndNode を処理し、対話終了処理を実行します。
    /// </summary>
    private async Task HandleEndNode()
    {
        await Task.Delay(100);
        ui.OnDialogEnd();
        onDialogEnd?.Invoke();
    }

    /// <summary>
    /// 指定されたポート名に対応する接続先ノードへ遷移します。
    /// </summary>
    public void NextNode(string portName)
    {
        if (graph.currentNode == null) return;
        foreach (var port in graph.currentNode.Ports)
        {
            if (port.fieldName == portName && port.Connection != null)
            {
                graph.currentNode = port.Connection.node as Dialog_BaseNode;
                if (graph.currentNode is Dialog_EndNode)
                {
                    HandleEndNode();
                }
                break;
            }
        }
    }

    /// <summary>
    /// 空白キー入力を検出し、入力待ち状態を解除します。
    /// </summary>
    public void HandleConfirmInput()
    {
        if (isWaitingForSpace && Input.GetKeyDown(KeyCode.Space))
        {
            AudioManager.Instance.PlaySE(SESoundData.SE.Decide);
            isWaitingForSpace = false;
        }
    }

    /// <summary>
    /// 上下キー入力により分岐選択肢の選択インデックスを更新します。
    /// </summary>
    public void HandleSelectionInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedOptionIndex = Mathf.Max(0, selectedOptionIndex - 1);
            AudioManager.Instance.PlaySE(SESoundData.SE.Cursor);
            ui.UpdateOptionText(selectedOptionIndex);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedOptionIndex = Mathf.Min(optionCount - 1, selectedOptionIndex + 1);
            AudioManager.Instance.PlaySE(SESoundData.SE.Cursor);
            ui.UpdateOptionText(selectedOptionIndex);
        }
    }

    /// <summary>
    /// プレイヤー入力（空白キー）を待機します。
    /// </summary>
    private async Task WaitForPlayerInput()
    {
        if (ui is EventDialogUI eventDialogUI)
        {
            while (eventDialogUI.IsCGAnimating)
            {
                await Task.Yield();
            }
        }

        isWaitingForSpace = true;
        while (isWaitingForSpace)
        {
            await Task.Yield();
        }
    }
}
