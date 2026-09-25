using UnityEngine;

public class GameFlowSystem : Singleton<GameFlowSystem>
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject heroSelectionPanel;
    [SerializeField] private GameObject mapPanel;

    void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        mainMenuPanel.SetActive(true);
    }

    public void ShowHeroSelection()
    {
        HideAllPanels();
        heroSelectionPanel.SetActive(true);
    }

    public void ShowMap()
    {
        HideAllPanels();
        mapPanel.SetActive(true);
    }

    public void HideAllPanels() 
    {
        mainMenuPanel.SetActive(false);
        heroSelectionPanel.SetActive(false);
        mapPanel.SetActive(false);
    }
}
