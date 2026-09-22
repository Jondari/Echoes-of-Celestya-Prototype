using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleFlowSystem : Singleton<BattleFlowSystem>
{
    [SerializeField] private GameObject BattleResultPanel;
    [SerializeField] private TMP_Text ResultTitleText;
    [SerializeField] private TMP_Text ActionButtonText;

    public BattleState State {  get; private set; } = BattleState.PLAYING;

    private void ShowBattleResult()
    {
        if (State == BattleState.VICTORY)
        {
            UpdatePanelText("Victory", "Continue");
        } 
        else if (State == BattleState.DEFEAT)
        {
            UpdatePanelText("Game over", "Retry");
        }
        BattleResultPanel.SetActive(true);
        CardViewHoverSystem.Instance.Hide();
    }

    public void Victory()
    {
        if (State == BattleState.PLAYING)
        {
            State = BattleState.VICTORY;
            ShowBattleResult();
        }
    }

    public void Defeat()
    {
        if (State == BattleState.PLAYING)
        {
            State = BattleState.DEFEAT;
            ShowBattleResult();
        }
    }

    private void UpdatePanelText(string resultTitleText, string actionButtonText)
    {
        ResultTitleText.text = resultTitleText;
        ActionButtonText.text = actionButtonText;
    }

    public void ReloadCurrentBattle()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
