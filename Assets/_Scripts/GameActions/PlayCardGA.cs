public class PlayCardGA : GameAction
{
    public EnemyView ManualTarget {  get; private set; }
    public Card Card {  get; set; }
    public PlayCardGA(Card card)
    {
        this.Card = card;
        ManualTarget = null;
    }

    public PlayCardGA(Card card, EnemyView target)
    {
        this.Card = card;
        ManualTarget = target;
    }
}
