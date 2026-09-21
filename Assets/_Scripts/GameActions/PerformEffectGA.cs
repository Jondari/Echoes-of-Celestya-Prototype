using System.Collections.Generic;

public class PerformEffectGA : GameAction
{
    public Effect Effect { get; private set; }
    public List<CombatantView> Targets { get; private set; }
    public PerformEffectGA(Effect effect, List<CombatantView> targets)
    {
        Effect = effect;
        Targets = targets == null ? null : new(targets);
    }
}
