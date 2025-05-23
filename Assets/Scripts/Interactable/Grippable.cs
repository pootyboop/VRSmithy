using UnityEngine;

public enum EGrippedState
{
    UNGRIPPED,
    ONEHANDED,
    TWOHANDED
}

public class Grippable : MonoBehaviour, IInteractable
{
    //refs
    GameObject previewMesh;
    Collider coll;

    //interaction info
    [Header("Interaction")]
    [SerializeField] private bool isInteractable = true;
    public int interactionPriority = 1; //0 by default
    private int defaultLayer;

    [Header("Gripping")]
    public Transform primaryGripTransform;
    public Transform secondaryGripTransform;
    [HideInInspector] public GameHand primaryGripGameHand, secondaryGripGameHand;
    [SerializeField] protected EGrippedState grippedState = EGrippedState.UNGRIPPED;

    public void Awake()
    {
        defaultLayer = gameObject.layer;

        if (coll == null)
        {
            if (TryGetComponent<Collider>(out var tryColl))
            {
                coll = tryColl;
            }
            else
            {
                Debug.LogWarning("No collider set for Grippable " + this + ". It can't be directly gripped!");
            }
        }

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
    public Collider GetCollider()
    {
        return coll;
    }



    public void InteractStart(GameHand hand)
    {
        InteractStartOverrideable(hand);
    }
    public void InteractStop(GameHand hand)
    {
        InteractStopOverrideable(hand);
    }
    public virtual void InteractStartOverrideable(GameHand hand)
    {
        if (!isInteractable)
        {
            return;
        }

        if (grippedState == EGrippedState.UNGRIPPED)
        {
            SetSelfAllChildrenLayer(InteractionManager.instance.grippedLayer);
        }

        UpdateGrips(hand, true);

        //if (IsGripGripped(true)) {  SetInteractable(false); }
    }
    public virtual void InteractStopOverrideable(GameHand hand)
    {
        UpdateGrips(hand, false);

        if (grippedState == EGrippedState.UNGRIPPED)
        {
            SetSelfAllChildrenLayer(defaultLayer);
        }

        SetInteractable(true);
    }
    public void SelectStart(GameHand hand)
    {
        SelectStartOverrideable(hand);
    }
    public void SelectStop(GameHand hand)
    {
        SelectStopOverrideable(hand);
    }
    public virtual void SelectStartOverrideable(GameHand hand)
    {
        if (previewMesh != null)
            Destroy(previewMesh);
        previewMesh = InteractionManager.instance.CreateSelectionPreview(primaryGripTransform);
    }
    public virtual void SelectStopOverrideable(GameHand hand)
    {
        if (previewMesh != null)
            Destroy(previewMesh);
    }



    void SetSelfAllChildrenLayer(int layer)
    {
        SetSelfAndDescendantsLayer(gameObject, layer);
    }
    void SetSelfAndDescendantsLayer(GameObject unc, int layer)
    {
        unc.layer = layer;

        foreach (Transform jit in unc.transform)
        {
            SetSelfAndDescendantsLayer(jit.gameObject, layer);
        }
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

        UpdateIsCurrentlyGripped();
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

    public EGrippedState UpdateIsCurrentlyGripped()
    {
        if (IsGripGripped(false))
        {
            if (IsGripGripped(true))
            {
                grippedState = EGrippedState.TWOHANDED;
            }

            else
            {
                grippedState = EGrippedState.ONEHANDED;
            }
        }

        else
        {
            grippedState = EGrippedState.UNGRIPPED;
        }

        return grippedState;
    }
}