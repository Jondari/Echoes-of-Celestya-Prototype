using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RewardSystem : Singleton<RewardSystem>
{
    private readonly System.Random random = new();
    [SerializeField] List<CardData> rewardPool = new();
    [SerializeField] private RewardOptionUI[] rewardOptions;

    private List<CardData> GenerateRewards()
    {
        List<CardData> availableCards = rewardPool.Distinct().Where(card => card != null).ToList();

        if (availableCards.Count == 0)
        {
            Debug.LogWarning(
                "No valid cards in the reward pool. Add CardData assets in the Inspector.",
                this
            );
            return new List<CardData>();
        }

        List<CardData> rewards = availableCards.OrderBy(x => random.Next())
            .Take(3)
            .ToList();

        // If fewer than three distinct cards are available, fill the remaining slots with random duplicates
        while (rewards.Count < 3) 
        {
            rewards.Add(availableCards[random.Next(availableCards.Count)]);
        }

        return rewards;
    }

    public void SetupRewards()
    {
        List<CardData> rewards = GenerateRewards();

        if(rewards.Count == 0 )
        {
            return;
        }

        for(var i = 0; i < rewardOptions.Length; i++)
        {
            rewardOptions[i].Setup(rewards[i]);
        }
    }
}
