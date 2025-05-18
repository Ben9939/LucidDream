using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ScenarioManager クラスは、シナリオグラフと各シナリオノードの制御を担当します。  
/// シングルトンパターンを採用しており、全体の劇情管理を行います。
/// </summary>
public class ScenarioManager : Singleton<ScenarioManager>
{
    [Header("Scenario Settings")]
    [SerializeField] public ScenarioGraph ScenarioGraph;

    [SerializeField] private List<DialogGraph> dialogGraphs = new List<DialogGraph>();
    /// <summary>
    /// 現在のシナリオノードを保持します。
    /// </summary>
    public Scenario_BaseNode CurrentScenarioNode { get; private set; }

    private DialogGraph currentDialogGraph = null;
    /// <summary>
    /// 現在のダイアロググラフを取得します。
    /// </summary>
    public DialogGraph CurrentDialogGraph { get { return currentDialogGraph; } }

    /// <summary>
    /// グローバル進捗更新時に発火するイベント
    /// </summary>
    public event Action<string> OnProgressChanged;

    [SerializeField] private List<ScenarioStateSO> scenarioStates;
    // 外部イベントとシナリオノードのマッピング（必要に応じて拡張）
    private Dictionary<string, Func<Scenario_BaseNode>> externalTriggerMap = new Dictionary<string, Func<Scenario_BaseNode>>();

    /// <summary>
    /// ResetGame イベント発火時にシナリオをリセットするためのアクション
    /// </summary>
    public UnityAction ResetScenarioAction;

    /// <summary>
    /// シナリオを初期化し、シナリオグラフの開始ノードをセットします。
    /// </summary>
    public void InitializeScenario()
    {
        if (ScenarioGraph != null)
        {
            ScenarioGraph.StartScenario();
        }
        else
        {
            Debug.LogError("ScenarioGraph is not set!");
        }
        ResetScenarioAction += ResetScenario;
        GameEventManager.StartListening(GameEventName.ResetGame, ResetScenarioAction);
    }

    /// <summary>
    /// 現在のシナリオノードから次のノードへ進み、各ノードの種類に応じた処理を実行します。
    /// </summary>
    public void AdvanceScenario()
    {
        // 次のノードへ進む（例："exit" ポートを使用）
        NextNode("exit");
        Scenario_BaseNode nextNode = ScenarioGraph.currentNode;
        if (nextNode == null)
            return;

        CurrentScenarioNode = nextNode;

        // ノードの種類に応じた処理分岐
        if (nextNode is Scenario_StartNode)
        {
            NextNode("exit");
        }
        else if (nextNode is Scenario_EnterScene enterSceneNode)
        {
            HandleEnterScene(enterSceneNode);
        }
        else if (nextNode is Scenario_CutInNode cutInNode)
        {
            HandleCutInNode(cutInNode);
        }
        else if (nextNode is Scenario_ConditionNode conditionNode)
        {
            HandleConditionNode(conditionNode);
        }
        else if (nextNode is Scenario_SceneChangeNode sceneChangeNode)
        {
            HandleSceneChangeNode(sceneChangeNode);
        }
        else if (nextNode is Scenario_BranchNode branchNode)
        {
            HandleBranchNode(branchNode);
        }
        else if (nextNode is Scenario_EventNode eventNode)
        {
            HandleEventNode(eventNode);
        }
        else if (nextNode is Scenario_EndingNode endingNode)
        {
            HandleEndingNode(endingNode);
        }
    }

    /// <summary>
    /// HandleEnterScene ノードの処理を実行します。
    /// </summary>
    /// <param name="node">Scenario_EnterScene ノード</param>
    private void HandleEnterScene(Scenario_EnterScene node)
    {
        StartCoroutine(GameStateManager.Instance.EnterState(node.TargetState));
    }

    /// <summary>
    /// HandleCutInNode ノードの処理を実行します。
    /// </summary>
    /// <param name="node">Scenario_CutInNode ノード</param>
    private void HandleCutInNode(Scenario_CutInNode node)
    {
        // Cut-in ノード固有の処理を実装する
    }

    /// <summary>
    /// HandleConditionNode ノードの処理を実行します。
    /// </summary>
    /// <param name="node">Scenario_ConditionNode ノード</param>
    private void HandleConditionNode(Scenario_ConditionNode node)
    {
        node.ScenarioState.Active();
        AdvanceScenario();
    }

    /// <summary>
    /// HandleSceneChangeNode ノードの処理を実行します。
    /// </summary>
    /// <param name="node">Scenario_SceneChangeNode ノード</param>
    private void HandleSceneChangeNode(Scenario_SceneChangeNode node)
    {
        GameStateManager.Instance.SwitchState(node.TargetState);
    }

    /// <summary>
    /// HandleBranchNode ノードの処理を実行します。
    /// </summary>
    /// <param name="node">Scenario_BranchNode ノード</param>
    private void HandleBranchNode(Scenario_BranchNode node)
    {
        // Branch ノード固有の処理を実装する
    }

    /// <summary>
    /// HandleEventNode ノードの処理を実行し、ダイアロググラフを設定してイベント状態に切り替えます。
    /// </summary>
    /// <param name="node">Scenario_EventNode ノード</param>
    private void HandleEventNode(Scenario_EventNode node)
    {
        currentDialogGraph = node.EventDialog;
        GameStateManager.Instance.SwitchState(GameState.Event);
    }

    /// <summary>
    /// HandleEndingNode ノードの処理を実行し、ダイアロググラフを設定してイベント状態に切り替えます。
    /// </summary>
    /// <param name="node">Scenario_EndingNode ノード</param>
    private void HandleEndingNode(Scenario_EndingNode node)
    {
        currentDialogGraph = node.EventDialog;
        GameStateManager.Instance.SwitchState(GameState.Event);
    }

    /// <summary>
    /// 指定されたポート名に基づいて、現在のシナリオノードから次のノードへ遷移します。
    /// </summary>
    /// <param name="portName">ポート名</param>
    private void NextNode(string portName)
    {
        if (ScenarioGraph.currentNode == null)
            return;

        foreach (var port in ScenarioGraph.currentNode.Ports)
        {
            if (port.fieldName == portName && port.Connection != null)
            {
                ScenarioGraph.currentNode = port.Connection.node as Scenario_BaseNode;
                break;
            }
        }
    }

    /// <summary>
    /// シナリオの状態をリセットし、ダイアロググラフをクリアします。
    /// </summary>
    public void ResetScenario()
    {
        foreach (var state in scenarioStates)
            state.ResetData();
        currentDialogGraph = null;

        foreach(var graph in dialogGraphs)
            graph.ResetData();
    }

    private void OnApplicationQuit()
    {
        ResetScenario();
        GameEventManager.StartListening(GameEventName.ResetGame, ResetScenarioAction);
        ResetScenarioAction -= ResetScenario;
    }
}
