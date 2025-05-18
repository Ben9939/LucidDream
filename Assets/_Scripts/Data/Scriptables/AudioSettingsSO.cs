using System;
using UnityEngine;

/// <summary>
/// オーディオ設定を管理するScriptableObjectです。
/// マスタ、BGM、SEの各ボリュームを管理し、変更時に対応するイベントを発火します。
/// </summary>
[CreateAssetMenu(menuName = "Audio/AudioSettings")]
public class AudioSettingsSO : ScriptableObject
{
    [Range(0, 1)]
    [SerializeField] private float masterVolume = 1f;
    [Range(0, 1)]
    [SerializeField] private float bgmVolume = 1f;
    [Range(0, 1)]
    [SerializeField] private float seVolume = 1f;

    public event Action<float> OnMasterVolumeChange;
    public event Action<float> OnBgmVolumeChange;
    public event Action<float> OnSeVolumeChange;

    /// <summary>
    /// マスタボリュームの取得・設定。
    /// 設定時にOnMasterVolumeChangeイベントが発火します。
    /// </summary>
    public float MasterVolume
    {
        get => masterVolume;
        set
        {
            masterVolume = value;
            OnMasterVolumeChange?.Invoke(masterVolume);
        }
    }

    /// <summary>
    /// BGMボリュームの取得・設定。
    /// 設定時にOnBgmVolumeChangeイベントが発火します。
    /// </summary>
    public float BgmVolume
    {
        get => bgmVolume;
        set
        {
            bgmVolume = value;
            OnBgmVolumeChange?.Invoke(bgmVolume);
        }
    }

    /// <summary>
    /// SEボリュームの取得・設定。
    /// 設定時にOnSeVolumeChangeイベントが発火します。
    /// </summary>
    public float SeVolume
    {
        get => seVolume;
        set
        {
            seVolume = value;
            OnSeVolumeChange?.Invoke(seVolume);
        }
    }

    public void ResetData()
    {
        masterVolume = 1f;
        bgmVolume = 1f;
        seVolume = 1f;
    }
}
