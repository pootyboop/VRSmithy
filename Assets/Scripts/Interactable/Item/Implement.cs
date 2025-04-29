using UnityEngine;

public class Implement : Item
{
    public new int interactionPriority = 3;
    public new string name = "Implement";
    public new EItemCategory itemCategory = EItemCategory.Implement;
    public new EStowableState stowable = EStowableState.HOLSTER;
}
