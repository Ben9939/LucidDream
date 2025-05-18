using UnityEngine;

/// <summary>
/// 対話の発話者を定義する列挙型です。
/// </summary>
public enum SpeakerName
{
    None,
    Player,
    Ghost,
    OneEye,
    Slime,
}

/// <summary>
/// ダイアログのラインノードです。
/// 入力ポート "entry" と出力ポート "exit" を持ち、対話内容（テキスト、画像など）および発話者情報を保持します。
/// </summary>
public class Dialog_LineNode : Dialog_BaseNode
{
    [Input] public int entry;
    [Output] public int exit;

    /// <summary>
    /// 発話者を指定します。Player, Ghost, OneEye, Slime など。
    /// Player または None の場合、テキストのみで処理されます。
    /// </summary>
    public SpeakerName speaker;
    /// <summary>
    /// 表示する対話テキストです。
    /// </summary>
    [TextArea]
    public string dialogLine;
    /// <summary>
    /// 対話中に表示する画像（任意）です。
    /// </summary>
    public Sprite CG = null;

    /// <summary>
    /// 発話者情報を返します。
    /// </summary>
    public SpeakerName GetSpeaker() => speaker;

    /// <summary>
    /// 対話テキストを返します。
    /// </summary>
    public string GetDialogLine() => dialogLine;

    /// <summary>
    /// 表示する画像 (CG) を返します。
    /// </summary>
    public Sprite GetCG() => CG;
}

