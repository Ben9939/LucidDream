using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FreeRoam_MenuController は、自由移動状態のメニューサブステートを管理します。
/// メニュー UI の表示・非表示を制御します。
/// </summary>
public class FreeRoam_MenuController : FreeRoam_ControllerBase
{
    [SerializeField] private GameObject menuCanvas; // メニュー UI キャンバス

    /// <summary>
    /// 管理対象のサブステートは Menu です。
    /// </summary>
    protected override FreeRoamSubState ControlledSubState => FreeRoamSubState.Menu;

    /// <summary>
    /// メニュー状態に入るとき、メニューキャンバスを有効にします。
    /// </summary>
    protected override void OnSubStateEnter()
    {
        menuCanvas.SetActive(true);
    }

    /// <summary>
    /// メニュー状態中の更新処理（必要に応じて実装）。
    /// </summary>
    protected override void OnSubStateUpdate()
    {
        // メニュー更新処理を追加できます
    }

    /// <summary>
    /// メニュー状態から出るとき、メニューキャンバスを無効にします。
    /// </summary>
    protected override void OnSubStateExit()
    {
        menuCanvas.SetActive(false);
    }

    /// <summary>
    /// メニュー UI のボタンクリック時に呼び出され、探索状態に切り替えます。
    /// </summary>
    public void HandleMenuOnClick()
    {
        currentSubState.RequestSubStateChange(FreeRoamSubState.Exploration);
    }
}