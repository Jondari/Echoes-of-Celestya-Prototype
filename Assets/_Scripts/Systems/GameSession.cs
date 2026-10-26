public class GameSession : Singleton<GameSession>
{
    public RunState CurrentRun { get; private set; }

    public EnergySettings EnergySettings { get; private set; } = new EnergySettings();

    public void ApplyEnergySettings(EnergySettings settings)
    {
        EnergySettings = settings.Copy();
    }

    public void SelectHero(HeroData hero)
    {
        CurrentRun = new(hero, EnergySettings);
    }
}
