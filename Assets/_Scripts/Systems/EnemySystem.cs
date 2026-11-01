using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    [SerializeField] private EnemyBoardView enemyBoardView;
    public List<EnemyView> Enemies => enemyBoardView.EnemyViews;
    private readonly System.Random random = new();
    [SerializeField] private EnemyIntentData waitIntent;

    // Only buffs active at the start of the enemy turn lose a stack.
    private readonly List<EnemyView> buffedEnemiesAtTurnStart = new();


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
        buffedEnemiesAtTurnStart.Clear();

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
        buffedEnemiesAtTurnStart.Clear();

        foreach (EnemyView enemy in Enemies)
        {
            if (enemy == null || enemy.CurrentHealth <= 0)
            {
                continue;
            }

            if (enemy.GetStatusEffectStacks(StatusEffectType.ATTACK_BUFF) > 0)
            {
                buffedEnemiesAtTurnStart.Add(enemy);
            }

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

            case EnemyIntentType.Heal:
                HealGA healGA = new(intent.Value, intent.Targets, enemy);
                ActionSystem.Instance.AddReaction(healGA);
                break;

            case EnemyIntentType.Buff:
            case EnemyIntentType.Debuff:
                StatusEffectType statusType =
                    intent.Type == EnemyIntentType.Buff
                        ? StatusEffectType.ATTACK_BUFF
                        : StatusEffectType.ATTACK_DEBUFF;

                AddStatusEffectGA statusGA = new(
                    statusType,
                    intent.Value,
                    new List<CombatantView>(intent.Targets));

                ActionSystem.Instance.AddReaction(statusGA);
                break;

            case EnemyIntentType.Wait:
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
            buffedEnemiesAtTurnStart.Clear();
            yield break;
        }

        foreach (EnemyView enemy in buffedEnemiesAtTurnStart)
        {
            if (enemy == null || enemy.CurrentHealth <= 0)
            {
                continue;
            }

            enemy.RemoveStatusEffect(StatusEffectType.ATTACK_BUFF, 1);
        }

        buffedEnemiesAtTurnStart.Clear();

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

            case EnemyIntentType.Heal:
                return option.HealAmount >= 1
                    && (option.Target == EnemyIntentTarget.Self
                        || option.Target == EnemyIntentTarget.OtherAlly
                        || option.Target == EnemyIntentTarget.AllAllies);

            case EnemyIntentType.Buff:
                return option.StatusStacks >= 1
                    && (option.Target == EnemyIntentTarget.Self
                        || option.Target == EnemyIntentTarget.OtherAlly
                        || option.Target == EnemyIntentTarget.AllAllies);

            case EnemyIntentType.Debuff:
                return option.StatusStacks >= 1
                    && option.Target == EnemyIntentTarget.Hero;

            default:
                return false;
        }
    }

    private bool CanUseOption(EnemyView enemy, EnemyIntentOption option)
    {
        switch (option.Definition.Type)
        {
            case EnemyIntentType.Heal:
                return GetHealTargets(enemy, option.Target).Count > 0;

            case EnemyIntentType.Buff:
                return GetBuffCandidates(enemy, option.Target).Count > 0;

            case EnemyIntentType.Debuff:
                return HeroSystem.Instance.HeroView != null
                    && HeroSystem.Instance.HeroView.CurrentHealth > 0;

            default:
                return true;
        }
    }

    private EnemyIntent ChooseIntent(EnemyView enemy)
    {
        List<EnemyIntentOption> availableOptions = new();
        bool hasValidOption = false;

        foreach (EnemyIntentOption option in enemy.Intents)
        {
            if (!IsOptionValid(option))
            {
                continue;
            }

            hasValidOption = true;

            if (CanUseOption(enemy, option))
            {
                availableOptions.Add(option);
            }
        }

        if (!hasValidOption)
        {
            throw new System.InvalidOperationException(
                $"Enemy '{enemy.name}' has no valid intent. " +
                "Configure at least one supported option in EnemyData.");
        }

        if (availableOptions.Count == 0)
        {
            return new EnemyIntent(
                EnemyIntentType.Wait,
                waitIntent != null ? waitIntent.Icon : null,
                0,
                1);
        }

        EnemyIntentOption selectedOption =
            availableOptions[random.Next(availableOptions.Count)];

        EnemyIntentData definition = selectedOption.Definition;

        switch (definition.Type)
        {
            case EnemyIntentType.Attack:
                int attackPower = Mathf.Max(0,enemy.AttackPower + enemy.GetAttackBonus());

                int damage = Mathf.Max(1, Mathf.RoundToInt(
                    attackPower * selectedOption.AttackMultiplier
                    + selectedOption.FlatBonus));

                return new EnemyIntent(
                    definition.Type,
                    definition.Icon,
                    damage,
                    selectedOption.HitCount);

            case EnemyIntentType.Defense:
                return new EnemyIntent(
                    definition.Type,
                    definition.Icon,
                    selectedOption.ArmorValue,
                    1);

            case EnemyIntentType.Heal:
                List<CombatantView> targets =
                    GetHealTargets(enemy, selectedOption.Target);

                return new EnemyIntent(
                    definition.Type,
                    definition.Icon,
                    selectedOption.HealAmount,
                    1,
                    selectedOption.Target,
                    targets);

            case EnemyIntentType.Buff:
                List<CombatantView> buffTargets = GetBuffCandidates(enemy, selectedOption.Target);

                if (selectedOption.Target == EnemyIntentTarget.OtherAlly)
                {
                    CombatantView selectedAlly = buffTargets[random.Next(buffTargets.Count)];

                    buffTargets = new() { selectedAlly };
                }

                return new EnemyIntent(
                    definition.Type,
                    definition.Icon,
                    selectedOption.StatusStacks,
                    1,
                    selectedOption.Target,
                    buffTargets);

            case EnemyIntentType.Debuff:
                return new EnemyIntent(
                    definition.Type,
                    definition.Icon,
                    selectedOption.StatusStacks,
                    1,
                    EnemyIntentTarget.Hero,
                    new() { HeroSystem.Instance.HeroView });

            default:
                throw new System.InvalidOperationException(
                    $"Intent type '{definition.Type}' is not implemented.");
        }
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

    private List<CombatantView> GetHealTargets(EnemyView caster, EnemyIntentTarget targetMode)
    {
        List<CombatantView> targets = new();

        switch (targetMode)
        {
            case EnemyIntentTarget.Self:
                if (caster.CurrentHealth > 0
                    && caster.CurrentHealth < caster.MaxHealth)
                {
                    targets.Add(caster);
                }
                break;

            case EnemyIntentTarget.OtherAlly:
                EnemyView selectedAlly = null;
                float lowestHealthRatio = 1f;

                foreach (EnemyView ally in Enemies)
                {
                    if (ally == null
                        || ally == caster
                        || ally.CurrentHealth <= 0
                        || ally.CurrentHealth >= ally.MaxHealth)
                    {
                        continue;
                    }

                    float healthRatio =
                        (float)ally.CurrentHealth / ally.MaxHealth;

                    if (healthRatio < lowestHealthRatio)
                    {
                        lowestHealthRatio = healthRatio;
                        selectedAlly = ally;
                    }
                }

                if (selectedAlly != null)
                {
                    targets.Add(selectedAlly);
                }
                break;

            case EnemyIntentTarget.AllAllies:
                bool hasWoundedAlly = false;

                foreach (EnemyView ally in Enemies)
                {
                    if (ally == null || ally.CurrentHealth <= 0)
                    {
                        continue;
                    }

                    targets.Add(ally);

                    if (ally.CurrentHealth < ally.MaxHealth)
                    {
                        hasWoundedAlly = true;
                    }
                }

                if (!hasWoundedAlly)
                {
                    targets.Clear();
                }
                break;
        }

        return targets;
    }

    private List<CombatantView> GetBuffCandidates(EnemyView caster, EnemyIntentTarget targetMode)
    {
        List<CombatantView> candidates = new();

        if (targetMode == EnemyIntentTarget.Self)
        {
            if (caster != null && caster.CurrentHealth > 0)
            {
                candidates.Add(caster);
            }

            return candidates;
        }

        if (targetMode != EnemyIntentTarget.OtherAlly
            && targetMode != EnemyIntentTarget.AllAllies)
        {
            return candidates;
        }

        foreach (EnemyView ally in Enemies)
        {
            if (ally == null || ally.CurrentHealth <= 0)
            {
                continue;
            }

            if (targetMode == EnemyIntentTarget.OtherAlly && ally == caster)
            {
                continue;
            }

            candidates.Add(ally);
        }

        return candidates;
    }
}
