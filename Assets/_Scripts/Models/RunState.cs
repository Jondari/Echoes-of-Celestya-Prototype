using System.Collections.Generic;

public class RunState
{
    public HeroData SelectedHero { get; private set; }
    public List<CardData> CurrentDeck { get; private set; }
    // the number of the current batlle (+1) Ex: CurrentEncounter = 0 <=> battle 1
    public int CurrentEncounter {  get; private set; }
    public bool[] BattleCompleted { get; private set; } = new bool[3];

    public EnergySettings EnergySettings { get; private set; }

    public RunState(HeroData hero, EnergySettings energySettings)
    {
        SelectedHero = hero;
        CurrentDeck = new List<CardData>(hero.Deck);
        EnergySettings = energySettings.Copy();
    }

    public void UpdateCurrentEncounter(int encounter)
    {
        if (encounter >= 0 && encounter < BattleCompleted.Length)
        {
            CurrentEncounter = encounter;
        }
    }

    // A battle is available if its index is valid, it has not been completed,
    // and it is either the first battle or the previous battle has been completed.
    public bool IsBattleAvailable(int battleIndex)
    {
        if (battleIndex >= 0 && battleIndex < BattleCompleted.Length)
        {
            return !BattleCompleted[battleIndex] && (battleIndex == 0 || BattleCompleted[battleIndex - 1]);
        }

        return false;
    }

    public void CompleteCurrentBattle()
    {
        BattleCompleted[CurrentEncounter] = true;
    }
}
