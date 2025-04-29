using UnityEngine;

public class Implement : Item
{
    void Awake()
    {
        interactionPriority = 3;
        name = "Implement";
        itemCategory = EItemCategory.Implement;
        stowable = EStowableState.HOLSTER;
    }
}
