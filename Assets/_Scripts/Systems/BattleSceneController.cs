using System.Collections;
using UnityEngine.SceneManagement;

public class BattleSceneController : Singleton<BattleSceneController>
{
    private bool isLoading = false;

    public void LoadBattle(int battleIndex)
    {
        if (GameSession.Instance != null 
            && GameSession.Instance.CurrentRun != null 
            && GameSession.Instance.CurrentRun.IsBattleAvailable(battleIndex))
        {
            if (!isLoading && !SceneManager.GetSceneByName("Battle").isLoaded)
            {
                GameSession.Instance.CurrentRun.UpdateCurrentEncounter(battleIndex);
                isLoading = true;
                StartCoroutine(LoadBattleRoutine());
            }
        }
    }

    private IEnumerator LoadBattleRoutine()
    {
        GameFlowSystem.Instance.HideAllPanels();

        if (MusicSystem.Instance != null)
        {
            MusicSystem.Instance.PlayDefaultMusic();
        }

        yield return SceneManager.LoadSceneAsync(
            "Battle",
            LoadSceneMode.Additive
        );
        Scene battleScene = SceneManager.GetSceneByName("Battle");
        SceneManager.SetActiveScene(battleScene);
        MatchSetupSystem.Instance.SetupBattle();
        isLoading = false;
    }

    public void RetryBattle()
    {
        if (!isLoading && SceneManager.GetSceneByName("Battle").isLoaded)
        {
            isLoading = true;
            StartCoroutine(RetryBattleRoutine());
        }
    }

    private IEnumerator RetryBattleRoutine()
    {
        Scene gameScene = SceneManager.GetSceneByName("Game");
        SceneManager.SetActiveScene(gameScene);
        yield return SceneManager.UnloadSceneAsync("Battle");
        yield return LoadBattleRoutine();
    }

    public void OpenRewards()
    {
        if (!isLoading && SceneManager.GetSceneByName("Battle").isLoaded)
        {
            isLoading = true;
            StartCoroutine(OpenRewardsRoutine());
        }
    }

    private IEnumerator OpenRewardsRoutine()
    {
        Scene gameScene = SceneManager.GetSceneByName("Game");
        SceneManager.SetActiveScene(gameScene);

        yield return SceneManager.UnloadSceneAsync("Battle");

        RewardSystem.Instance.SetupRewards();

        GameFlowSystem.Instance.ShowRewards();
        isLoading = false;
    }
}
