using UnityEngine;

public enum EComponentCategory
{
    Attachment_Point,
    Body,
    Activator,
    Functional,
    Tether,
    Attractor,
    Melee,
    Ranged,
    Damage_Enhancer,
    Shield
}

public enum EComponentSubcategory
{
    None,
    Handle,
    Brace,
    Arm,
    Trigger,
    Toggler,
    Power_Source,
    Spearhead,
    Blade,
    Hammer_Head,
    Coater,
    Conducer,
    Chamber,
    Barrel,
    Muzzle,
    Magazine,
    Bowarm,
    War_Door,
    Buckler
}



public class Component : Item
{
    public EComponentCategory componentCategory = EComponentCategory.Body;
    public EComponentSubcategory componentSubcategory = EComponentSubcategory.None;



    void Awake()
    {
        name = "Component";
        itemCategory = EItemCategory.Component;
    }
}