using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class EnemyView : CombatantView
{
    [SerializeField] private TMP_Text attackText;
    public int AttackPower {  get; private set; }
    private EnemyData enemyData;

    public IReadOnlyList<EnemyIntentOption> Intents => enemyData.Intents;

    public EnemyIntent CurrentIntent { get; private set; }

    public void Setup(EnemyData enemyData)
    {
        this.enemyData = enemyData;
        AttackPower = enemyData.AttackPower;
        UpdateAttackText();
        SetupBase(enemyData.Health, enemyData.Image);
    }

    private void UpdateAttackText()
    {
        attackText.text = AttackPower.ToString();
    }

    public void SetIntent(EnemyIntent intent)
    {
        CurrentIntent = intent;
    }
}
