using UnityEngine;
using System.Collections;

public class StatusEffectSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AddStatusEffectGA>(AddStatusEffectPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AddStatusEffectGA>();
    }

    private IEnumerator AddStatusEffectPerformer(AddStatusEffectGA addStatusEffectGA)
    {
        if (addStatusEffectGA.StackCount <= 0)
        {
            yield break;
        }

        foreach (CombatantView target in addStatusEffectGA.Targets)
        {
            if (BattleFlowSystem.Instance.State != BattleState.PLAYING)
            {
                yield break;
            }

            if (target == null || target.CurrentHealth <= 0)
            {
                continue;
            }

            target.AddStatusEffect(addStatusEffectGA.StatusEffectType, addStatusEffectGA.StackCount);
            yield return null; // TODO ADD VFX for adding status effects
        }
    }
}
