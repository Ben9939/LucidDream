using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// BGMデータを定義するクラス。
/// </summary>
[System.Serializable]
public class BGMSoundData
{
    /// <summary>
    /// BGM の種類を表す列挙型。
    /// </summary>
    public enum BGM
    {
        None,
        Nose,
        Title,
        Intro,
        Battle,
        Scene1,
        Scene2,
        Scene3,
        Scene4,
        Scene5,
        Scene6,
        Scene7,
        Scene8,
        Puzzle,
        Ending,
        Tutorial
    }

    /// <summary>
    /// 再生する BGM の種類。
    /// </summary>
    public BGM bgm;

    /// <summary>
    /// 再生するオーディオクリップ。
    /// </summary>
    public AudioClip audioClip;

    /// <summary>
    /// BGM 個別の音量（0～1）。
    /// </summary>
    [Range(0, 1)]
    public float volume = 1;
}

/// <summary>
/// SEデータを定義するクラス。
/// </summary>
[System.Serializable]
public class SESoundData
{
    /// <summary>
    /// SE の種類を表す列挙型。
    /// </summary>
    public enum SE
    {
        None,
        OpenCease,
        CloseCease,
        SetGame,
        SwitchOn,
        Start,
        Move,
        Sleep,
        OpenDoor,
        CloseDoor,
        Cursor,
        Decide,
        Speak,
        Attack_Lv1,
        Attack_Lv2,
        Attack_Lv3,
        Damage_Lv1,
        Damage_Lv2,
        Damage_Lv3,
        Healing_Lv1,
        Healing_Lv2,
        IncreaseMana_Lv1,
        IncreaseMana_Lv2,
        BattleResult_Win,
        BattleResult_Lose,
        GetPiece,
        PlacePiece,
        Mechanism,
        EnterScene,
    }

    /// <summary>
    /// 再生する SE の種類。
    /// </summary>
    public SE se;

    /// <summary>
    /// 再生するオーディオクリップ。
    /// </summary>
    public AudioClip audioClip;

