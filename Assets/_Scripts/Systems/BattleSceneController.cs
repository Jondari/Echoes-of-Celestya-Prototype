using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleSceneController : MonoBehaviour
{
    private bool isLoading = false;

    public void LoadBattle()
    {
        if (!isLoading && !SceneManager.GetSceneByName("Battle").isLoaded)
        {
            isLoading = true;
            StartCoroutine(LoadBattleRoutine());
        }
    }

    private IEnumerator LoadBattleRoutine()
    {
        GameFlowSystem.Instance.HideAllPanels();
        yield return SceneManager.LoadSceneAsync(
            "Battle",
            LoadSceneMode.Additive
        );
        Scene battleScene = SceneManager.GetSceneByName("Battle");
        SceneManager.SetActiveScene(battleScene);
        MatchSetupSystem.Instance.SetupBattle();
        isLoading = false;
    }
}
