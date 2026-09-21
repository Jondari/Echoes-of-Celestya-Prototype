using System.Collections.Generic;

[System.Serializable]
public class NoTM : TargetMode
{
    public override List<CombatantView> GetTargets()
    {
        return null;
    }
}
