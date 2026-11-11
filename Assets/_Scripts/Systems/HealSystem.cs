using System.Collections;
using UnityEngine;

public class HealSystem : MonoBehaviour
{
    [SerializeField] private GameObject healVFX;
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

            int healthBeforeHeal = target.CurrentHealth;
            target.Heal(healGA.Amount);

            if (healVFX != null && target.CurrentHealth > healthBeforeHeal)
            {
                Instantiate(healVFX, target.transform.position, healVFX.transform.rotation);
                yield return new WaitForSeconds(0.4f);
            }
            else
            {
                yield return null;
            }
        }
    }
}