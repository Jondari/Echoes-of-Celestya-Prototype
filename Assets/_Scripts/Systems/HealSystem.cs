using System.Collections;
using UnityEngine;

public class HealSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<HealGA>(HealPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<HealGA>();
    }

    private IEnumerator HealPerformer(HealGA healGA)
    {
        foreach (CombatantView target in healGA.Targets)
        {
            if (BattleFlowSystem.Instance.State != BattleState.PLAYING
                || healGA.Caster == null
                || healGA.Caster.CurrentHealth <= 0)
            {
                yield break;
            }

            if (target == null || target.CurrentHealth <= 0)
            {
                continue;
            }

            target.Heal(healGA.Amount);

            yield return null;
        }
    }
}