public class GameSession : Singleton<GameSession>
{
    public RunState CurrentRun { get; private set; }

    public EnergySettings EnergySettings { get; private set; } = new EnergySettings();
    public bool CounterAttackEnabled { get; private set; } = false;

    public void ApplyEnergySettings(EnergySettings settings)
    {
        EnergySettings = settings.Copy();
    }

    public void SetCounterAttackEnabled(bool enabled)
    {
        CounterAttackEnabled = enabled;
    }

    public void SelectHero(HeroData hero)
    {
        CurrentRun = new(hero, EnergySettings, CounterAttackEnabled);
    }
}
