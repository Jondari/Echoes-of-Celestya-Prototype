using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class PlayerPawnUI : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image pawnImage;
    [SerializeField] private RectTransform pawnTransform;

    [SerializeField] private Vector2 startPosition = new(-563,-452);
    [SerializeField] private Vector2[] battlePositions ={new(-563, -325), new(-766, -191), new(-577, -71)};

    [SerializeField] private CanvasGroup mapCanvasGroup;
    [SerializeField, Min(0f)] private float moveDuration = 0.5f;
    private bool isMoving;
    private Tween moveTween;

    private void OnEnable()
    {
        if (pawnImage == null || pawnTransform == null)
        {
            Debug.LogError("Player pawn references are missing.", this);
            return;
        }

        pawnTransform.anchoredPosition = startPosition;
        pawnImage.enabled = false;

        RunState run = GameSession.Instance != null
            ? GameSession.Instance.CurrentRun
            : null;

        if (run == null || run.SelectedHero == null)
        {
            return;
        }

        pawnImage.sprite = run.SelectedHero.Image;
        pawnImage.enabled = pawnImage.sprite != null;

        int lastCompletedBattle = -1;

        for (int i = 0; i < run.BattleCompleted.Length; i++)
        {
            if (!run.BattleCompleted[i])
            {
                break;
            }

            lastCompletedBattle = i;
        }

        if (lastCompletedBattle < 0)
        {
            return;
        }

        if (battlePositions == null
            || lastCompletedBattle >= battlePositions.Length)
        {
            Debug.LogError(
                "No pawn position configured for the completed battle.",
                this);
            return;
        }

        pawnTransform.anchoredPosition =
            battlePositions[lastCompletedBattle];
    }

    public void MoveToBattle(int battleIndex)
    {
        if (isMoving || !isActiveAndEnabled)
        {
            return;
        }

        RunState run = GameSession.Instance != null
            ? GameSession.Instance.CurrentRun
            : null;

        if (run == null
            || !run.IsBattleAvailable(battleIndex)
            || BattleSceneController.Instance == null
            || SceneManager.GetSceneByName("Battle").isLoaded)
        {
            return;
        }

        if (pawnTransform == null
            || mapCanvasGroup == null
            || battlePositions == null
            || battleIndex < 0
            || battleIndex >= battlePositions.Length)
        {
            Debug.LogError("Missing pawn movement configuration.", this);
            return;
        }

        isMoving = true;
        mapCanvasGroup.interactable = false;

        StartCoroutine(MoveToBattleRoutine(battleIndex));
    }

    private IEnumerator MoveToBattleRoutine(int battleIndex)
    {
        Vector2 destination = battlePositions[battleIndex];

        if (moveDuration > 0f)
        {
            moveTween = pawnTransform
                .DOAnchorPos(destination, moveDuration)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);

            yield return moveTween.WaitForCompletion();
            moveTween = null;
        }
        else
        {
            pawnTransform.anchoredPosition = destination;
        }

        BattleSceneController.Instance.LoadBattle(battleIndex);

        // LoadBattle normally hides MapPanel immediately.
        // Restore interaction if the request was not accepted.
        if (isActiveAndEnabled)
        {
            isMoving = false;
            mapCanvasGroup.interactable = true;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        moveTween?.Kill();
        moveTween = null;

        isMoving = false;

        if (mapCanvasGroup != null)
        {
            mapCanvasGroup.interactable = true;
        }
    }
}
