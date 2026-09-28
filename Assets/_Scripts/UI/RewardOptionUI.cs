using TMPro;
using UnityEngine;

public class RewardOptionUI : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image artwork;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private UnityEngine.UI.Button button;
    private CardData cardData;

    public void Setup(CardData card)
    {
        cardData = card;
        nameText.text = card.name;
        costText.text = card.Mana.ToString();
        descriptionText.text = card.Description;
        artwork.sprite = card.Image;
    }

    public void OnClicked()
    {
        if (cardData == null)
        {
            return;
        }

        RewardSystem.Instance.SelectReward(cardData);
    }

    public void SetInteractable(bool value)
    {
        button.interactable = value;
    }
}
