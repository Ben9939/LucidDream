using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ダイアログオプションのデータクラスです。
/// 選択肢のテキストと、選択時に実行される UnityEvent を保持します。
/// </summary>
[System.Serializable]
public class OptionData
{
    [Header("Option Text")]
    public string optionText;      // 選択肢のテキスト

    [Header("On Selected Event")]
    public UnityEvent onSelected;  // 選択時に実行されるイベント
}
