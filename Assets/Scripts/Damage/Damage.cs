using System;
using UnityEngine;



[Serializable]
public class Damage
{
    public DamageStats stats = new(); //all the info about the damage being dealt
    public DamageDealable dealer;   //the source of the damage, e.g. the player who shot a gun
    public GameObject literalDealer;    //the physical damager, e.g. the bullet shot by the player
    public float damageAmount = 0f;  //the final, true amount of damage to dealt (i.e. velocity is factored in)
    public Vector3 direction = Vector3.zero;   //the direction and magnitude from which the damage was dealt
    public Vector3 position = Vector3.zero; //the position from which the damage was dealt

}



[Serializable]
public class DamageStats
{
    public PhysicalDamageStats physical = new();
    public DamageEffect[] effects;
}

public enum EPhysicalDamageType
{
    UNSET,
    IMPALING,
    LATCHING,
    CUTTING,
    SHREDDING,
    IMPACT
}

[Serializable]
public class PhysicalDamageStats
{
    public EPhysicalDamageType type = EPhysicalDamageType.UNSET;    //type of physical damage
    public float rawDamage = 0f;    //base raw damage dealt by this dealer, not factoring in velocity etc.
    [Range(0f, 1f)] public float sharpness = 0f;    //determines how strong/soft surfaces are affected by this damage. 0 is fully blunt, 1 is fully sharp
    public float knockback = 0f;    //how much knockback force this damage applies to damagetakeables from its position/direction
}

public enum EDamageEffectType
{
    ELECTRICAL,
    POISONING,
    GASOLINE
}

[Serializable]
public class DamageEffect
{
    public EDamageEffectType type;  //type of effect
    public float amplitude = 1f;    //the level of strength the effect is applied with
    public float duration = 30f;    //time in seconds to apply the effect for. this may not be relevant for all effect types
    public float radius = 0f;   //the radius the effect will AOE in. if 0, no AOE will occur and the damage will only affect directly hit damagetakeables
}