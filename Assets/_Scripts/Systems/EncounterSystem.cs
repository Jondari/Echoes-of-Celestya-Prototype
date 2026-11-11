using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EncounterSystem : Singleton<EncounterSystem>
{
    [SerializeField] private List<EnemyData> enemyPool = new();
    [SerializeField, Min(1)] private int minEnemies = 1;
    [SerializeField, Range(1, 3)] private int maxEnemies = 3;

    private readonly System.Random random = new();

    public bool TryGenerateEnemies(out List<EnemyData> enemies)
    {
        enemies = new List<EnemyData>();

        if (minEnemies < 1 || maxEnemies < minEnemies || maxEnemies > 3)
        {
            Debug.LogError("Enemy count must satisfy 1 <= minEnemies <= maxEnemies <= 3.", this);
            return false;
        }

        List<EnemyData> candidates = enemyPool
            .Where(enemy => enemy != null)
            .Distinct()
            .ToList();

        if (candidates.Count == 0)
        {
            Debug.LogError("Enemy pool contains no valid entries.", this);
            return false;
        }

        int enemyCount = random.Next(minEnemies, maxEnemies + 1);

        for (int i = 0; i < enemyCount; i++)
        {
            enemies.Add(candidates[random.Next(candidates.Count)]);
        }

        return true;
    }
}
