public class ExecuteEnemyIntentGA : GameAction
{
    public EnemyView Enemy { get; }
    public EnemyIntent Intent { get; }

    public ExecuteEnemyIntentGA(EnemyView enemy, EnemyIntent intent)
    {
        Enemy = enemy;
        Intent = intent;
    }
}
