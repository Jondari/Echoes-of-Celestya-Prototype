using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class EnemyView : CombatantView
{
    [SerializeField] private TMP_Text intentText;
    [SerializeField] private SpriteRenderer intentIcon;
    public int AttackPower {  get; private set; }
    private EnemyData enemyData;

    public IReadOnlyList<EnemyIntentOption> Intents => enemyData.Intents;

    public EnemyIntent CurrentIntent { get; private set; }

    public void Setup(EnemyData enemyData)
    {
        this.enemyData = enemyData;
        AttackPower = enemyData.AttackPower;
        CurrentIntent = null;

        SetupBase(enemyData.Health, enemyData.Image);
        UpdateIntentDisplay();
    }

    private void UpdateIntentDisplay()
    {
        if (CurrentIntent == null)
        {
            intentIcon.sprite = null;
            intentText.text = "";
            return;
        }

        if (CurrentIntent.Type == EnemyIntentType.Wait)
        {
            intentIcon.sprite = CurrentIntent.Icon;
            intentText.text = CurrentIntent.Icon != null ? "" : "Wait";
            return;
        }

        intentIcon.sprite = CurrentIntent.Icon;

        string valueText;

        switch (CurrentIntent.Type)
        {
            case EnemyIntentType.Attack:
                valueText = CurrentIntent.HitCount > 1
                    ? $"{CurrentIntent.HitCount}x{CurrentIntent.Value}"
                    : CurrentIntent.Value.ToString();
                break;

            case EnemyIntentType.Heal:
                valueText = $"{CurrentIntent.Value} {CurrentIntent.TargetMode}";
                break;

            case EnemyIntentType.Buff:
                valueText =
                    $"+2 · {CurrentIntent.Value}t · {CurrentIntent.TargetMode}";
                break;

            case EnemyIntentType.Debuff:
                valueText = $"-2 · {CurrentIntent.Value}t";
                break;

            default:
                valueText = CurrentIntent.Value.ToString();
                break;
        }

        intentText.text = CurrentIntent.Icon != null
            ? valueText
            : $"{CurrentIntent.Type}: {valueText}";
    }

    public void SetIntent(EnemyIntent intent)
    {
        CurrentIntent = intent;
        UpdateIntentDisplay();
    }
}
