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

    /// <summary>
    /// Target selection for support intents.
    /// Heal and Buff use Self, OtherAlly or AllAllies.
    /// Attack always targets the hero; Defense always targets the caster.
    /// Target is ignored for Attack and Defense.
    /// Debuff requires Hero.
    /// </summary>
    [field: SerializeField]
    [field: Tooltip(
    "Heal/Buff: choose Self, OtherAlly or AllAllies. " +
    "Ignored for Attack (always Hero) and Defense (always Self). " +
    "Debuff: select Hero.")]
    public EnemyIntentTarget Target { get; private set; }

    [field: SerializeField, Min(1)]
    public int HealAmount { get; private set; } = 1;

    [field: SerializeField, Min(1)]
    public int StatusStacks { get; private set; } = 1;
}