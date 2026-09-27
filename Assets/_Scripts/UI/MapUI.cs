using UnityEngine;

public class MapUI : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button[] battleButtons;

    private void OnEnable()
    {
        RefreshButtons();
    }

    private void RefreshButtons()
    {
        RunState run = null;

        if (GameSession.Instance != null)
        {
            run = GameSession.Instance.CurrentRun;
        }

        for (var i = 0;  i < battleButtons.Length; i ++)
        {
            if (battleButtons[i] == null)
            {
                Debug.LogWarning($"Battle button at index {i} is not assigned.", this);
                continue;
            }

            battleButtons[i].interactable = run != null && run.IsBattleAvailable(i);
        }
    }
}
