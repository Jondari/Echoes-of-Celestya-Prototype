using System.Collections;
using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    [SerializeField] private GameObject damageVFX;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<DealDamageGA>(DealDamagePerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<DealDamageGA>();
    }

    private IEnumerator DealDamagePerformer(DealDamageGA dealDamageGA)
    {

        foreach (var target in dealDamageGA.Targets)
        {
            if (BattleFlowSystem.Instance.State != BattleState.PLAYING)
            {
                yield break;
            }

            // A queued action outlive one of its targets.
            if (target == null || target.CurrentHealth <= 0)
            {
                continue;
            }

            target.Damage(dealDamageGA.Amount);
            // Intanciante the VFX
            Instantiate(damageVFX, target.transform.position, Quaternion.identity);
            yield return new WaitForSeconds(0.15f);

            if (target == null)
            {
                continue;
            }

            if (target.CurrentHealth <= 0)
            {
                if (target is EnemyView enemyView)
                {
                    KillEnemyGA killEnemyGA = new(enemyView);
                    ActionSystem.Instance.AddReaction(killEnemyGA);
                }
                else
                {
                    // Do some game over logic here
                    // Open Game Over Scene
                    BattleFlowSystem.Instance.Defeat();
                }
            }
        }
   
    }
}
