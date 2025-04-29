using UnityEngine;

//used for a larger hitbox for interactables
[RequireComponent(typeof(Collider))]
public class InteractableRedirector : MonoBehaviour, IInteractable
{
    //interaction info
    public bool isInteractable = true;
    public int interactionPriority = 4;
    public IInteractable targetInteractable;

    //grip transforms
    public Transform primaryGripTransform, secondaryGripTransform;

    void Awake()
    {
        if (primaryGripTransform == null) {
            primaryGripTransform = transform;
        }
    }
    void Start()
    {
        SetTargetInteractable(targetInteractable);
    }

    public void SetTargetInteractable(IInteractable newTargetInteractable) {
        targetInteractable = newTargetInteractable;
        SetInteractable(targetInteractable != null);
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
        targetInteractable.InteractStart(hand);
    }



    public void InteractStop(GameHand hand) {
        targetInteractable.InteractStop(hand);
    }
}