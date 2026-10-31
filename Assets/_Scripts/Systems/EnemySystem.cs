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
        ActionSystem.AttachPerformer<ExecuteEnemyIntentGA>(ExecuteEnemyIntentPerformer);
        ActionSystem.AttachPerformer<PrepareEnemyIntentsGA>(PrepareEnemyIntentsPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemyTurnGA>();
        ActionSystem.DetachPerformer<AttackHeroGA>();
        ActionSystem.DetachPerformer<KillEnemyGA>();
        ActionSystem.DetachPerformer<ExecuteEnemyIntentGA>();
        ActionSystem.DetachPerformer<PrepareEnemyIntentsGA>();
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
        foreach (var enemy in enemyBoardView.EnemyViews)
        {
            int burnStacks = enemy.GetStatusEffectStacks(StatusEffectType.BURN);
            if (burnStacks > 0)
            {
                ApplyBurnGA applyBurnGA = new(burnStacks, enemy);
                ActionSystem.Instance.AddReaction(applyBurnGA);
            }

            ExecuteEnemyIntentGA executeIntentGA = new(enemy, enemy.CurrentIntent);
            ActionSystem.Instance.AddReaction(executeIntentGA);
        }
        PrepareEnemyIntentsGA prepareIntentsGA = new();
        ActionSystem.Instance.AddReaction(prepareIntentsGA);

        yield return null;
    }

    private IEnumerator AttackHeroPerformer(AttackHeroGA attackHeroGA)
    {
        EnemyView attacker = attackHeroGA.Attacker;
        HeroView hero = HeroSystem.Instance.HeroView;

        if (attacker == null
        || attacker.CurrentHealth <= 0
        || hero == null
        || hero.CurrentHealth <= 0
        || BattleFlowSystem.Instance.State != BattleState.PLAYING)
        {
            yield break;
        }

        float startX = attacker.transform.position.x;

        Tween tween = attacker.transform
            .DOMoveX(startX - 1f, 0.15f)
            .SetLink(attacker.gameObject, LinkBehaviour.KillOnDestroy);

        yield return tween.WaitForCompletion();

        if (attacker == null)
        {
            yield break;
        }

        attacker.transform
            .DOMoveX(startX, 0.15f)
            .SetLink(attacker.gameObject, LinkBehaviour.KillOnDestroy);

        if (attacker.CurrentHealth <= 0
            || hero == null
            || hero.CurrentHealth <= 0
            || BattleFlowSystem.Instance.State != BattleState.PLAYING)
        {
            yield break;
        }

        attackHeroGA.MarkExecuted();

        for (int i = 0; i < attackHeroGA.HitCount; i++)
        {
            DealDamageGA dealDamageGA = new(
                attackHeroGA.DamagePerHit,
                new() { hero },
                attackHeroGA.Caster);

            ActionSystem.Instance.AddReaction(dealDamageGA);
        }
    }

    private IEnumerator KillEnemyPerformer(KillEnemyGA killEnemyGA)
    {
        yield return enemyBoardView.RemoveEnemy(killEnemyGA.EnemyView);
        if (enemyBoardView.EnemyViews.Count == 0)
        {
            BattleFlowSystem.Instance.Victory();
        }
    }

    private IEnumerator ExecuteEnemyIntentPerformer(ExecuteEnemyIntentGA executeIntentGA)
    {
        EnemyView enemy = executeIntentGA.Enemy;
        EnemyIntent intent = executeIntentGA.Intent;

        if (enemy == null
            || enemy.CurrentHealth <= 0
            || BattleFlowSystem.Instance.State != BattleState.PLAYING)
        {
            yield break;
        }

        if (intent == null)
        {
            Debug.LogError($"Enemy '{enemy.name}' has no prepared intent.", enemy);
            yield break;
        }

        switch (intent.Type)
        {
            case EnemyIntentType.Attack:
                AttackHeroGA attackHeroGA = new(enemy, intent.Value, intent.HitCount);
                ActionSystem.Instance.AddReaction(attackHeroGA);
                break;

            case EnemyIntentType.Defense:
                AddStatusEffectGA armorGA = new(StatusEffectType.ARMOR, intent.Value, new() { enemy });
                ActionSystem.Instance.AddReaction(armorGA);
                break;

            default:
                Debug.LogError($"Intent type '{intent.Type}' is not implemented.", enemy);
                break;
        }

        yield return null;
    }

    private IEnumerator PrepareEnemyIntentsPerformer(PrepareEnemyIntentsGA prepareEnemyIntentsGA)
    {
        if (BattleFlowSystem.Instance.State != BattleState.PLAYING)
        {
            yield break;
        }

        PrepareIntents();

        yield return null;
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
