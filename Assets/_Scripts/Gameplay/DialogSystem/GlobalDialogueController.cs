using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 対話 UI の種類を定義する列挙型です。
/// </summary>
public enum EventDialogUIType
{
    None,
    EventUI,
    ObjectUI,
    NpcUI
}

/// <summary>
/// イベント対話情報を保持する構造体です。
/// 対話グラフと、どの UI タイプで表示するかを指定します。
/// </summary>
[System.Serializable]
public struct EventDialog
{
    /// <summary>
    /// 対話グラフ（実際の対話内容やフローを定義）
    /// </summary>
    public DialogGraph dialogGraph;

    /// <summary>
    /// 対話 UI の種類を指定します。
    /// </summary>
    public EventDialogUIType dialogType;
}

/// <summary>
/// イベント名とそれに対応する対話情報を関連付けるマッピングクラスです。
/// 対話イベントの発生時に、どの対話グラフを表示するかを管理します。
/// </summary>
[System.Serializable]
public class EventDialogMapping
{
    /// <summary>
    /// 対応するイベント名または対話ID
    /// </summary>
    public GameEventName eventName;

    /// <summary>
    /// 対応するイベント対話情報
    /// </summary>
    public EventDialog eventDialog;
}

/// <summary>
/// GlobalDialogueController は、イベントと対話グラフのマッピングを管理し、対話をトリガーします。
/// </summary>
public class GlobalDialogueController : MonoBehaviour
{
    [Header("Dialogue Mappings (EventName <-> DialogGraph)")]
    [SerializeField] private List<EventDialogMapping> dialogMappings;
    private Dictionary<GameEventName, EventDialog> dialogDictionary;

    // 各イベントに対するリスナーを保持する辞書
    private Dictionary<GameEventName, UnityAction> eventListeners;

    private void Awake()
    {
        dialogDictionary = new Dictionary<GameEventName, EventDialog>();
        eventListeners = new Dictionary<GameEventName, UnityAction>();

        foreach (var mapping in dialogMappings)
        {
            if (mapping != null && !dialogDictionary.ContainsKey(mapping.eventName))
            {
                dialogDictionary.Add(mapping.eventName, mapping.eventDialog);
            }
            else
            {
                Debug.LogWarning("Duplicate or null dialogue mapping for event: " + mapping.eventName);
            }
        }
    }

    private void OnEnable()
    {
        foreach (var eventName in dialogDictionary.Keys)
        {
            UnityAction action = () => HandleDialogueTrigger(eventName);
            eventListeners[eventName] = action;
            GameEventManager.StartListening(eventName, action);
        }
    }

    private void OnDisable()
    {
        foreach (var kvp in eventListeners)
        {
            GameEventManager.StopListening(kvp.Key, kvp.Value);
        }
        eventListeners.Clear();
    }

    /// <summary>
    /// 指定されたイベントに対応する対話グラフを開始します。
    /// </summary>
    private void HandleDialogueTrigger(GameEventName dialogueEventName)
    {
        if (dialogDictionary.TryGetValue(dialogueEventName, out EventDialog eventDialog))
        {
            FreeRoam_DialogController.Instance.StartEventDialog(eventDialog.dialogGraph, eventDialog.dialogType);
        }
        else
        {
            Debug.LogWarning("対話グラフが見つかりません。イベント名: " + dialogueEventName);
        }
    }
}
