using UnityEngine;
using XNode;

/// <summary>
/// ScenarioNodeType 列挙型は、シナリオノードの種類を定義します。
/// ノードのタイプに応じた処理分岐に利用します。
/// </summary>
public enum ScenarioNodeType
{
    None,
    Start,
    Enter,
    CutIn,
    Condition,
    SceneChange,
    Branch,
    Event,
    Ending
}

/// <summary>
/// Scenario_BaseNode は、すべてのシナリオノードの基底クラスです。
/// 各ノードはこのクラスを継承し、GUID の初期化などの共通処理を行います。
/// </summary>
public class Scenario_BaseNode : Node
{
    [HideInInspector]
    public string guid;

    protected override void Init()
    {
        base.Init();
        if (string.IsNullOrEmpty(guid))
        {
            guid = System.Guid.NewGuid().ToString();
        }
    }
}

