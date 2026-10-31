using UnityEngine;

[System.Serializable]
public class EnemyIntentOption
{
    [field: SerializeField]
    public EnemyIntentData Definition { get; private set; }

    [field: SerializeField, Min(0.01f)]
    public float AttackMultiplier { get; private set; } = 1f;

    [field: SerializeField]
    public int FlatBonus { get; private set; } = 0;

    [field: SerializeField, Min(1)]
    public int HitCount { get; private set; } = 1;

    [field: SerializeField, Min(1)]
    public int ArmorValue { get; private set; } = 1;
}