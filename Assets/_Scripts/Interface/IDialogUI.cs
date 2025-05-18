using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ダイアログ UI の初期化、表示、非表示などの操作を定義するインターフェイスです。
/// </summary>
public interface IDialogUI
{
    /// <summary>
    /// UI の初期化（例：ダイアログのオープン、状態のリセット）を行います。
    /// </summary>
    void InitUI();

    /// <summary>
    /// 指定された話者の表示（立ち絵や位置調整が必要な場合に実装）を行います。
    /// </summary>
    /// <param name="speaker">表示対象の話者</param>
    void ShowSpeaker(SpeakerName speaker);

    /// <summary>
    /// 指定された話者の非表示を行います。
    /// </summary>
    /// <param name="speaker">非表示にする話者</param>
    void HideSpeaker(SpeakerName speaker);

    /// <summary>
    /// CG（コンピュータグラフィックス）を表示します。
    /// </summary>
    /// <param name="newSprite">表示するスプライト</param>
    void ShowCg(Sprite newSprite);

    /// <summary>
    /// CG を非表示にします。
    /// </summary>
    void HideCg();

    /// <summary>
    /// 指定された話者が話しているアニメーション状態に設定します。
    /// </summary>
    /// <param name="speaker">対象の話者</param>
    /// <param name="isTalking">話しているかどうか</param>
    void SetSpeakerTalking(SpeakerName speaker, bool isTalking);

    /// <summary>
    /// 話者の名前を設定します。
    /// </summary>
    /// <param name="speaker">対象の話者</param>
    void SetSpeakerName(SpeakerName speaker);

    /// <summary>
    /// UI 上に対話テキストを表示します。
    /// </summary>
    /// <param name="text">表示するテキスト</param>
    void ShowLineText(string text);

    /// <summary>
    /// 対話テキストをクリアします。
    /// </summary>
    void ClearLineText();

    /// <summary>
    /// 分岐選択肢（文字列リスト）を表示します。
    /// </summary>
    /// <param name="options">選択肢リスト</param>
    void ShowOptions(List<string> options);

    /// <summary>
    /// 選択中の選択肢のテキストを更新します。
    /// </summary>
    /// <param name="selectedOptionIndex">選択中のインデックス</param>
    void UpdateOptionText(int selectedOptionIndex);

    /// <summary>
    /// 分岐選択肢を非表示にします。
    /// </summary>
    void HideOptions();

    /// <summary>
    /// 対話終了時の後処理（UI のクローズなど）を実行します。
    /// </summary>
    void OnDialogEnd();
}
