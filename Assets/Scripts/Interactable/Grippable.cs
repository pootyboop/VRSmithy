using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Grippable : MonoBehaviour, IInteractable
{
    //refs
    GameObject previewMesh;

    //interaction info
    [SerializeField] private bool isInteractable = true;
    public int interactionPriority = 1; //0 by default

    //grip transforms
    public Transform primaryGripTransform, secondaryGripTransform;
    [HideInInspector] public GameHand primaryGripGameHand, secondaryGripGameHand;

    void Awake()
    {
        if (primaryGripTransform == null)
        {
            primaryGripTransform = transform;
        }
    }

    public bool GetInteractable()
    {
        return isInteractable;
    }
    public void SetInteractable(bool newInteractable)
    {
        if (isInteractable != newInteractable)
        {
            isInteractable = newInteractable;
            Player.instance.UpdateHandBestInteractables();
        }
    }
    public int GetInteractionPriority()
    {
        return interactionPriority;
    }



    public void InteractStart(GameHand hand)
    {
        this.InteractStartOverrideable(hand);
    }
    public void InteractStop(GameHand hand)
    {
        this.InteractStopOverrideable(hand);
    }
    public virtual void InteractStartOverrideable(GameHand hand) {
        if (!isInteractable)
        {
            return;
        }
        SetInteractable(false);
        UpdateGrips(hand, true);
    }
    public virtual void InteractStopOverrideable(GameHand hand)
    {
        UpdateGrips(hand, false);
        SetInteractable(true);
    }
    public void SelectStart(GameHand hand)
    {
        if (previewMesh != null)
            Destroy(previewMesh);
        previewMesh = InteractionManager.instance.CreateSelectionPreview(primaryGripTransform);
    }
    public void SelectStop(GameHand hand)
    {
        if (previewMesh != null)
            Destroy(previewMesh);
    }



    void UpdateGrips(GameHand hand, bool newGripping)
    {
        //hand is releasing - just release the necessary hands
        if (!newGripping)
        {
            //primary grip released - also release secondary grip
            if (hand == primaryGripGameHand)
            {
                SetHandInteracting(primaryGripGameHand, false, false);
                SetHandInteracting(secondaryGripGameHand, true, false);
            }

            //secondary grip released
            else if (hand == secondaryGripGameHand)
            {
                SetHandInteracting(secondaryGripGameHand, true, false);
            }

            return;
        }


        if (secondaryGripGameHand == null)
        {
            if (primaryGripGameHand == null)
            {
                SetHandInteracting(hand, false, true);
                return;
            }

            SetHandInteracting(hand, true, true);
        }
    }

    void SetHandInteracting(GameHand hand, bool isSecondary, bool newInteracting)
    {
        if (hand == null)
        {
            return;
        }

        hand.SetGripping(this, newInteracting);

        if (newInteracting)
        {
            if (isSecondary)
            {
                secondaryGripGameHand = hand;
            }
            else
            {
                primaryGripGameHand = hand;
            }
        }

        else
        {
            if (isSecondary)
            {
                secondaryGripGameHand = null;
            }
            else
            {
                primaryGripGameHand = null;
            }
        }
    }

    public bool IsGripGripped(bool isSecondary)
    {
        if (!isSecondary && primaryGripGameHand != null)
        {
            return true;
        }

        if (isSecondary && secondaryGripGameHand != null)
        {
            return true;
        }

        return false;
    }

    public bool IsGrippedAtAll()
    {
        return primaryGripGameHand != null || secondaryGripGameHand != null;
    }
}