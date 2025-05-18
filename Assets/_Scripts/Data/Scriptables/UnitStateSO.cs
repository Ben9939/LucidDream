using System;
using UnityEngine;

/// <summary>
/// ユニットの状態（例：体力、経験値など）を管理するScriptableObjectです。  
/// 初期値、現在値、必要値、および比較結果を保持し、状態変化時にイベントを発火します。
/// </summary>
[CreateAssetMenu(menuName = "Unit/UnitState")]
public class UnitStateSO : ScriptableObject
{
    [SerializeField] private string stateName;
    [SerializeField] private int initialValue;
    [SerializeField] private int currentValue;
    [SerializeField] private int requiredValue;
    private ComparisonResult comparisonType;

    /// <summary>
    /// 状態名を返します（ScriptableObjectの名前と同じ）。
    /// </summary>
    public string StateName => stateName;

    /// <summary>
    /// 現在の状態値。変更時にOnStateChangedイベントと比較結果の更新を行います。
    /// </summary>
    public int Value
    {
        get => currentValue;
        set
        {
            if (currentValue != value)
            {
                currentValue = value;
                OnStateChanged?.Invoke(currentValue);
                UpdateComparisonResult();
            }
        }
    }

    /// <summary>
    /// 必要な状態値を返します。
    /// </summary>
    public int RequiredValue => requiredValue;

    /// <summary>
    /// 現在の比較結果（Equal, GreaterThan, etc.）を返します。
    /// </summary>
    public ComparisonResult CurrentComparisonType => comparisonType;

    /// <summary>
    /// 状態値が必要値と等しい場合、Equalフラグが含まれているかどうかを返します。
    /// </summary>
    public bool StateComparisonResult => (comparisonType & ComparisonResult.Equal) == ComparisonResult.Equal;

    public event Action<int> OnStateChanged;
    public event Action<ComparisonResult> OnComparisonResultChanged;

    /// <summary>
    /// 状態を初期値にリセットし、イベントを発火します。
    /// </summary>
    public void ResetState()
    {
        currentValue = initialValue;
        UpdateComparisonResult();
        OnStateChanged?.Invoke(currentValue);
    }

    /// <summary>
    /// 現在の状態値と必要値に基づいて比較結果を更新し、OnComparisonResultChangedイベントを発火します。  
    /// 値が等しい場合はEqual, GreaterThanOrEqual, LessThanOrEqualを含み、  
    /// 値が大きいまたは小さい場合はそれぞれ対応するフラグを設定します。
    /// </summary>
    private void UpdateComparisonResult()
    {
        if (currentValue == requiredValue)
        {
            comparisonType = ComparisonResult.Equal | ComparisonResult.GreaterThanOrEqual | ComparisonResult.LessThanOrEqual;
        }
        else if (currentValue > requiredValue)
        {
            comparisonType = ComparisonResult.GreaterThan | ComparisonResult.GreaterThanOrEqual;
        }
        else // currentValue < requiredValue
        {
            comparisonType = ComparisonResult.LessThan | ComparisonResult.LessThanOrEqual;
        }
        OnComparisonResultChanged?.Invoke(comparisonType);
    }

    /// <summary>
    /// 指定されたComparisonResultに対して、現在の状態がその条件を満たすかどうかを返します。
    /// </summary>
    public bool Compare(ComparisonResult op)
    {
        switch (op)
        {
            case ComparisonResult.Equal:
                return currentValue == requiredValue;
            case ComparisonResult.GreaterThan:
                return currentValue > requiredValue;
            case ComparisonResult.LessThan:
                return currentValue < requiredValue;
            case ComparisonResult.GreaterThanOrEqual:
                return currentValue >= requiredValue;
            case ComparisonResult.LessThanOrEqual:
                return currentValue <= requiredValue;
            default:
                return false;
        }
    }

    public void ResetData()
    {
        currentValue = initialValue;
    }
}

/// <summary>
/// 状態の比較結果を示す列挙型です。
/// </summary>
public enum ComparisonResult
{
    None,
    Equal,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual
}
