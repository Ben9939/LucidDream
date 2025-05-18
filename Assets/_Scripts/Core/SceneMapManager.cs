using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static SceneMapManager;
/// <summary>
/// シーンマップの管理を行うクラスです。
/// シーンの切り替え、プレイヤーの位置更新、シーン入場イベントの実行などを制御します。
/// </summary>
public class SceneMapManager : MonoBehaviour
{
    public enum SceneBoundaryName
    {
        Level1_1,
        Level1_2,
        Level2_1,
        Level2_2,
        Level3_1,
        Level3_2,
        Level4_1,
        Level4_2,
        Level4_3,
        Level4_4,
        Level5_1,
        Level6_1,
        Level7_1,
        Level7_2,
        Level7_3,
        None,
    }

    [Serializable]
    public struct EnterSceneEvent
    {
        public GameEventName EventName;

        [SerializeField]
        private ScenarioStateSO requiredScenarioState;  // 必要なシナリオ状態

        [SerializeField]
        private bool requiredStateValue;  // 必要な状態の値

        // 必要な状態が満たされているかを判定する
        public bool Condition => requiredScenarioState && requiredScenarioState.Condition == requiredStateValue;
    }

    [Serializable]
    public struct SceneBoundary
    {
        public SceneBoundaryName SceneBoundaryName;   // 境界の名称
        public GameObject Boundary;                     // プレイヤー初期位置設定用の境界オブジェクト
        public GameObject Scene;                        // 関連するシーンのオブジェクト
        public bool IsOneWay;                           // 一方向のみの境界かどうか
        public BGMSoundData.BGM Bgm;                    // 関連するBGM
        public ColorMode ColorMode;                     // 色設定
        public EnterSceneEvent EnterSceneEvent;         // シーン入場時のイベント
    }

    [Serializable]
    public struct SceneBoundaryPair
    {
        public SceneBoundary BoundaryA; 
        public SceneBoundary BoundaryB; 
    }

    [SerializeField] private List<SceneBoundaryPair> boundaryPairs = new List<SceneBoundaryPair>();
    private Dictionary<GameObject, SceneBoundary> boundaryMap = new Dictionary<GameObject, SceneBoundary>();

    public static SceneMapManager Instance { get; private set; }
    public static SceneBoundaryName CurrentBoundary = SceneBoundaryName.None;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeBoundaryMap();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 境界の対応表を初期化する
    /// </summary>
    private void InitializeBoundaryMap()
    {
        foreach (var pair in boundaryPairs)
        {
            if (pair.BoundaryA.Boundary != null && pair.BoundaryB.Boundary != null)
            {
                boundaryMap[pair.BoundaryA.Boundary] = pair.BoundaryB;
                boundaryMap[pair.BoundaryB.Boundary] = pair.BoundaryA;
            }
        }
    }

    private SceneBoundary SceneBoundaryNameTOSceneBoundary(SceneBoundaryName sceneBoundaryName)
    {
        foreach (var pair in boundaryPairs)
        {
            if (pair.BoundaryA.SceneBoundaryName == sceneBoundaryName)
            {
                return pair.BoundaryA;
            }
            if (pair.BoundaryB.SceneBoundaryName == sceneBoundaryName)
            {
                return pair.BoundaryB;
            }
        }
        return new SceneBoundary();
    }

