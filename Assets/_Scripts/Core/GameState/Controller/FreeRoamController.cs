using EasyTransition;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// フリー・ローム状態における処理を管理するクラス。
/// GameStateManagerから現在の状態を取得し、FreeRoamStateの場合はイベントにサブスクライブする。
/// </summary>
public class FreeRoamController : ControllerBase
{
    /// <summary>
    /// コンポーネント開始時に呼ばれる。
    /// 現在のGameStateを取得し、FreeRoamStateであればイベントにサブスクライブする。
    /// </summary>
    private void Start()
    {
        // GameStateManagerから現在のGameStateを取得する
        GameStateBase currentState = GameStateManager.Instance.GetCurrentState();

        // 現在の状態がFreeRoamStateの場合、イベントにサブスクライブする
        if (currentState is FreeRoamState freeRoamState)
        {
            SubscribeToState(freeRoamState);
        }
    }

    /// <summary>
    /// コンポーネント破棄時に呼ばれる。
    /// イベントのサブスクライブ解除を行い、イベントリークを防止する。
    /// </summary>
    private void OnDestroy()
    {
        // GameStateManagerから現在のGameStateを取得する
        GameStateBase currentState = GameStateManager.Instance.GetCurrentState();

        // 現在の状態がFreeRoamStateの場合、イベントからのサブスクライブ解除を行う
        if (currentState is FreeRoamState freeRoamState)
        {
            UnsubscribeFromState(freeRoamState);
        }
    }

    /// <summary>
    /// FreeRoamState開始時に実行される初期化処理。
    /// 必要に応じて初期化ロジックをここに記述する。
    /// </summary>
    protected override void Initialize()
    {
    }

    /// <summary>
    /// FreeRoamState更新時に毎フレーム実行される処理。
    /// 必要に応じて更新ロジックをここに記述する。
    /// </summary>
    protected override void OnUpdate()
    {
    }

    /// <summary>
    /// FreeRoamState終了時に実行されるクリーンアップ処理。
    /// 必要に応じてクリーンアップロジックをここに記述する。
    /// </summary>
    protected override void Cleanup()
    {
    }
}