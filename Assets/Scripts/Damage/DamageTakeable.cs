using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;



public interface IDamageable
{
    public void Damage(Damage damage);
    public DamageResistance GetDamageResistance();
}

public enum EDamageability
{
    DAMAGEABLE_NUMBERS, //show damage numbers when damage is dealt.
    DAMAGEABLE_NO_NUMBERS,  //deal damage but dont show numbers.
    UNDAMAGEABLE_NUMBERS,   //dont deal damage but show numbers you wouldve dealt.
    UNDAMAGEABLE_NO_NUMBERS //dont deal damage or show numbers.
}



public class DamageTakeable : MonoBehaviour, IDamageable
{
    //refs
    public DamageTakeable owningDamageTakeable; //optional. damage dealt to this object gets dealt to this reference too. avoid infinite loops!
    IEnumerator effectsCoroutine;

    //vals
    [Min(0)] public float maxHealth = 1f;    //max health, and what this object's health will start at
    public EDamageability damageability = EDamageability.DAMAGEABLE_NUMBERS;
    public DamageResistance resistance = new();
    [SerializeField] DamageThreshhold[] threshholds;

    //state
    float health;   //current health
    List<DamageEffect> activeEffects = new();

    void Awake()
    {
        health = maxHealth;
    }

    void Start()
    {
        CheckShouldKill();
    }

    public DamageResistance GetDamageResistance()
    {
        return resistance;
    }

    public void Damage(Damage damage)
    {

        foreach (var threshhold in threshholds)
        {
            threshhold.TryApplyDamage(damage);
        }

        if (owningDamageTakeable != null)
        {
            owningDamageTakeable.Damage(damage);
        }

        if (IsDamageable())
        {
            AddHealth(-damage.damageAmount);
        }

        foreach (var effect in damage.stats.effects)
        {
            ApplyEffect(effect);
        }
    }

    public void AddHealth(float amount)
    {
        health += amount;
        CheckShouldKill();
    }

    void CheckShouldKill()
    {
        if (health <= 0f)
        {
            Kill();
        }
    }

    public void Kill()
    {
        if (!IsDamageable())
        {
            return;
        }

        //kill code here
        print(this + ": Dead!");
    }

    public void ApplyEffect(DamageEffect effect)
    {
        bool startTracking = activeEffects.Count > 0;

        activeEffects.Add(effect);

        if (startTracking)
            SetTrackingEffects(true);
    }

    void SetTrackingEffects(bool newTracking)
    {
        if (effectsCoroutine != null)
        {
            StopCoroutine(effectsCoroutine);
            effectsCoroutine = null;
        }

        if (newTracking)
        {
            effectsCoroutine = UpdateEffects();
            StartCoroutine(effectsCoroutine);
        }
    }

    IEnumerator UpdateEffects()
    {
        while (activeEffects.Count > 0)
        {

            yield return null;
        }

        SetTrackingEffects(false);
        yield break;
    }

    bool IsDamageable()
    {
        switch (damageability)
        {
            case EDamageability.DAMAGEABLE_NUMBERS:
            case EDamageability.DAMAGEABLE_NO_NUMBERS:
                return true;
            default:
                return false;
        }
    }

    bool ShowsDamageNumbers()
    {
        switch (damageability)
        {
            case EDamageability.DAMAGEABLE_NUMBERS:
            case EDamageability.UNDAMAGEABLE_NUMBERS:
                return true;
            default:
                return false;
        }
    }
}



[Serializable]
public class DamageThreshhold
{
    [Min(0.001f)] [Tooltip("The amount of damage required to cross the threshhold")]
    public float damageThreshhold = 1f;
    float damageTaken = 0f;
     [Tooltip("Physical damage types that this threshhold will accept and take damage from.")]
    public EPhysicalDamageType[] applicableDamageTypes;
     [Tooltip("Damage effect types that this threshhold will accept and take damage from.")]
    public EDamageEffectType[] applicableEffectTypes;
     [Tooltip("Amount of times this threshhold can be reused (resetting after each threshhold is crossed). Set to -1 for infinite uses.")]
    [Min(1)] public int usesBeforeExpiry = 1;
    public UnityEvent onThreshholdCrossed;

    public void TryApplyDamage(Damage damage)
    {
        if (IsThreshholdTracking() && IsDamageApplicable(damage))
        {
            damageTaken += damage.damageAmount;
            TryCrossThreshhold();
        }
    }

    void TryCrossThreshhold()
    {
        if (damageTaken >= damageThreshhold)
        {
            CrossThreshhold();
        }
    }

    void CrossThreshhold()
    {
        if (IsThreshholdTracking())
        {
            onThreshholdCrossed.Invoke();
            damageTaken = 0f;
            if (usesBeforeExpiry != -1)
            {
                usesBeforeExpiry--;
            }
        }
    }

    bool IsThreshholdTracking() {
        return usesBeforeExpiry > 0 || usesBeforeExpiry == -1;
    }

    bool IsDamageApplicable(Damage damage)
    {
        if (applicableDamageTypes.Contains(damage.stats.physical.type))
        {
            return true;
        }

        foreach (var effect in damage.stats.effects)
        {
            if (applicableEffectTypes.Contains(effect.type))
            {
                return true;
            }
        }

        return false;
    }
}