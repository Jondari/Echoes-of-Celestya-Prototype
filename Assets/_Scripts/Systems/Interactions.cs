public class Interactions : Singleton<Interactions>
{
    public bool PlayerIsDragging { get; set; } = false;
    public bool PlayerCanInteract()
    {
        return BattleFlowSystem.Instance.State == BattleState.PLAYING && ActionSystem.Instance.IsPerforming != true;
    }

    public bool PlayerCanHover()
    {
        return BattleFlowSystem.Instance.State == BattleState.PLAYING && PlayerIsDragging != true;
    }
}
