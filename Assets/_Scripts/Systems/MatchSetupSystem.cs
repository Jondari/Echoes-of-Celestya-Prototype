using UnityEngine;
using System.Collections.Generic;

public class MatchSetupSystem : Singleton<MatchSetupSystem>
{
    [SerializeField] private HeroData heroData;
    [SerializeField] private PerkData perkData;
    [SerializeField] private List<EnemyData> enemyDatas;
    [SerializeField] private GameObject standaloneEventSystem;
    [SerializeField] private AudioListener standaloneAudioListener;

    public void SetupBattle()
    {
        HeroData selectedHero = null;
        
        if (GameSession.Instance != null && GameSession.Instance.CurrentRun != null)
        {
            selectedHero = GameSession.Instance.CurrentRun.SelectedHero;
        }

        if (selectedHero == null)
        {
            selectedHero = heroData;
        }

        if (selectedHero == null)
        {
            Debug.LogError("No HeroData available to initialize the battle.", this);
            return;
        }

        List<CardData> deck = selectedHero.Deck;

        if (GameSession.Instance != null && GameSession.Instance.CurrentRun != null)
        {
            deck = GameSession.Instance.CurrentRun.CurrentDeck;
        }

        EnergySettings energySettings = new();

        if (GameSession.Instance != null
            && GameSession.Instance.CurrentRun != null)
        {
            energySettings = GameSession.Instance.CurrentRun.EnergySettings;
        }

        ManaSystem.Instance.Setup(energySettings);
        HeroSystem.Instance.Setup(selectedHero);
        CardSystem.Instance.Setup(deck);
        EnemySystem.Instance.Setup(enemyDatas);
        PerkSystem.Instance.AddPerk(new Perk(perkData));

        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.Perform(drawCardsGA);
    }

    // Initialize Battle directly for standalone testing in the Editor.
    private void Start()
    {
    #if UNITY_EDITOR
        if (GameSession.Instance == null)
        {
            if (standaloneEventSystem != null)
            {
                standaloneEventSystem.SetActive(true);
            }
            else
            {
                Debug.LogWarning(
                    "No standalone EventSystem assigned for Battle testing.",
                    this
                );
            }

            SetupBattle();
        }
    #endif        
    }

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
        {
            return;
        }

        if (standaloneAudioListener == null)
        {
            Debug.LogWarning(
                "No standalone AudioListener assigned for Battle.",
                this
            );
            return;
        }

        standaloneAudioListener.enabled = MusicSystem.Instance == null;
    }
}
