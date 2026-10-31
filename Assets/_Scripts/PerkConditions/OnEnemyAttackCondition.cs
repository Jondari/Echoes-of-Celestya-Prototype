using System;

[System.Serializable]
public class OnEnemyAttackCondition : PerkCondition
{
    public override bool SubConditionIsMet(GameAction gameAction)
    {
        if (gameAction is not AttackHeroGA attackHeroGA)
        {
            return false;
        }

        return attackHeroGA.HasExecuted
            && attackHeroGA.Attacker != null
            && attackHeroGA.Attacker.CurrentHealth > 0
            && HeroSystem.Instance.HeroView != null
            && HeroSystem.Instance.HeroView.CurrentHealth > 0
            && BattleFlowSystem.Instance.State == BattleState.PLAYING;
    }

    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.SubscribeReaction<AttackHeroGA>(reaction, reactionTiming);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.UnsubscribeReaction<AttackHeroGA>(reaction, reactionTiming);
    }
}
