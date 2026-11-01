using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DealDamageEffect : Effect
{
    [SerializeField] private int damageAmount;

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        int attackBonus = caster != null
        ? caster.GetAttackBonus()
        : 0;

        int amount = Mathf.Max(0, damageAmount + attackBonus);

        DealDamageGA dealDamageGA = new(amount, targets, caster);
        return dealDamageGA;
    }
}
