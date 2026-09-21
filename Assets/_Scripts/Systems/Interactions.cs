public class Interactions : Singleton<Interactions>
{
    public bool PlayerIsDragging { get; set; } = false;
    public bool PlayerCanInteract()
    {
        return ActionSystem.Instance.IsPerforming != true;
    }

    public bool PlayerCanHover()
    {
        return PlayerIsDragging != true;
    }
}