    /// <summary>
    /// 自由移動シーンを設定する
    /// </summary>
    public void SetFreeRoamScene()
    {
        var playerData = PlayerController.Instance.playerData;
        // プレイヤーデータに有効な境界が設定されているか確認する
        if (!Enum.IsDefined(typeof(SceneBoundaryName), playerData.CurrentBoundary))
        {
            Debug.LogWarning("No valid currentBoundary found in PlayerData. Cannot restore scene!");
            return;
        }

        Debug.Log("Comparing CurrentBoundary (" + CurrentBoundary.ToString() + ") with PlayerData.CurrentBoundary (" + playerData.CurrentBoundary.ToString() + ")");

        // 内部のCurrentBoundaryとPlayerDataのCurrentBoundaryが異なる場合、新しいシーンを起動する
        if (!CurrentBoundary.Equals(playerData.CurrentBoundary))
        {
            SetFreeRoamScene(playerData.CurrentBoundary);
            CurrentBoundary = playerData.CurrentBoundary;
            SceneBoundary targetBoundary = SceneBoundaryNameTOSceneBoundary(playerData.CurrentBoundary);

            if (targetBoundary.Bgm != BGMSoundData.BGM.None)
            {
                AudioManager.Instance.PlayBGM(targetBoundary.Bgm);
            }
            else
            {
                AudioManager.Instance.StopBGM();
            }

        }
        else
        {
            // 同じ場合は以前の状態に戻す：関連するシーンを有効にし、プレイヤー位置を復元する
            Debug.Log("Restoring previous state: activating associated scene and resetting player position.");
            SceneBoundary targetBoundary = SceneBoundaryNameTOSceneBoundary(playerData.CurrentBoundary);
            if (targetBoundary.Scene == null)
            {
                Debug.LogWarning("No scene associated with " + playerData.CurrentBoundary.ToString() + " found. Cannot restore state!");
                return;
            }
            PlayerController.Instance.SetColor(targetBoundary.ColorMode);
            targetBoundary.Scene.SetActive(true);

            if (targetBoundary.Bgm != BGMSoundData.BGM.None)
            {
                AudioManager.Instance.PlayBGM(targetBoundary.Bgm);
            }
            else
            {
                AudioManager.Instance.StopBGM();
            }
            if (targetBoundary.EnterSceneEvent.Condition)
            {
                GameEventManager.TriggerEvent(targetBoundary.EnterSceneEvent.EventName);
            }
        }
    }

    /// <summary>
    /// 指定した境界名に基づいてシーンを切り替え、PlayerDataと内部のCurrentBoundaryを更新する
    /// </summary>
    /// <param name="targetBoundaryName">Target boundary name</param>
    public void SetFreeRoamScene(SceneBoundaryName targetBoundaryName)
    {
        SceneBoundary targetBoundary = new SceneBoundary();
        bool found = false;
        foreach (var pair in boundaryPairs)
        {
            if (pair.BoundaryA.SceneBoundaryName == targetBoundaryName)
            {
                targetBoundary = pair.BoundaryA;
                found = true;
                break;
            }
            if (pair.BoundaryB.SceneBoundaryName == targetBoundaryName)
            {
                targetBoundary = pair.BoundaryB;
                found = true;
                break;
            }
        }
        if (!found)
        {
            Debug.LogError("Boundary named " + targetBoundaryName.ToString() + " not found!");
            return;
        }

        if (targetBoundary.Scene == null)
        {
            Debug.LogError("Boundary " + targetBoundaryName.ToString() + " does not have an associated scene!");
            return;
        }

        var playerData = PlayerController.Instance.playerData;
        // 既に初期化された境界がある場合、古いシーンを無効化する
        SceneBoundary oldBoundary = FindBoundaryByEnum(CurrentBoundary, out _);
        if (oldBoundary.Scene != null)
        {
            oldBoundary.Scene.SetActive(false);
        }

        // 新しい境界に関連するシーンを有効化し、PlayerDataと内部のCurrentBoundaryを更新する
        targetBoundary.Scene.SetActive(true);
        PlayerController.Instance.SetColor(targetBoundary.ColorMode);
        playerData.CurrentBoundary = targetBoundaryName;
        CurrentBoundary = targetBoundaryName;

        // 対象境界上のTilemapCollider2Dからプレイヤーの初期位置を設定する
        TilemapCollider2D tilemapCollider = targetBoundary.Boundary.GetComponent<TilemapCollider2D>();
        if (tilemapCollider != null)
        {
            Vector3 newPosition = tilemapCollider.bounds.center;
            PlayerController.Instance.SetPlayerPosition(newPosition);
            playerData.CurrentPosition = newPosition;
        }
        else
        {
            Debug.LogWarning("TilemapCollider2D not found on boundary " + targetBoundaryName.ToString() + ". Cannot update player position!");
        }
        if (targetBoundary.EnterSceneEvent.Condition)
        {
            GameEventManager.TriggerEvent(targetBoundary.EnterSceneEvent.EventName);
        }
    }

    /// <summary>
    /// トランジション効果でシーン切り替えとプレイヤー位置更新を行う
    /// </summary>
    /// <param name="sceneBoundaryName">切り替え対象の境界名(SceneBoundaryName)</param>
    public void SceneSwitchEvent(SceneBoundaryName sceneBoundaryName)
    {
        StartCoroutine(SceneSwitch(sceneBoundaryName));
    }

