public class AttackHeroGA : GameAction, IHaveCaster
{
    public EnemyView Attacker { get; private set; }
    public CombatantView Caster { get; private set; }
    public int DamagePerHit { get; }
    public int HitCount { get; }

    public bool HasExecuted { get; private set; }

    public AttackHeroGA(EnemyView attacker, int damagePerHit, int hitCount)
    {
        Attacker = attacker;
        Caster = Attacker;
        DamagePerHit = damagePerHit;
        HitCount = hitCount;
    }

    public void MarkExecuted()
    {
        HasExecuted = true;
    }
}
