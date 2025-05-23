using UnityEngine;

public enum EItemCategory {
    Miscellaneous,
    Component,
    Implement,
    Material
}
public enum EStowableState {
    NONE,
    HOLSTER,
    INVENTORY
}

public class Item : Holdable
{
    
    [Header("Item Data")]
    public new string name = "Item";
    public EItemCategory itemCategory = EItemCategory.Miscellaneous;
    public string description = "An overview of the item's function/uses and how it was obtained.";
    public float baseValue = 1f;
    private float totalValue;
    public EStowableState stowable = EStowableState.INVENTORY;

    public void Awake()
    {
        base.Awake();
        CalculateTotalValue();
    }

    public float GetTotalValue() {
        return CalculateTotalValue();
    }
    float CalculateTotalValue() {
        float totalValue = baseValue;
        return totalValue;
    }
}
