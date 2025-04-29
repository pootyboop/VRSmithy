using UnityEngine;

public class Holdable : Grippable
{
    public new int interactionPriority = 2;
    public new void InteractStart(GameHand hand) {
        base.InteractStart(hand);
    }



    public new void InteractStop(GameHand hand) {
        base.InteractStop(hand);
    }
}