    /// <summary>
    /// SE 個別の音量（0～1）。
    /// </summary>
    [Range(0, 1)]
    public float volume = 1;
}
/// <summary>
/// AudioManager クラスは、ゲーム内のオーディオ管理を担当するシングルトンパターンのクラスです。
/// このクラスは、BGM（バックグラウンドミュージック）と SE（サウンドエフェクト）の再生、停止、フェード、音量調整を行います。
/// </summary>
public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioSettingsSO audioSettings;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmAudioSource; // BGM用AudioSource
    [SerializeField] private AudioSource seAudioSource;  // SE用AudioSource

    [Header("BGM Data")]
    [SerializeField] private List<BGMSoundData> bgmSoundDatas;
    [Header("SE Data")]
    [SerializeField] private List<SESoundData> seSoundDatas;

    [Header("Volume Settings")]
    [Range(0, 1)] public float masterVolume = 1;      // マスターボリューム
    [Range(0, 1)] public float bgmMasterVolume = 1;     // BGM用ボリューム
    [Range(0, 1)] public float seMasterVolume = 1;      // SE用ボリューム

    [SerializeField] private GameObject VolumeText;     // 音量表示用テキスト

    // 音量バー表示用のコルーチンを追跡する変数
    private Coroutine volumeTextCoroutine;

    // 現在再生中のBGMの元のボリューム（基準値）を保持する変数
    private float currentBGMBaseVolume = 1f;

    // 外部から現在再生中のBGMを取得するためのプロパティ
    public BGMSoundData.BGM CurrentBGM { get; private set; } = BGMSoundData.BGM.None;
    // 外部から最後に再生されたSEを取得するためのプロパティ
    public SESoundData.SE LastPlayedSE { get; private set; } = SESoundData.SE.None;

    private void OnEnable()
    {
        audioSettings.OnMasterVolumeChange += AdjustMasterVolume;
        audioSettings.OnBgmVolumeChange += AdjustBGMVolume;
        audioSettings.OnSeVolumeChange += AdjustSEVolume;
    }

    /// <summary>
    /// 指定したテキストを表示し、一定時間後に非表示にするコルーチン
    /// </summary>
    private IEnumerator ShowVolumeText(string text)
    {
        VolumeText.GetComponent<Text>().text = text;
        VolumeText.SetActive(true);
        yield return new WaitForSeconds(2f);
        VolumeText.SetActive(false);
    }

    /// <summary>
    /// 指定した音量タイプのバーを表示する（最大15ユニット）
    /// </summary>
    private void DisplayVolumeBar(string volumeType, float volumeValue)
    {
        volumeValue = Mathf.Clamp01(volumeValue);
        int barCount = Mathf.FloorToInt(volumeValue * 15);
        string volumeBar = new string('|', barCount) + new string('-', 15 - barCount);
        string text = $"{volumeType} Volume: {volumeBar}";

        if (volumeTextCoroutine != null)
        {
            StopCoroutine(volumeTextCoroutine);
        }
        volumeTextCoroutine = StartCoroutine(ShowVolumeText(text));
    }

    /// <summary>
    /// マスターボリュームを調整して反映する
    /// </summary>
    public void AdjustMasterVolume(float value)
    {
        masterVolume = value;
        ApplyVolumeChanges();
        DisplayVolumeBar("Master", masterVolume);
    }

    /// <summary>
    /// BGM用ボリュームを調整して反映する
    /// </summary>
    public void AdjustBGMVolume(float value)
    {
        bgmMasterVolume = value;
        ApplyVolumeChanges();
        DisplayVolumeBar("BGM", bgmMasterVolume);
    }

    /// <summary>
    /// SE用ボリュームを調整して反映する
    /// </summary>
    public void AdjustSEVolume(float value)
    {
        seMasterVolume = value;
        ApplyVolumeChanges();
        DisplayVolumeBar("SE", seMasterVolume);
    }

    /// <summary>
    /// BGMとSEのボリューム設定を更新する（BGMの場合は元のボリュームを基準に計算）
    /// </summary>
    private void ApplyVolumeChanges()
    {
        if (bgmAudioSource != null && bgmAudioSource.clip != null)
        {
            bgmAudioSource.volume = currentBGMBaseVolume * bgmMasterVolume * masterVolume;
        }
        // SEはPlayOneShotで再生しているため、ここでの更新は不要
    }

    /// <summary>
    /// 指定されたBGMを再生し、CurrentBGMプロパティを更新する
    /// </summary>
    public void PlayBGM(BGMSoundData.BGM bgm)
    {
        BGMSoundData data = bgmSoundDatas.Find(data => data.bgm == bgm);
        if (data == null || data.audioClip == null) return;
        CurrentBGM = bgm;
        currentBGMBaseVolume = data.volume; // 基準となるボリュームを保存
        bgmAudioSource.clip = data.audioClip;
        bgmAudioSource.volume = currentBGMBaseVolume * bgmMasterVolume * masterVolume;
        bgmAudioSource.Play();
    }

    /// <summary>
    /// BGMをフェードアウトしながら停止する
    /// </summary>
    public void FadeOutBGM(float fadeDuration)
    {
        if (bgmAudioSource == null || !bgmAudioSource.isPlaying)
        {
            Debug.LogWarning("現在再生中のBGMはありません");
            return;
        }
        StartCoroutine(FadeOutBGMCoroutine(fadeDuration));
    }

    private IEnumerator FadeOutBGMCoroutine(float fadeDuration)
    {
        float startVolume = bgmAudioSource.volume;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            bgmAudioSource.volume = Mathf.Lerp(startVolume, 0, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        bgmAudioSource.volume = 0;
        bgmAudioSource.Stop();
        CurrentBGM = BGMSoundData.BGM.None;
    }

    /// <summary>
    /// 指定されたBGMをフェードインしながら再生する
    /// </summary>
    public void FadeInBGM(BGMSoundData.BGM bgm, float fadeDuration)
    {
        StartCoroutine(FadeInBGMChain(bgm, fadeDuration));
    }

    private IEnumerator FadeInBGMChain(BGMSoundData.BGM bgm, float fadeDuration)
    {
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            yield return StartCoroutine(FadeOutBGMCoroutine(fadeDuration));
        }
        BGMSoundData data = bgmSoundDatas.Find(d => d.bgm == bgm);
        if (data == null || data.audioClip == null)
        {
            Debug.LogWarning("指定されたBGMが存在しないか、AudioClipがありません");
            yield break;
        }
        CurrentBGM = bgm;
        currentBGMBaseVolume = data.volume;
        bgmAudioSource.clip = data.audioClip;
        bgmAudioSource.volume = 0;
        bgmAudioSource.Play();
        float targetVolume = currentBGMBaseVolume * bgmMasterVolume * masterVolume;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            bgmAudioSource.volume = Mathf.Lerp(0, targetVolume, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        bgmAudioSource.volume = targetVolume;
    }

    /// <summary>
    /// 現在再生中のBGMを停止する
    /// </summary>
    public void StopBGM()
    {
        bgmAudioSource.Stop();
        CurrentBGM = BGMSoundData.BGM.None;
    }

    /// <summary>
    /// 指定されたSEを再生し、LastPlayedSEプロパティを更新する
    /// </summary>
    public void PlaySE(SESoundData.SE se)
    {
        SESoundData data = seSoundDatas.Find(data => data.se == se);
        if (data == null || data.audioClip == null) return;
        LastPlayedSE = se;
        seAudioSource.volume = data.volume * seMasterVolume * masterVolume;
        seAudioSource.PlayOneShot(data.audioClip);
    }

    /// <summary>
    /// 現在再生中のSEを停止する
    /// </summary>
    public void StopSE()
    {
        seAudioSource.Stop();
    }

    /// <summary>
    /// 指定されたBGMのクリップ長を取得する
    /// </summary>
    public float GetBGMClipLength(BGMSoundData.BGM bgm)
    {
        BGMSoundData data = bgmSoundDatas.Find(d => d.bgm == bgm);
        return data != null && data.audioClip != null ? data.audioClip.length : 0f;
    }

    /// <summary>
    /// 指定されたSEのクリップ長を取得する
    /// </summary>
    public float GetSEClipLength(SESoundData.SE se)
    {
        SESoundData data = seSoundDatas.Find(d => d.se == se);
        return data != null && data.audioClip != null ? data.audioClip.length : 0f;
    }

    private void OnApplicationQuit()
    {
        audioSettings.ResetData();
    }

    // 外部からBGMとSEのAudioSourceを取得できるようにする
    public AudioSource GetBGMSource() => bgmAudioSource;
    public AudioSource GetSESource() => seAudioSource;
}
