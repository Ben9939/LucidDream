using UnityEngine;

/// <summary>
/// Systems クラスは、常駐する主要システムを管理するためのクラスです。
/// このクラスは、一度生成されるとシーン遷移等で破棄されず、サブシステムを子オブジェクトとして保持します。
/// </summary>
public class Systems : PresistentSingleton<Systems>
{
}
