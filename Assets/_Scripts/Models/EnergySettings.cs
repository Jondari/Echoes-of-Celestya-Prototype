[System.Serializable]
public class EnergySettings
{
    public int InitialEnergy = 3;
    public int EnergyPerTurn = 3;
    public int MaxEnergy = 3;
    public bool CarryOver = false;
    public EnergySettings Copy()
    {
        return new EnergySettings
        {
            InitialEnergy = this.InitialEnergy,
            EnergyPerTurn = this.EnergyPerTurn,
            MaxEnergy = this.MaxEnergy,
            CarryOver = this.CarryOver
        };
    }
}