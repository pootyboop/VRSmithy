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
    [SerializeField] Implement implement;



    void Reset()
    {
        name = "Component";
        itemCategory = EItemCategory.Component;
    }

    public void Awake()
    {
        base.Awake();
        TrySolveImplement();
    }

    public void OnCollisionEnter(Collision collision)
    {
        //print("comp collision on: " + this);
        if (TryGetComponent(out DamageDealable damageDealable))
        {
            damageDealable.OnCollisionEnter(collision);
        }
    }

    void TrySolveImplement()
    {
        if (implement == null)
        {
            Implement newImplement = transform.parent.GetComponent<Implement>();
            if (newImplement != null)
            {
                SetImplement(newImplement);
            }
        }

        else
        {
            SetImplement(implement);
        }
    }

    public void SetImplement(Implement newImplement)
    {
        if (implement != null)
        {

        }

        implement = newImplement;
        bool isAttachingToImplement = implement != null;

        //SetInteractable(!isAttachingToImplement);
        transform.SetParent(isAttachingToImplement ? implement.transform : null, true);

        if (isAttachingToImplement)
        {
            SetInteractable(true);
            SetRB(implement.GetRB());
        }

        else
        {
            SetRB(null);
        }

        implement.OnComponentAttachedChanged(this, isAttachingToImplement);
    }



    public override void InteractStartOverrideable(GameHand hand)
    {
        if (implement != null)
        {
            implement.InteractStartOverrideable(hand);
        }
        else
        {
            base.InteractStartOverrideable(hand);
        }
    }
    public override void InteractStopOverrideable(GameHand hand)
    {
        if (implement != null)
        {
            implement.InteractStopOverrideable(hand);
        }
        else
        {
            base.InteractStopOverrideable(hand);
        }
    }
    public override void SelectStopOverrideable(GameHand hand)
    {
        if (implement != null)
        {
            implement.SelectStopOverrideable(hand);
        }
        else
        {
            base.SelectStopOverrideable(hand);
        }
    }
}