    /// <summary>
    /// トランジション効果でシーン切り替えとプレイヤー位置更新を行う
    /// </summary>
    /// <param name="sceneBoundaryName">切り替え対象の境界名(string)</param>
    public void SceneSwitchEvent(string sceneBoundaryName)
    {
        StartCoroutine(SceneSwitch(sceneBoundaryName));
    }

    /// <summary>
    /// 境界のGameObject名を使用してシーンを切り替える（トランジション効果あり）
    /// </summary>
    /// <param name="sceneBoundaryName">Target boundary GameObject name</param>
    public IEnumerator SceneSwitch(string sceneBoundaryName)
    {
        // シーン切り替え前にプレイヤーの操作を無効化
        PlayerController.Instance.DisablePlayerControl();
        var currentBoundary = FindBoundaryByName(sceneBoundaryName, out SceneBoundary targetBoundary);
        if (targetBoundary.Boundary != null)
        {
            yield return StartCoroutine(SwitchSceneCoroutine(currentBoundary, targetBoundary));
            var playerData = PlayerController.Instance.playerData;
            playerData.CurrentBoundary = targetBoundary.SceneBoundaryName;
            CurrentBoundary = targetBoundary.SceneBoundaryName;
        }
        else
        {
            Debug.LogWarning("Scene boundary '" + sceneBoundaryName + "' not found!");
        }
        // シーン切り替え完了後にプレイヤーの操作を有効化
        PlayerController.Instance.EnablePlayerControl();
    }

    /// <summary>
    /// SceneBoundaryNameを使用してシーンを切り替える（トランジション効果あり）
    /// </summary>
    /// <param name="sceneBoundaryName">Target boundary name</param>
    public IEnumerator SceneSwitch(SceneBoundaryName sceneBoundaryName)
    {
        SceneBoundary currentBoundary;
        var targetBoundary = FindBoundaryByEnum(sceneBoundaryName, out currentBoundary);
        if (targetBoundary.Boundary != null)
        {
            yield return StartCoroutine(SwitchSceneCoroutine(currentBoundary, targetBoundary));
            var playerData = PlayerController.Instance.playerData;
            playerData.CurrentBoundary = targetBoundary.SceneBoundaryName;
            CurrentBoundary = targetBoundary.SceneBoundaryName;
        }
        else
        {
            Debug.LogWarning("Scene boundary '" + sceneBoundaryName.ToString() + "' not found!");
        }
    }

    /// <summary>
    /// 境界のGameObject名から対応するSceneBoundaryを検索する
    /// </summary>
    /// <param name="sceneBoundaryName">境界のGameObject名</param>
    /// <param name="currentBoundary">対応するもう一方の境界</param>
    private SceneBoundary FindBoundaryByName(string sceneBoundaryName, out SceneBoundary currentBoundary)
    {
        foreach (var pair in boundaryPairs)
        {
            if (pair.BoundaryA.Boundary.name == sceneBoundaryName)
            {
                currentBoundary = pair.BoundaryB;
                return pair.BoundaryA;
            }
            if (pair.BoundaryB.Boundary.name == sceneBoundaryName)
            {
                currentBoundary = pair.BoundaryA;
                return pair.BoundaryB;
            }
        }
        currentBoundary = new SceneBoundary();
        return new SceneBoundary();
    }

    /// <summary>
    /// SceneBoundaryNameのEnumを使って対応するSceneBoundaryを検索する
    /// </summary>
    /// <param name="sceneBoundaryName">検索する境界名</param>
    /// <param name="currentBoundary">対応するもう一方の境界</param>
    private SceneBoundary FindBoundaryByEnum(SceneBoundaryName sceneBoundaryName, out SceneBoundary currentBoundary)
    {
        foreach (var pair in boundaryPairs)
        {
            if (pair.BoundaryA.SceneBoundaryName == sceneBoundaryName)
            {
                currentBoundary = pair.BoundaryA;
                return pair.BoundaryB;
            }
            if (pair.BoundaryB.SceneBoundaryName == sceneBoundaryName)
            {
                currentBoundary = pair.BoundaryB;
                return pair.BoundaryA;
            }
        }
        currentBoundary = new SceneBoundary();
        return new SceneBoundary();
    }

