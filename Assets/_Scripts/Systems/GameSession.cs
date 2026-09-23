public class GameSession : Singleton<GameSession>
{
    public HeroData SelectedHero { get; private set; }

    public void SelectHero(HeroData hero)
    {
        SelectedHero = hero;
    }
}
