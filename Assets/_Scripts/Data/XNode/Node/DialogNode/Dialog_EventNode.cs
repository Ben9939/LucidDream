using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ダイアログのイベントノードです。
/// 入力ポート "entry" と出力ポート "exit" を持ち、選択時に UnityEvent を実行します。
/// </summary>
public class Dialog_EventNode : Dialog_BaseNode
{
    [Input] public int entry;
    [Output] public int exit;

    /// <summary>
    /// このノードが実行するイベントです。
    /// </summary>
    public UnityEvent Event;

    /// <summary>
    /// UnityEvent を返します。
    /// </summary>
    public UnityEvent GetUnityEvent()
    {
        return Event;
    }

    // 必要に応じて GetType() のオーバーライドを追加してください。
}

