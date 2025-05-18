using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ゲーム内のイベント名を定義する列挙型です。
/// </summary>
public enum GameEventName
{
    None,
    EnterLevel7BeforeGettingCandle,
    BattleWon,
    BattleLost,
    PuzzleCompleted,
    QRCodeReadSuccess,
    QRCodeReadFailure,
    ResetGame,
}

/// <summary>
/// ゲーム内のイベント管理を行うクラスです。
/// シングルトンパターンを採用しており、イベントの登録、解除、トリガーを管理します。
/// </summary>
public class GameEventManager : MonoBehaviour
{
    public static GameEventManager Instance { get; private set; }
    private Dictionary<GameEventName, UnityEvent> eventDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            eventDictionary = new Dictionary<GameEventName, UnityEvent>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 指定されたイベントにリスナーを登録します。
    /// </summary>
    /// <param name="eventName">登録するイベント名</param>
    /// <param name="listener">リスナーとなる UnityAction</param>
    public static void StartListening(GameEventName eventName, UnityAction listener)
    {
        if (Instance.eventDictionary.TryGetValue(eventName, out UnityEvent thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new UnityEvent();
            thisEvent.AddListener(listener);
            Instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    /// <summary>
    /// 指定されたイベントからリスナーを解除します。
    /// </summary>
    /// <param name="eventName">解除するイベント名</param>
    /// <param name="listener">解除する UnityAction</param>
    public static void StopListening(GameEventName eventName, UnityAction listener)
    {
        if (Instance == null) return;
        if (Instance.eventDictionary.TryGetValue(eventName, out UnityEvent thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    /// <summary>
    /// 指定されたイベントをトリガー（発火）します。
    /// </summary>
    /// <param name="eventName">トリガーするイベント名</param>
    public static void TriggerEvent(GameEventName eventName)
    {
        if (Instance.eventDictionary.TryGetValue(eventName, out UnityEvent thisEvent))
        {
            thisEvent.Invoke();
        }
    }
}
