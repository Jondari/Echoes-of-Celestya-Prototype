using UnityEngine;

public class EnemyIntent
{
    public EnemyIntentType Type { get; }
    public Sprite Icon { get; }
    public int Value { get; }
    public int HitCount { get; }

    public EnemyIntent(EnemyIntentType type, Sprite icon, int value, int hitCount)
    {
        Type = type;
        Icon = icon;
        Value = value;
        HitCount = type == EnemyIntentType.Attack ? hitCount : 1;
    }
}