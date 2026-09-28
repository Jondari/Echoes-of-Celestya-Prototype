public class GameSession : Singleton<GameSession>
{
    public RunState CurrentRun { get; private set; }

    public void SelectHero(HeroData hero)
    {
        CurrentRun = new(hero);
    }
}
