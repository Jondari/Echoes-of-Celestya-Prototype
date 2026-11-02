using UnityEngine;
using System.Collections.Generic;

public class EnemyIntent
{
    public EnemyIntentType Type { get; }
    public Sprite Icon { get; }
    /// <summary>
    /// Meaning depends on Type:
    /// Attack: damage per hit.
    /// Defense: armor granted.
    /// Heal: health restored per target, capped by missing health.
    /// Buff / Debuff: stacks added, extending duration rather than strength.
    /// Summon: number of enemies to summon, subject to available slots.
    /// Wait: unused.
    /// </summary>
    public int Value { get; }
    public int HitCount { get; }
    public EnemyIntentTarget TargetMode { get; }
    public IReadOnlyList<CombatantView> Targets { get; }

    public EnemyIntent(EnemyIntentType type, Sprite icon, int value, int hitCount)
    : this(
        type,
        icon,
        value,
        hitCount,
        type == EnemyIntentType.Attack
            ? EnemyIntentTarget.Hero
            : EnemyIntentTarget.Self,
        new List<CombatantView>())
    {
    }

    public EnemyIntent(EnemyIntentType type, Sprite icon, int value, int hitCount, EnemyIntentTarget targetMode, List<CombatantView> targets)
    {
        Type = type;
        Icon = icon;
        Value = value;
        HitCount = type == EnemyIntentType.Attack ? hitCount : 1;
        TargetMode = targetMode;
        Targets = new List<CombatantView>(targets).AsReadOnly();
    }
}