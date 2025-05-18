using EasyTransition;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// TransitionController クラスは、シーン遷移エフェクトの管理を行います。
/// EasyTransition ライブラリを利用して、トランジション効果の実行と関連イベントの通知を提供します。
/// </summary>
public class TransitionController : MonoBehaviour
{
    [SerializeField] List<TransitionData> transitionDatas;   // トランジションデータのリスト
    [SerializeField] float loadDelay = 0;                      // ロード遅延時間
    public TransitionManager manager;                         // トランジション管理クラス
    bool isTransitioning;                                     // 現在トランジション中かどうか
    public static TransitionController Instance { get; private set; }

    public event Action isOnTransitionStart;                  // トランジション開始時のイベント
    public event Action isOnTransitionEnd;                    // トランジション終了時のイベント

    /// <summary>
    /// Awake メソッド
    /// シングルトンインスタンスの設定を行います。
    /// </summary>
    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Start メソッド
    /// TransitionManager のイベントにリスナーを登録し、トランジション状態を管理します。
    /// </summary>
    public void Start()
    {
        manager.onTransitionBegin += () => {
            isTransitioning = true;
            //Debug.Log("Start isTransitioning " + isTransitioning);
        };

        manager.onTransitionEnd += () => {
            isTransitioning = false;
            //Debug.Log("Start onTransitionEnd " + isTransitioning);
        };
    }

    /// <summary>
    /// 指定されたトランジション名に基づいてトランジションを再生します。
    /// トランジション開始時と終了時に対応するイベントを発火します。
    /// </summary>
    /// <param name="transitionName">再生するトランジションの名前</param>
    /// <returns>トランジション再生のコルーチン</returns>
    public IEnumerator PlayTransition(TransitionData.TransitionName transitionName)
    {
        if (!isTransitioning)
        {
            TransitionData data = transitionDatas.Find(data => data.transitionName == transitionName);
            isOnTransitionStart?.Invoke();
            manager.Transition(data.transitionSetting, loadDelay);
            // 指定されたトランジション時間だけ待機
            yield return new WaitForSeconds(data.transitionSetting.transitionTime);
            isOnTransitionEnd?.Invoke();
        }
    }

    /// <summary>
    /// 指定されたトランジション名に対応するトランジションの時間を返します。
    /// </summary>
    /// <param name="transitionName">トランジションの名前</param>
    /// <returns>トランジションの時間</returns>
    public float GetTransitionTime(TransitionData.TransitionName transitionName)
    {
        TransitionData data = transitionDatas.Find(data => data.transitionName == transitionName);
        return data.transitionSetting.transitionTime;
    }
}

/// <summary>
/// TransitionData クラスは、トランジションに関する設定情報を保持します。
/// </summary>
[System.Serializable]
public class TransitionData
{
    public enum TransitionName
    {
        Fade,
        RectangleGrid,
        RectangleWipe,
        VerticalCurtain,
        Noise
    }
    public TransitionName transitionName;           // トランジションの名前
    public TransitionSettings transitionSetting;    // トランジションの設定情報
}
