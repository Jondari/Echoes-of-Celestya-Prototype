using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    [SerializeField] private EnemyBoardView enemyBoardView;
    public List<EnemyView> Enemies => enemyBoardView.EnemyViews;
    private readonly System.Random random = new();


    private void OnEnable()
    {
        ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
        ActionSystem.AttachPerformer<AttackHeroGA>(AttackHeroPerformer);
        ActionSystem.AttachPerformer<KillEnemyGA>(KillEnemyPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemyTurnGA>();
        ActionSystem.DetachPerformer<AttackHeroGA>();
        ActionSystem.DetachPerformer<KillEnemyGA>();
    }

    public void Setup(List<EnemyData> enemyDatas)
    {
        foreach (EnemyData enemyData in enemyDatas)
        {
            enemyBoardView.AddEnemy(enemyData);
        }

        foreach (EnemyView enemy in Enemies)
        {
            foreach (EnemyIntentOption option in enemy.Intents)
            {
                if (!IsOptionValid(option))
                {
                    Debug.LogWarning(
                        $"Enemy '{enemy.name}' has an invalid or unsupported " +
                        "intent option. This option will be ignored.",
                        enemy);
                }
                else if (option.Definition.Icon == null)
                {
                    Debug.LogWarning(
                        $"Intent '{option.Definition.name}' has no icon. " +
                        "A text fallback will be displayed.",
                        option.Definition);
                }
            }
        }

        PrepareIntents();
    }

    // Performers

    private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurnGA)
    {
        Debug.Log("Enemy Turn");
        foreach (var enemy in enemyBoardView.EnemyViews)
        {
            int burnStacks = enemy.GetStatusEffectStacks(StatusEffectType.BURN);
            if (burnStacks > 0)
            {
                ApplyBurnGA applyBurnGA = new(burnStacks, enemy);
                ActionSystem.Instance.AddReaction(applyBurnGA);
            }
            AttackHeroGA attackHeroGA = new(enemy);
            ActionSystem.Instance.AddReaction(attackHeroGA);
        }
        Debug.Log("End Enemy Turn");
        yield return null;
    }

    private IEnumerator AttackHeroPerformer(AttackHeroGA attackHeroGA)
    {
        EnemyView attacker = attackHeroGA.Attacker;

        if (attacker == null || attacker.CurrentHealth <= 0)
        {
            yield break;
        }

        // Attack animation (this is a simple example)
        Tween tween = attacker.transform.DOMoveX(attacker.transform.position.x - 1f, 0.15f);
        yield return tween.WaitForCompletion();
        attacker.transform.DOMoveX(attacker.transform.position.x + 1f, 0.15f);
        // Deal Damage
        DealDamageGA dealDamageGA = new(attacker.AttackPower, new() { HeroSystem.Instance.HeroView }, attackHeroGA.Caster);
        ActionSystem.Instance.AddReaction(dealDamageGA);
    }

    private IEnumerator KillEnemyPerformer(KillEnemyGA killEnemyGA)
    {
        yield return enemyBoardView.RemoveEnemy(killEnemyGA.EnemyView);
        if (enemyBoardView.EnemyViews.Count == 0)
        {
            BattleFlowSystem.Instance.Victory();
        }
    }

    private bool IsOptionValid(EnemyIntentOption option)
    {
        if (option == null || option.Definition == null)
        {
            return false;
        }

        switch (option.Definition.Type)
        {
            case EnemyIntentType.Attack:
                return option.AttackMultiplier > 0f
                    && !float.IsNaN(option.AttackMultiplier)
                    && !float.IsInfinity(option.AttackMultiplier)
                    && option.HitCount >= 1;

            case EnemyIntentType.Defense:
                return option.ArmorValue >= 1;

            default:
                return false;
        }
    }

    private EnemyIntent ChooseIntent(EnemyView enemy)
    {
        List<EnemyIntentOption> validOptions = new();

        foreach (EnemyIntentOption option in enemy.Intents)
        {
            if (IsOptionValid(option))
            {
                validOptions.Add(option);
            }
        }

        if (validOptions.Count == 0)
        {
            throw new System.InvalidOperationException(
                $"Enemy '{enemy.name}' has no valid intent. " +
                "Configure at least one Attack or Defense option in EnemyData.");
        }

        EnemyIntentOption selectedOption =
            validOptions[random.Next(validOptions.Count)];

        EnemyIntentData definition = selectedOption.Definition;

        int value;
        int hitCount;

        if (definition.Type == EnemyIntentType.Attack)
        {
            value = Mathf.Max(1, Mathf.RoundToInt(
                enemy.AttackPower * selectedOption.AttackMultiplier
                + selectedOption.FlatBonus));

            hitCount = selectedOption.HitCount;
        }
        else
        {
            value = selectedOption.ArmorValue;
            hitCount = 1;
        }

        return new EnemyIntent(
            definition.Type,
            definition.Icon,
            value,
            hitCount);
    }

    private void PrepareIntents()
    {
        foreach (EnemyView enemy in Enemies)
        {
            if (enemy == null || enemy.CurrentHealth <= 0)
            {
                continue;
            }

            enemy.SetIntent(ChooseIntent(enemy));
        }
    }
}
