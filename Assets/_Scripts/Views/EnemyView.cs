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

        intentIcon.sprite = CurrentIntent.Icon;

        string valueText = CurrentIntent.Type == EnemyIntentType.Attack
            && CurrentIntent.HitCount > 1
            ? $"{CurrentIntent.HitCount}x{CurrentIntent.Value}"
            : CurrentIntent.Value.ToString();

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
