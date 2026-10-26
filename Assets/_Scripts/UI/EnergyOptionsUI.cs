using TMPro;
using UnityEngine;

public class EnergyOptionsUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField initialEnergyInput;
    [SerializeField] private TMP_InputField energyPerTurnInput;
    [SerializeField] private TMP_InputField maxEnergyInput;
    [SerializeField] private UnityEngine.UI.Toggle carryOverToggle;
    [SerializeField] private TMP_Text errorText;

    private void OnEnable()
    {
        errorText.text = "";

        if (GameSession.Instance == null)
        {
            Debug.LogWarning(
                "No GameSession available to load energy settings.",
                this
            );
            return;
        }

        EnergySettings settings = GameSession.Instance.EnergySettings;

        initialEnergyInput.text = settings.InitialEnergy.ToString();
        energyPerTurnInput.text = settings.EnergyPerTurn.ToString();
        maxEnergyInput.text = settings.MaxEnergy.ToString();
        carryOverToggle.isOn = settings.CarryOver;
    }

    public void ApplySettings()
    {
        errorText.text = "";

        if (!int.TryParse(initialEnergyInput.text, out int initialEnergy)
            || !int.TryParse(energyPerTurnInput.text, out int energyPerTurn)
            || !int.TryParse(maxEnergyInput.text, out int maxEnergy))
        {
            errorText.text = "Enter a valid integer in each field.";
            return;
        }

        if (initialEnergy < 0 || energyPerTurn < 0)
        {
            errorText.text = "Energy values cannot be negative.";
            return;
        }

        if (maxEnergy < 1)
        {
            errorText.text = "Maximum energy must be at least 1.";
            return;
        }

        if (initialEnergy > maxEnergy || energyPerTurn > maxEnergy)
        {
            errorText.text = "Energy values cannot exceed maximum energy.";
            return;
        }

        if (GameSession.Instance == null)
        {
            Debug.LogWarning(
                "No GameSession available to apply energy settings.",
                this
            );
            return;
        }

        EnergySettings settings = new EnergySettings
        {
            InitialEnergy = initialEnergy,
            EnergyPerTurn = energyPerTurn,
            MaxEnergy = maxEnergy,
            CarryOver = carryOverToggle.isOn
        };

        GameSession.Instance.ApplyEnergySettings(settings);
        GameFlowSystem.Instance.ShowMainMenu();
    }
}
