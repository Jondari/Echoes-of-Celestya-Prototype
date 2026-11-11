using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatantView : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private StatusEffectsUI statusEffectsUI;

    [Header("Idle Bounce")]
    [SerializeField] private bool enableIdleBounce = true;
    [SerializeField, Range(0f, 0.15f)] private float idleBounceAmount = 0.0104f;
    [Tooltip("Full bounce cycle duration. Each combatant gets a random speed and starting phase.")]
    [SerializeField] private Vector2 idleBounceCycleDuration = new(2.4f, 3.6f);

    private Vector3 initialSpriteScale;
    private Vector3 initialSpritePosition;
    private Tween idleBounceTween;
    private bool previousEnableIdleBounce;

    public int MaxHealth {  get; private set; }
    public int CurrentHealth {  get; private set; }
    private Dictionary<StatusEffectType, int> statusEffects = new();

    private void Awake()
    {
        initialSpriteScale = spriteRenderer.transform.localScale;
        initialSpritePosition = spriteRenderer.transform.localPosition;
    }

    private void OnEnable()
    {
        if (CurrentHealth > 0)
        {
            StartIdleBounce();
        }
    }

    private void Update()
    {
        // Apply Inspector toggle changes during Play Mode without restarting the tween every frame.
        if (enableIdleBounce != previousEnableIdleBounce)
        {
            StartIdleBounce();
        }
    }

    private void OnDisable()
    {
        StopIdleBounce();
    }

    private void StartIdleBounce()
    {
        previousEnableIdleBounce = enableIdleBounce;
        StopIdleBounce();
        if (!enableIdleBounce || !isActiveAndEnabled || CurrentHealth <= 0 || idleBounceAmount <= 0f)
        {
            return;
        }

        // Animate only the sprite so attack movement, health text and status icons stay independent.
        Transform visual = spriteRenderer.transform;
        float spriteBottom = spriteRenderer.sprite != null ? spriteRenderer.sprite.bounds.min.y : 0f;
        Vector3 stretchedScale = Vector3.Scale(initialSpriteScale,
            new Vector3(1f - idleBounceAmount, 1f + idleBounceAmount, 1f));
        Vector3 squashedScale = Vector3.Scale(initialSpriteScale,
            new Vector3(1f + idleBounceAmount, 1f - idleBounceAmount, 1f));
        float minDuration = Mathf.Max(0.2f, idleBounceCycleDuration.x);
        float maxDuration = Mathf.Max(minDuration, idleBounceCycleDuration.y);
        float cycleDuration = Random.Range(minDuration, maxDuration);

        // Oscillate continuously between the two shapes, without pauses or a reset between cycles.
        visual.localScale = squashedScale;
        idleBounceTween = visual.DOScale(stretchedScale, cycleDuration * 0.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
            .OnUpdate(() =>
            {
                // Compensate for scaling around the pivot so the sprite's bottom stays anchored.
                float offsetY = spriteBottom * (initialSpriteScale.y - visual.localScale.y);
                visual.localPosition = initialSpritePosition
                    + visual.localRotation * new Vector3(0f, offsetY, 0f);
            });

        // Start at a random point in the cycle so combatants do not bounce in sync.
        idleBounceTween.Goto(Random.Range(0f, cycleDuration), true);
    }

    private void StopIdleBounce()
    {
        idleBounceTween?.Kill();
        idleBounceTween = null;
        if (spriteRenderer != null)
        {
            spriteRenderer.transform.localScale = initialSpriteScale;
            spriteRenderer.transform.localPosition = initialSpritePosition;
        }
    }

    protected void SetupBase(int health, Sprite image)
    {
        MaxHealth = CurrentHealth = health;
        spriteRenderer.sprite = image;
        UpdateHealthText();
        StartIdleBounce();
    }

    private void UpdateHealthText()
    {
        healthText.text = "HP: " + CurrentHealth;
    }

    public void Damage(int damageAmount)
    {
        int remainingDamage = damageAmount;
        int currentArmor = GetStatusEffectStacks(StatusEffectType.ARMOR);
        if(currentArmor > 0)
        {
            if (currentArmor >= damageAmount)
            {
                RemoveStatusEffect(StatusEffectType.ARMOR, remainingDamage);
                remainingDamage = 0;
            }
            else if (currentArmor < damageAmount)
            {
                RemoveStatusEffect(StatusEffectType.ARMOR, currentArmor);
                remainingDamage -= currentArmor;
            }
        }
        if (remainingDamage > 0)
        {
            CurrentHealth -= remainingDamage;
            if (CurrentHealth < 0)
            {
                CurrentHealth = 0;
            }
        }
        if (CurrentHealth <= 0)
        {
            StopIdleBounce();
        }
        // shaking animation
        transform.DOShakePosition(0.2f, 0.5f).SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        UpdateHealthText() ;
    }

    public void AddStatusEffect(StatusEffectType type, int stackCount)
    {
        if (statusEffects.ContainsKey(type))
        {
            statusEffects[type] += stackCount;
        }
        else
        {
            statusEffects.Add(type, stackCount);
        }
        statusEffectsUI.UpdateStatusEffectUI(type, GetStatusEffectStacks(type));
    }

    public void RemoveStatusEffect(StatusEffectType type, int stackCount)
    {
        if (statusEffects.ContainsKey(type))
        {
            statusEffects[type] -= stackCount;
            if (statusEffects[type] <= 0)
            {
                statusEffects.Remove(type);
            }
        }
        statusEffectsUI.UpdateStatusEffectUI(type, GetStatusEffectStacks(type));
    }

    public int GetStatusEffectStacks(StatusEffectType type)
    {
        if (statusEffects.ContainsKey(type))
            return statusEffects[type];
        else
            return 0;
    }

    public void Heal(int amount)
    {
        if (CurrentHealth <= 0 || amount <= 0)
        {
            return;
        }

        int recoveredHealth = Mathf.Min(amount, MaxHealth - CurrentHealth);
        CurrentHealth += recoveredHealth;
        UpdateHealthText();
    }

    /// <summary>
    /// Returns the fixed attack modifier from active statuses.
    /// Stacks determine duration, not strength.
    /// </summary>
    public int GetAttackBonus()
    {
        int bonus = GetStatusEffectStacks(StatusEffectType.ATTACK_BUFF) > 0
            ? 2 : 0;

        int penalty = GetStatusEffectStacks(StatusEffectType.ATTACK_DEBUFF) > 0
            ? 2 : 0;

        return bonus - penalty;
    }
}
