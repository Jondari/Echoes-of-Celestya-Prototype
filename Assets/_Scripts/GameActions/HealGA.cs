using System.Collections.Generic;

public class HealGA : GameAction, IHaveCaster
{
    public int Amount { get; }
    public IReadOnlyList<CombatantView> Targets { get; }
    public CombatantView Caster { get; }

    public HealGA(
        int amount,
        IReadOnlyList<CombatantView> targets,
        CombatantView caster)
    {
        Amount = amount;
        Targets = new List<CombatantView>(targets).AsReadOnly();
        Caster = caster;
    }
}