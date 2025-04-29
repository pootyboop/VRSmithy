using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Grippable : MonoBehaviour, IInteractable
{
    //interaction info
    private bool isInteractable = true;
    public int interactionPriority = 1; //0 or higher

    //grip transforms
    public Transform primaryGripTransform, secondaryGripTransform;

    void Awake()
    {
        if (primaryGripTransform == null) {
            primaryGripTransform = transform;
        }
    }
    
    public bool GetInteractable() {
        return isInteractable;
    }
    public void SetInteractable(bool newInteractable) {
        isInteractable = newInteractable;
    }
    public int GetInteractionPriority() {
        return interactionPriority;
    }



    public void InteractStart(GameHand hand) {
        if (!isInteractable) {
            return;
        }
        isInteractable = false;
        hand.SetGripping(this, true);
    }



    public void InteractStop(GameHand hand) {
        isInteractable = true;
        hand.SetGripping(this, false);
    }
}