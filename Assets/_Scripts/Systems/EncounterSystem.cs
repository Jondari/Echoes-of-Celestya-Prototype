using System.Collections.Generic;
using UnityEngine;

public class EncounterSystem : Singleton<EncounterSystem>
{
    [SerializeField] private List<EnemyData> enemyPool = new();
    [SerializeField, Min(1)] private int minEnemies = 1;
    [SerializeField, Range(1, 3)] private int maxEnemies = 3;

    private readonly System.Random random = new();
}
