using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using XNode;

/// <summary>
/// ダイアログ分岐ノードです。<br/>
/// 入力ポート "entry" と、動的な出力ポートリスト "options"（OptionData のリスト）を持ち、
/// 各オプションの条件に基づいて次のノードを選択します。
/// </summary>
[NodeWidth(350)]
public class Dialog_BranchNode : Dialog_BaseNode
{
    [Input] public int entry;
    // OptionData のリストを利用して分岐オプションを管理
    [Output(dynamicPortList = true)] public List<OptionData> options;

    /// <summary>
    /// ノードの種類を Branch として返します。
    /// </summary>
    public override DialogNodeType GetType() => DialogNodeType.Branch;


    /// <summary>
    /// すべての選択肢のテキストを取得し、リストとして返します。
    /// </summary>
    public List<string> GetOptionTexts()
    {
        List<string> texts = new List<string>();
        if (options != null)
        {
            foreach (OptionData option in options)
            {
                texts.Add(option.optionText);
            }
        }
        return texts;
    }

    /// <summary>
    /// 指定されたインデックスの選択肢に対応する UnityEvent を取得します。
    /// </summary>
    /// <param name="index">選択肢のインデックス</param>
    /// <returns>対応する UnityEvent。インデックスが無効な場合は null を返します。</returns>
    public UnityEvent GetOptionEvent(int index)
    {
        if (options != null && index >= 0 && index < options.Count)
        {
            return options[index].onSelected;
        }
        return null;
    }

    /// <summary>
    /// 指定された選択肢のインデックスに基づき、次に接続するノードのフィールド名を取得します。<br/>
    /// （イベント駆動の場合は、イベント内で処理されることが多いため、必要に応じて実装してください。）
    /// </summary>
    /// <param name="index">選択肢のインデックス</param>
    /// <returns>次ノードに接続するためのフィールド名。無効な場合は null。</returns>
    public string GetNextNode(int index)
    {
        if (index < 0 || options == null || index >= options.Count)
        {
            return null;
        }
        return "options " + index.ToString();
    }
}
