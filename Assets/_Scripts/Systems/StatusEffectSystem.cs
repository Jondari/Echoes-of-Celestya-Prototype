using UnityEngine;
using System.Collections;

public class StatusEffectSystem : MonoBehaviour
{
    [SerializeField] private GameObject defenseVFX;
    [SerializeField] private GameObject buffVFX;
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
            GameObject statusVFX = addStatusEffectGA.StatusEffectType switch
            {
                StatusEffectType.ARMOR => defenseVFX,
                StatusEffectType.ATTACK_BUFF => buffVFX,
                _ => null
            };

            if (statusVFX != null)
            {
                Instantiate(statusVFX, target.transform.position, statusVFX.transform.rotation);
                yield return new WaitForSeconds(0.4f);
            }
            else
            {
                yield return null;
            }
        }
    }
}
