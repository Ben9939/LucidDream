using System;

/// <summary>
/// スクリプトのロード処理を管理するインターフェイスです。
/// </summary>
public interface IScriptLoader
{
    /// <summary>
    /// ロード完了時に発火するイベント。
    /// </summary>
    event Action OnLoaded;

    /// <summary>
    /// スクリプトのロードを開始します。
    /// </summary>
    void StartLoading();

    /// <summary>
    /// スクリプトのロードを停止します。
    /// </summary>
    void StopLoading();

    /// <summary>
    /// ロード進捗の更新処理を実行します。
    /// </summary>
    void UpdateHandle();
}
