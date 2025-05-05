using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;
    public Transform handParent;
    public TrueHand lTrueHand, rTrueHand;
    public GameHand lGameHand, rGameHand;
    public Camera cam;
    XRIDefaultInputActions controls;



    void Awake()
    {
        instance = this;
        controls = new XRIDefaultInputActions();
        controls.XRILeftInteraction.Select.started += ctx => SetHandGripping(false, true);
        controls.XRILeftInteraction.Select.canceled += ctx => SetHandGripping(false, false);
        controls.XRIRightInteraction.Select.started += ctx => SetHandGripping(true, true);
        controls.XRIRightInteraction.Select.canceled += ctx => SetHandGripping(true, false);
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void SetHandGripping(bool isRight, bool newGripping)
    {
        GetGameHandByBool(isRight).SetInput(newGripping);
    }

    public void UpdateHandBestInteractables() {
        lGameHand.UpdateBestInteractable();
        rGameHand.UpdateBestInteractable();
    }

    public GameHand[] GetGameHands() {
        GameHand[] gameHands = new GameHand[2];
        gameHands[0] = lGameHand;
        gameHands[1] = rGameHand;
        return gameHands;
    }

    GameHand GetGameHandByBool(bool isRight)
    {
        if (isRight)
        {
            return rGameHand;
        }

        return lGameHand;
    }
}
