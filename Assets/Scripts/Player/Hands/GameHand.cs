using System.Collections.Generic;
using UnityEngine;



public enum EGameHandState
{
    EMPTY,  //holding nothing
    GRIPPING,   //holding onto something that cannot necessarily be moved freely - e.g. ladder handle
    HOLDING //holding onto something that can be moved freely according to hand movements - e.g. weapon
}

[RequireComponent(typeof(SphereCollider))]
public class GameHand : HandBase
{
    //refs
    [SerializeField] private TrueHand trueHand;
    private SphereCollider pickupOverlap;

    //state
    EGameHandState state = EGameHandState.EMPTY;
    List<IInteractable> overlappedInteractables = new List<IInteractable>();
    IInteractable bestInteractable;
    Grippable grippedObject;



    void Awake()
    {
        pickupOverlap = GetComponent<SphereCollider>();
    }

    void Update()
    {
        UpdateMovement();
    }

    void UpdateMovement()
    {
        switch (state)
        {
            default:
            case EGameHandState.EMPTY:
                //stick to TrueHand position - no limitation
                transform.position = trueHand.transform.position;
                return;

            case EGameHandState.HOLDING:
            case EGameHandState.GRIPPING:
                //stick to currently held actor - no free movement
                return;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out IInteractable temp))
        {
            UpdateOverlappedInteractables(temp, true);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out IInteractable temp))
        {
            UpdateOverlappedInteractables(temp, false);
        }
    }

    void UpdateOverlappedInteractables(IInteractable currInteractable, bool isAdding)
    {
        if (isAdding)
            overlappedInteractables.Add(currInteractable);
        else
            overlappedInteractables.Remove(currInteractable);

        UpdateBestInteractable();
    }

    void UpdateBestInteractable()
    {
        if (grippedObject != null)
        {
            bestInteractable = grippedObject;
            return;
        }

        SolveNewBestInteractable();
    }

    void SolveNewBestInteractable()
    {
        IInteractable best = null;
        int bestPriority = -1;
        foreach (IInteractable interactable in overlappedInteractables)
        {
            if (interactable.GetInteractionPriority() > bestPriority)
            {
                best = interactable;
                bestPriority = interactable.GetInteractionPriority();
            }
        }
        bestInteractable = best;
    }

    public void SetGripping(Grippable grippable, bool shouldGrip)
    {
        if (shouldGrip)
        {
            grippedObject = grippable;
            bestInteractable = grippable;
            state = EGameHandState.GRIPPING;

            transform.SetParent(grippable.primaryGripTransform, false);
        }

        else
        {
            if (grippedObject == grippable)
            {
                grippedObject = null;
                state = EGameHandState.EMPTY;

                transform.SetParent(Player.instance.handParent, true);
            }
        }
    }
}