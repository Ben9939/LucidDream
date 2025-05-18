using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// オーディオ設定を更新するためのスライダー処理を管理するクラス
/// </summary>
public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private AudioSettingsSO audioSettings;

    /// <summary>
    /// マスターボリュームを設定する
    /// </summary>
    public void SetMasterVolume(float value)
    {
        audioSettings.MasterVolume = value;
    }

    /// <summary>
    /// BGMボリュームを設定する
    /// </summary>
    public void SetBGMVolume(float value)
    {
        audioSettings.BgmVolume = value;
    }

    /// <summary>
    /// SEボリュームを設定する
    /// </summary>
    public void SetSEVolume(float value)
    {
        audioSettings.SeVolume = value;
    }
}