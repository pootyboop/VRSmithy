using System.Collections.Generic;
using UnityEngine;



public enum EGameHandState
{
    EMPTY,  //holding nothing
    GRIPPING,   //holding onto something that cannot necessarily be moved freely - e.g. ladder handle
    HOLDING //holding onto something that can be moved freely according to hand movements - e.g. weapon
}

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class GameHand : HandBase
{
    //refs
    public TrueHand trueHand;
    private SphereCollider rbColl, pickupOverlap;
    private Rigidbody rb;

    //state
    [SerializeField] private EGameHandState state = EGameHandState.EMPTY;
    [SerializeField] private List<IInteractable> overlappedInteractables = new();
    [SerializeField] private IInteractable bestInteractable, currInteractable;



    void Awake()
    {
        SphereCollider[] colls = GetComponents<SphereCollider>();
        foreach (var coll in colls) {
            if (coll.isTrigger) {
                pickupOverlap = coll;
            } else {
                rbColl = coll;
            }
        }
        //pickupOverlap = GetComponent<SphereCollider>();
        //rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
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

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out IInteractable temp))
        {
            UpdateOverlappedInteractables(temp, true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out IInteractable temp))
        {
            UpdateOverlappedInteractables(temp, false);
        }
    }

    void UpdateOverlappedInteractables(IInteractable newInteractable, bool isAdding)
    {
        if (isAdding)
            overlappedInteractables.Add(newInteractable);
        else
            overlappedInteractables.Remove(newInteractable);

        UpdateBestInteractable();
    }

    public void UpdateBestInteractable()
    {
        if (currInteractable != null || state != EGameHandState.EMPTY)
        {
            SetBestInteractable(null);
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

        SetBestInteractable(best);
    }

    void SetBestInteractable(IInteractable interactable) {
        if (bestInteractable == interactable) {
            return;
        }

        bestInteractable?.SelectStop(this);
        bestInteractable = interactable;
        bestInteractable?.SelectStart(this);
    }

    public void SetInput(bool newInput)
    {
        if (newInput)
        {
            if (bestInteractable == null)
            {
                return;
            }

            bestInteractable.InteractStart(this);
        }

        else
        {
            currInteractable?.InteractStop(this);
        }
    }

    public void SetGripping(Grippable grippable, bool shouldGrip)
    {
        if (shouldGrip)
        {
            currInteractable = grippable;

            if (grippable.GetType() == typeof(Holdable))
            {
                state = EGameHandState.HOLDING;
            }
            else
            {
                state = EGameHandState.GRIPPING;
            }

            //transform.SetParent(grippable.primaryGripTransform, true);\
            rbColl.enabled = false;
            ParentSafely(grippable.primaryGripTransform);
            transform.position = grippable.primaryGripTransform.position;
            transform.rotation = grippable.primaryGripTransform.rotation;

        }

        else
        {
            if ((object)currInteractable == grippable)
            {
                currInteractable = null;

                state = EGameHandState.EMPTY;

                transform.SetParent(Player.instance.handParent, true);
                rbColl.enabled = true;
                RestoreOriginalLocalScale();
            }
        }

        
        UpdateBestInteractable();
    }
}