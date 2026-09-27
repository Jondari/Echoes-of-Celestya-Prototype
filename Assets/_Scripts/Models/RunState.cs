using System.Collections.Generic;

public class RunState
{
    public HeroData SelectedHero { get; private set; }
    public List<CardData> CurrentDeck { get; private set; }
    // the number of the current batlle (+1) Ex: CurrentEncounter = 0 <=> battle 1
    public int CurrentEncounter {  get; private set; }
    public bool[] BattleCompleted { get; private set; } = new bool[3];

    public RunState(HeroData hero)
    {
        SelectedHero = hero;
        CurrentDeck = new List<CardData>(hero.Deck);
    }

    public void UpdateCurrentEncounter(int encounter)
    {
        CurrentEncounter = encounter;
    }
}
