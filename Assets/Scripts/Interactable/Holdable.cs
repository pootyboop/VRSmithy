using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Holdable : Grippable
{
    Rigidbody rb;
    


    private void Awake() {
        rb = GetComponent<Rigidbody>();
        interactionPriority = 2;
    }



    public new void InteractStart(GameHand hand) {
        base.InteractStart(hand);
    }



    public new void InteractStop(GameHand hand) {
        base.InteractStop(hand);
    }
}