    /// <summary>
    /// シーン切り替えとプレイヤー位置更新の共通処理をまとめる
    /// 以下の処理を実施する：
    /// 1. 現在のシーンを無効化する（存在すれば）
    /// 2. 目標シーンを有効化し、PlayerDataのCurrentBoundaryを更新する
    /// 3. 目標境界上のTilemapCollider2Dからプレイヤー位置を更新する
    /// </summary>
    private void ApplySceneAndPlayerSettings(SceneBoundary currentBoundary, SceneBoundary targetBoundary)
    {
        var playerData = PlayerController.Instance.playerData;
        if (currentBoundary.Scene != null)
        {
            currentBoundary.Scene.SetActive(false);
        }
        targetBoundary.Scene.SetActive(true);
        // PlayerDataのCurrentBoundaryを更新する
        playerData.CurrentBoundary = currentBoundary.SceneBoundaryName;

        var collider = targetBoundary.Boundary.GetComponent<TilemapCollider2D>();
        if (collider != null)
        {
            Vector3 newPosition = collider.bounds.center;
            PlayerController.Instance.SetPlayerPosition(newPosition);
            playerData.CurrentPosition = newPosition;
        }
        else
        {
            Debug.LogWarning("TilemapCollider2D not found on '" + targetBoundary.Boundary.name + "'!");
        }
        if (targetBoundary.EnterSceneEvent.Condition)
        {
            GameEventManager.TriggerEvent(targetBoundary.EnterSceneEvent.EventName);
        }
    }

    /// <summary>
    /// トランジション付きのシーン切り替えを行うコルーチン
    /// </summary>
    private IEnumerator SwitchSceneCoroutine(SceneBoundary currentBoundary, SceneBoundary targetBoundary)
    {
        if (targetBoundary.IsOneWay)
        {
            Debug.LogWarning("Cannot enter '" + targetBoundary.Boundary.name + "' because it is a one-way boundary!");
            yield break;
        }
        AudioManager.Instance.PlaySE(SESoundData.SE.EnterScene);
        yield return StartCoroutine(TransitionController.Instance.PlayTransition(TransitionData.TransitionName.Fade));

        // シーン切り替えとプレイヤー位置更新を実行し、PlayerDataのCurrentBoundaryを更新する
        ApplySceneAndPlayerSettings(currentBoundary, targetBoundary);
        
        if (targetBoundary.Bgm != BGMSoundData.BGM.None)
        {
             AudioManager.Instance.PlayBGM(targetBoundary.Bgm);
        }
        else
        {
            AudioManager.Instance.StopBGM();
        }
    }

    /// <summary>
    /// トランジション効果なしでシーン切り替えとプレイヤー位置更新を行う
    /// </summary>
    /// <param name="sceneBoundaryName">切り替え対象の境界名</param>
    public void DirectSceneSwitch(SceneBoundaryName sceneBoundaryName)
    {
        SceneBoundary currentBoundary;
        var targetBoundary = FindBoundaryByEnum(sceneBoundaryName, out currentBoundary);
        if (targetBoundary.Boundary != null)
        {
            ApplySceneAndPlayerSettings(currentBoundary, targetBoundary);
            if (targetBoundary.Bgm != BGMSoundData.BGM.None)
            {
                AudioManager.Instance.PlayBGM(targetBoundary.Bgm);
            }
            else
            {
                AudioManager.Instance.StopBGM();
            }
        }
        else
        {
            Debug.LogWarning("Boundary '" + sceneBoundaryName.ToString() + "' not found!");
        }
    }
    /// <summary>
    /// 指定したバウンドリーの IsOneWay フラグを設定します。
    /// true を設定すると、そのバウンドリーへのシーン切り替えが禁止されます。
    /// </summary>
    /// <param name="boundaryName">対象のシーンバウンドリー名</param>
    /// <param name="isOneWay">true: 一方向（禁止）、false: 双方向（許可）</param>
    public void SetBoundaryOneWay(SceneBoundaryName boundaryName, bool isOneWay)
    {
        for (int i = 0; i < boundaryPairs.Count; i++)
        {
            SceneBoundaryPair pair = boundaryPairs[i];
            bool modified = false;

            if (pair.BoundaryA.SceneBoundaryName == boundaryName)
            {
                pair.BoundaryA.IsOneWay = isOneWay;
                modified = true;
            }
            if (pair.BoundaryB.SceneBoundaryName == boundaryName)
            {
                pair.BoundaryB.IsOneWay = isOneWay;
                modified = true;
            }

            if (modified)
            {
                // 更新された pair をリストに再代入して反映させる
                boundaryPairs[i] = pair;
                Debug.Log("Boundary " + boundaryName.ToString() + " IsOneWay set to " + isOneWay);
            }
        }
    }

}
