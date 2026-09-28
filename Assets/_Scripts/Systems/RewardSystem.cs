using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RewardSystem : Singleton<RewardSystem>
{
    private readonly System.Random random = new();
    [SerializeField] List<CardData> rewardPool = new();
    [SerializeField] private RewardOptionUI[] rewardOptions;
    private List<CardData> currentRewards = new();
    private bool isRewardPending = false;

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
        isRewardPending = false;
        SetOptionsInteractable(false);

        currentRewards = GenerateRewards();

        if (currentRewards.Count == 0 )
        {
            return;
        }

        for (var i = 0; i < rewardOptions.Length; i++)
        {
            rewardOptions[i].Setup(currentRewards[i]);
        }
        isRewardPending = true;
        SetOptionsInteractable(true);
    }

    public void SelectReward(CardData card)
    {
        if (card == null || !isRewardPending || !currentRewards.Contains(card))
        {
            return;
        }

        if (GameSession.Instance != null && GameSession.Instance.CurrentRun != null)
        {
            isRewardPending = false;
            SetOptionsInteractable(false);
            GameSession.Instance.CurrentRun.CurrentDeck.Add(card);
            GameSession.Instance.CurrentRun.CompleteCurrentBattle();
            GameFlowSystem.Instance.ShowMap();
        }
    }

    private void SetOptionsInteractable(bool isInteractable)
    {
        for (var i = 0; i < rewardOptions.Length; i++)
        {
            rewardOptions[i].SetInteractable(isInteractable);
        }
    }
}
