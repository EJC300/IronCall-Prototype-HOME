using UnityEngine;

public enum NutritionState { Starving, Optimal, OverNutrition }

[RequireComponent(typeof(PlayerController))]
public class PlayerNeeds : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;

    [Header("Nutrition")]
    [SerializeField] private float nutrition = 70f; // 0-100
    [SerializeField] private float nutritionLowThreshold = 30f;
    [SerializeField] private float nutritionHighThreshold = 70f;
    [SerializeField] private float nutritionDecayRate = 0.5f;   // passive, per second
    [SerializeField] private float nutritionActivityDrain = 1f; // extra per second while moving
    public NutritionState CurrentNutritionState { get; private set; }

    [Header("Weight (Fat Mass)")]
    [SerializeField] private float weight = 100f;
    [SerializeField] private float starveWeightLossRate = 2f;
    [SerializeField] private float optimalFatLossRate = 0.5f;
    [SerializeField] private float overNutritionWeightGainRate = 1f;

    [Header("Fitness")]
    [SerializeField] private float fitness = 20f; // 0-100
    [SerializeField] private float fitnessGainRate = 2f;             // per second, active + optimal
    [SerializeField] private float fitnessLossRateOvernutrition = 1f; // per second while overnutrition

    [Header("Stress")]
    [SerializeField] private float stress = 20f; // 0-100 composite, this is THE stress meter
    [SerializeField] private float generalStress = 0f;
    [SerializeField] private float generalStressKick = 15f;    // per K press
    [SerializeField] private float generalStressDecayRate = 5f; // per second
    [SerializeField] private float generalStressContribution = 0.01f; // scales generalStress -> stress/sec
    [SerializeField] private float fitnessStressRelief = 0.05f; // per fitness point per second
    [SerializeField] private float mentalStressFactor = 0.1f;   // per mental-deviation point per second
    [SerializeField] private KeyCode stressKey = KeyCode.K;
    // Others not implemented: hook additional contributors into UpdateStress() below

    [Header("Mental")]
    [SerializeField] private float mental = 50f; // 0-100, 50 = neutral
    [SerializeField] private float mentalChangeAmount = 10f; // per key press
    [SerializeField] private KeyCode mentalUpKey = KeyCode.Z;
    [SerializeField] private KeyCode mentalDownKey = KeyCode.X;

    [Header("Damage / Health")]
    [SerializeField] private float damage = 0f; // 0-100, 100 = dead
    [SerializeField] private float damageKeyAmount = 10f;
    [SerializeField] private KeyCode damageKey = KeyCode.T;
    [SerializeField] private float starveDamageRate = 3f;          // per second
    [SerializeField] private float optimalHealRate = 4f;           // per second, max (at zero stress)
    [SerializeField] private float overNutritionDamageRate = 0.2f; // per second, very slow
    [SerializeField] private float maxSpeedPenaltyAtFullDamage = 0.8f; // fraction of speed lost at 100 dmg

    [Header("Energy / Stamina")]
    [SerializeField] private float energy = 100f; // 0-100
    [SerializeField] private float staminaDrainRate = 8f;
    [SerializeField] private float staminaRegenRate = 5f;
    [SerializeField] private float activityVelocityThreshold = 0.2f;

    public bool IsDead { get; private set; }
    public event System.Action OnDeath;

    public float Nutrition => nutrition;
    public float Weight => weight;
    public float Fitness => fitness;
    public float Stress => stress;
    public float Mental => mental;
    public float Damage => damage;
    public float Energy => energy;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (IsDead) return;

        HandleInput();

        bool isActive = IsPlayerActive();

        UpdateNutrition(isActive, Time.deltaTime);
        UpdateWeight(Time.deltaTime);
        UpdateFitness(isActive, Time.deltaTime);
        mental = Mathf.Clamp(mental, 0f, 100f); // Z/X already applied in HandleInput
        UpdateStress(Time.deltaTime);
        UpdateDamage(Time.deltaTime);
        UpdateEnergy(isActive, Time.deltaTime);

        ApplyMovementPenalty();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(stressKey))
            generalStress = Mathf.Clamp(generalStress + generalStressKick, 0f, 100f);

        if (Input.GetKeyDown(mentalUpKey))
            mental = Mathf.Clamp(mental + mentalChangeAmount, 0f, 100f);

        if (Input.GetKeyDown(mentalDownKey))
            mental = Mathf.Clamp(mental - mentalChangeAmount, 0f, 100f);

        if (Input.GetKeyDown(damageKey))
            ApplyDamage(damageKeyAmount);
    }

    private bool IsPlayerActive()
    {
        if (playerController == null || playerController.rb == null) return false;
        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(playerController.rb.linearVelocity, transform.up);
        return horizontalVelocity.magnitude > activityVelocityThreshold;
    }

    private void UpdateNutrition(bool isActive, float dt)
    {
        nutrition -= nutritionDecayRate * dt;
        if (isActive) nutrition -= nutritionActivityDrain * dt;
        nutrition = Mathf.Clamp(nutrition, 0f, 100f);

        if (nutrition < nutritionLowThreshold) CurrentNutritionState = NutritionState.Starving;
        else if (nutrition > nutritionHighThreshold) CurrentNutritionState = NutritionState.OverNutrition;
        else CurrentNutritionState = NutritionState.Optimal;
    }

    public void EatFood(float nutritionAmount)
    {
        nutrition = Mathf.Clamp(nutrition + nutritionAmount, 0f, 100f);
    }

    private void UpdateWeight(float dt)
    {
        switch (CurrentNutritionState)
        {
            case NutritionState.Starving: weight -= starveWeightLossRate * dt; break;
            case NutritionState.Optimal: weight -= optimalFatLossRate * dt; break;
            case NutritionState.OverNutrition: weight += overNutritionWeightGainRate * dt; break;
        }
        weight = Mathf.Clamp(weight, 0f, 300f); // arbitrary prototype ceiling
    }

    private void UpdateFitness(bool isActive, float dt)
    {
        if (CurrentNutritionState == NutritionState.OverNutrition)
            fitness -= fitnessLossRateOvernutrition * dt;
        else if (isActive && CurrentNutritionState == NutritionState.Optimal)
            fitness += fitnessGainRate * dt;
        // Starving: fitness held steady here — change if you want it to erode too

        fitness = Mathf.Clamp(fitness, 0f, 100f);
    }

    private void UpdateStress(float dt)
    {
        generalStress = Mathf.Clamp(generalStress - generalStressDecayRate * dt, 0f, 100f);

        float mentalDeviation = 50f - mental; // positive when mental is below neutral

        float stressDelta =
            generalStress * generalStressContribution * dt
            - fitness * fitnessStressRelief * dt
            + mentalDeviation * mentalStressFactor * dt;

        stress = Mathf.Clamp(stress + stressDelta, 0f, 100f);
    }

    private void UpdateDamage(float dt)
    {
        float healthChangeRate = 0f; // + worsens, - heals

        switch (CurrentNutritionState)
        {
            case NutritionState.Starving:
                healthChangeRate = starveDamageRate;
                break;
            case NutritionState.Optimal:
                float healEfficiency = 1f - (stress / 100f); // high stress cripples recharge
                healthChangeRate = -optimalHealRate * healEfficiency;
                break;
            case NutritionState.OverNutrition:
                healthChangeRate = overNutritionDamageRate;
                break;
        }

        damage = Mathf.Clamp(damage + healthChangeRate * dt, 0f, 100f);

        if (damage >= 100f && !IsDead) Die();
    }

    public void ApplyDamage(float amount)
    {
        if (IsDead) return;
        damage = Mathf.Clamp(damage + amount, 0f, 100f);
        if (damage >= 100f) Die();
    }

    private void Die()
    {
        IsDead = true;
        OnDeath?.Invoke();
        // Hook ragdoll / respawn / game-over UI here
    }

    private void UpdateEnergy(bool isActive, float dt)
    {
        energy += (isActive ? -staminaDrainRate : staminaRegenRate) * dt;
        energy = Mathf.Clamp(energy, 0f, 100f);
    }

    private void ApplyMovementPenalty()
    {
        if (playerController == null) return;
        float damagePenalty = (damage / 100f) * maxSpeedPenaltyAtFullDamage;
        playerController.SpeedMultiplier = Mathf.Clamp01(1f - damagePenalty);
    }
}
