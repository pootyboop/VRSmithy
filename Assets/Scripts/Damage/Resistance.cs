using System;
using System.Collections.Generic;
using UnityEngine;



[Serializable]
public class DamageResistance
{
    public PhysicalDamageResistance[] physical;
    public DamageEffectResistance[] effects;
    [Range(0f, 1f)] public float toughness = 0.5f;    //resistance to sharpness. directly correlates to physical damage sharpness from 0 to 1
    [Range(0f, 1f)] public float stability = 0.5f;  //resistance multiplier to knockback from 0 to 1

    public Damage ApplyResistanceToDamage(Damage damage)
    {
        Damage resDamage = damage;
        float physicalRes = GetPhysicalResistance(damage.stats.physical.type);
        resDamage.damageAmount *= physicalRes;
        //resistedDamage.damageAmount *= resistedDamage.stats.physical.sharpness - toughness;
        resDamage.stats.physical.knockback *= 1f - stability;

        List<DamageEffect> resEffects = new();
        foreach (var effect in damage.stats.effects)
        {
            DamageEffect newEffect = ApplyResistanceToEffect(effect);
            if (newEffect != null) {
                resEffects.Add(newEffect);
            }
        }

        resDamage.stats.effects = resEffects.ToArray();

        float efficacy = physicalRes;
        DamageManager.instance.SpawnDamageNumber(resDamage, efficacy);

        return resDamage;
    }

    float GetPhysicalResistance(EPhysicalDamageType physicalType)
    {
        foreach (var res in physical)
        {
            if (res.type == physicalType)
            {
                return res.resistance;
            }
        }

        return 0f;
    }

    DamageEffect ApplyResistanceToEffect(DamageEffect effect)
    {
        DamageEffectResistance effectRes = GetEffectResistance(effect.type);
        if (!effectRes.receiveEffect) {
            return null;
        }
        effect.amplitude *= effectRes.resistance;
        effect.duration *= effectRes.resistance;
        return effect;
    }

    DamageEffectResistance GetEffectResistance(EDamageEffectType effectType)
    {
        foreach (var res in effects)
        {
            if (res.type == effectType)
            {
                return res;
            }
        }

        return new()
        {
            type = effectType
        };
    }
}



[Serializable]
public class PhysicalDamageResistance
{
    public EPhysicalDamageType type;  //type of damage
    public float resistance = 1f;   //multiplier to damage received. so 1.5 = +50% damage, 0.75 = -25% (resulting in 75%) damage
}



[Serializable]
public class DamageEffectResistance
{
    public EDamageEffectType type;  //type of effect
    public bool receiveEffect = true;   //whether this effect can be applied at all to the object
    public float resistance = 1f;   //multiplier to effect amplitude and duration. so 1.5 = +50% damage, 0.75 = -25% (resulting in 75%) damage
}