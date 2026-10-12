using UnityEngine;

public class GameFlowSystem : Singleton<GameFlowSystem>
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject heroSelectionPanel;
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private GameObject rewardPanel;

    void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        mainMenuPanel.SetActive(true);

        if (MusicSystem.Instance != null)
        {
            MusicSystem.Instance.PlayDefaultMusic();
        }
    }

    public void ShowHeroSelection()
    {
        HideAllPanels();
        heroSelectionPanel.SetActive(true);

        if (MusicSystem.Instance != null)
        {
            MusicSystem.Instance.PlayDefaultMusic();
        }
    }

    public void ShowMap()
    {
        HideAllPanels();
        mapPanel.SetActive(true);

        if (MusicSystem.Instance != null)
        {
            MusicSystem.Instance.PlayMapMusic();
        }
    }

    public void ShowRewards()
    {
        HideAllPanels();
        rewardPanel.SetActive(true);

        if (MusicSystem.Instance != null)
        {
            MusicSystem.Instance.PlayMapMusic();
        }
    }

    public void HideAllPanels() 
    {
        mainMenuPanel.SetActive(false);
        heroSelectionPanel.SetActive(false);
        mapPanel.SetActive(false);
        rewardPanel.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
