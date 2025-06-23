using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;



public enum EPlayerMovementMode
{
    DEFAULT,
    CROUCH,
    DODGE
}



[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    //refs
    public static Player instance;
    Rigidbody rb;
    CapsuleCollider coll;
    MovementHelper mvmt;
    XRIDefaultInputActions controls;
    IEnumerator dodgeTimer, rotateCooldown;

    [Header("References")]
    public Transform handCamParent;
    public Camera cam;
    public TrueHand lTrueHand, rTrueHand;
    public GameHand lGameHand, rGameHand;
    [SerializeField] Transform lDesktopHandPos, rDesktopHandPos;

    //state
    [Header("Desktop Mode")]
    [SerializeField] private bool isVRMode = false;
    [SerializeField] private bool handsTrackConsistently = false;
    private bool areDesktopHandsTracking = false;
    [SerializeField] private float mouseSensitivity = 0.03f;
    private float desktopCameraHeight;

    EPlayerMovementMode movementMode = EPlayerMovementMode.DEFAULT;
    private Vector2 movementInput = Vector2.zero, rotationInput = Vector2.zero;
    private Vector3 movementInputCleaned = Vector3.zero, dodgeInput = Vector3.zero;
    private Vector3 preDodgeVelocity;

    //vals
    [Header("Movement")]
    [SerializeField] float acceleration = 10f;
    [SerializeField] float maxSpeed = 10f;
    [SerializeField] float jumpStrength = 10f;
    [SerializeField] float dodgeStrength = 2f;
    [SerializeField] float dodgeDuration = 0.25f;
    [SerializeField] float dodgeCooldownTime = 2f;
    [SerializeField] float dodgeCooldownMovementSpeedPenalty = 0.5f;
    [SerializeField] float crouchMovementSpeedPenalty = 0.5f;
    [SerializeField] float rotateDegrees = 45f;
    [SerializeField] float rotateCooldownTime = 0.4f;



    void Awake()
    {
        instance = this;
        rb = GetComponent<Rigidbody>();
        coll = GetComponent<CapsuleCollider>();
        mvmt = GetComponent<MovementHelper>();
        desktopCameraHeight = handCamParent.position.y;
        UpdateDeviceMode();
        SetMovementMode(movementMode);
    }

    void UpdateDeviceMode()
    {
        print("Playing in " + (isVRMode ? "VR" : "PC") + " mode.");

        if (isVRMode)
        {
        }

        else
        {
            if (handsTrackConsistently)
            {
                areDesktopHandsTracking = true;
            }
            else
            {
                DesktopHandsToCamera();
            }
        }

        SetMouseVisibility(isVRMode);
        cam.GetComponent<TrackedPoseDriver>().enabled = isVRMode;

        UpdateControls();
    }

    void DesktopHandsButtonHit()
    {
        if (handsTrackConsistently)
        {
            areDesktopHandsTracking = !areDesktopHandsTracking;
        }

        else
        {
            DesktopHandsToCamera();
        }
    }

    void DesktopHandsToCamera()
    {
        lTrueHand.transform.position = lDesktopHandPos.transform.position;
        rTrueHand.transform.position = rDesktopHandPos.transform.position;
        lTrueHand.transform.rotation = lDesktopHandPos.transform.rotation;
        rTrueHand.transform.rotation = rDesktopHandPos.transform.rotation;
    }

    void UpdateControls()
    {
        if (controls == null)
        {
            controls = new XRIDefaultInputActions();
            controls.Enable();

            //joysticks
            controls.XRILeftLocomotion.Move.performed += ReceiveMovementInput;
            controls.XRILeftLocomotion.Move.canceled += ReceiveMovementInput;
            controls.XRIRightLocomotion.Move.performed += ReceiveRotationInput;
            controls.XRIRightLocomotion.Move.canceled += ReceiveRotationInput;

            //grips
            controls.XRILeftInteraction.Select.started += ctx => SetHandGripping(false, true);
            controls.XRILeftInteraction.Select.canceled += ctx => SetHandGripping(false, false);
            controls.XRIRightInteraction.Select.started += ctx => SetHandGripping(true, true);
            controls.XRIRightInteraction.Select.canceled += ctx => SetHandGripping(true, false);

            //generic
            controls.Generic.Jump.performed += Jump;
            controls.Generic.Dodge.performed += Dodge;
        }

        if (isVRMode)
        {
            controls.Generic.MouseRotation.performed -= ReceiveMouseInput;
            controls.Editor.HandsToCam.performed -= ctx => DesktopHandsButtonHit();
            controls.Editor.Crouch.started -= ctx => SetCrouching(true);
            controls.Editor.Crouch.canceled -= ctx => SetCrouching(false);
        }

        else
        {
            controls.Generic.MouseRotation.performed += ReceiveMouseInput;
            controls.Editor.HandsToCam.performed += ctx => DesktopHandsButtonHit();
            controls.Editor.Crouch.started += ctx => SetCrouching(true);
            controls.Editor.Crouch.canceled += ctx => SetCrouching(false);
        }
    }



    private void OnEnable()
    {
        controls?.Enable();
    }

    private void OnDisable()
    {
        controls?.Disable();
    }

    void FixedUpdate()
    {
        float groundOffset = .05f;
        float height = cam.transform.position.y - transform.position.y;
        coll.center = new Vector3(0f, height / 2 + groundOffset, 0f);
        coll.height = height - groundOffset;

        Move(Time.fixedDeltaTime);

        if (!isVRMode && handsTrackConsistently && areDesktopHandsTracking)
        {
            DesktopHandsToCamera();
        }
    }

    void SetCrouching(bool newCrouching)
    {
        SetMovementMode(newCrouching ? EPlayerMovementMode.CROUCH : EPlayerMovementMode.DEFAULT);
    }

    void UpdateCrouchDesktop()
    {
        if (isVRMode)
        {
            return;
        }

        handCamParent.position = new Vector3(
            handCamParent.position.x,
            movementMode == EPlayerMovementMode.CROUCH ? 0.1f : desktopCameraHeight,
            handCamParent.position.z
        );
    }

    void Jump(InputAction.CallbackContext ctx)
    {
        //rb.AddForce(new Vector3(0f, jumpStrength, 0f));
        mvmt.Jump();
    }

    void Dodge(InputAction.CallbackContext ctx)
    {
        if (dodgeTimer != null)
        {
            return;
        }

        if (movementInput == Vector2.zero)
        {
            dodgeInput = PrepareMovementInput(new Vector2(0f, -1f));
        }
        else
        {
            dodgeInput = movementInputCleaned;
        }

        dodgeInput *= dodgeStrength;
        preDodgeVelocity = rb.linearVelocity;

        dodgeTimer = DodgeTimer();
        StartCoroutine(dodgeTimer);
    }

    IEnumerator DodgeTimer()
    {
        //dodge start
        SetMovementMode(EPlayerMovementMode.DODGE);

        yield return new WaitForSeconds(dodgeDuration);

        //dodge end, cooldown start
        SetMovementMode(EPlayerMovementMode.DEFAULT);
        yield return new WaitForSeconds(dodgeCooldownTime);

        //cooldown end
        dodgeTimer = null;

        yield break;
    }

    void Move(float time)
    {
        switch (movementMode)
        {
            default:
            case EPlayerMovementMode.DEFAULT:
            case EPlayerMovementMode.CROUCH:
                if (movementInput != Vector2.zero && rb.linearVelocity.magnitude < maxSpeed)
                {
                    DesiredMovement(time);
                }
                break;
            case EPlayerMovementMode.DODGE:
                //rb.AddForce(dodgeInput);
                rb.linearVelocity = new Vector3(dodgeInput.x, rb.linearVelocity.y, dodgeInput.z);
                break;
        }
    }

    private void DesiredMovement(float time)
    {
        movementInputCleaned = PrepareMovementInput(movementInput);
        Vector3 movementVector = movementInputCleaned * acceleration;

        if (dodgeTimer != null)
        {
            movementVector *= dodgeCooldownMovementSpeedPenalty;
        }
        if (movementMode == EPlayerMovementMode.CROUCH)
        {
            movementVector *= crouchMovementSpeedPenalty;
        }

        mvmt.Move(movementVector, time);
    }

    Vector3 PrepareMovementInput(Vector2 input)
    {
        //get the camera's forward and right vectors, normalized WITHOUT the Y value so the player moves on the X and Z axes
        Vector3 forward = NormalizeVectorFlat(cam.transform.forward);
        Vector3 right = NormalizeVectorFlat(cam.transform.right);

        //multiply by player movement input (meaning the character moves relative to its rotation)
        forward *= input.y;
        right *= input.x;

        //combine into one final vector and normalize the final direction vector so you don't move faster diagonally
        Vector3 preppedInput = (forward + right).normalized;
        return preppedInput;
    }

    private Vector3 NormalizeVectorFlat(Vector3 vectorIn)
    {
        vectorIn.y = 0f;  //remove the Y value so the player can't move upward (without jumping)
        return vectorIn.normalized; //re-normalize without the Y value
    }

    private void ReceiveMovementInput(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.action.ReadValue<Vector2>();
    }

    public void SetMovementMode(EPlayerMovementMode newMode)
    {
        switch (movementMode)
        {
            case EPlayerMovementMode.DODGE:
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
                dodgeInput = Vector2.zero;
                rb.linearVelocity = preDodgeVelocity;
                preDodgeVelocity = Vector3.zero;
                break;
            default:
                break;
        }

        movementMode = newMode;

        switch (movementMode)
        {
            case EPlayerMovementMode.DODGE:
                break;
            default:
                break;
        }

        UpdateCrouchDesktop();
    }

    private void ReceiveRotationInput(InputAction.CallbackContext ctx)
    {
        rotationInput = ctx.action.ReadValue<Vector2>();

        if (rotationInput.x != 0f && rotateCooldown == null)
        {
            Rotate(rotationInput.x);
        }
    }

    void Rotate(float input)
    {
        float rotationAmount = rotateDegrees;
        if (input < 0)
        {
            rotationAmount *= -1f;
        }

        transform.Rotate(0f, rotationAmount, 0f);

        rotateCooldown = RotateCooldown();
        StartCoroutine(rotateCooldown);
    }

    IEnumerator RotateCooldown()
    {
        yield return new WaitForSeconds(rotateCooldownTime);
        rotateCooldown = null;
        yield break;
    }

    private void ReceiveMouseInput(InputAction.CallbackContext ctx)
    {
        Vector2 mousePos = ctx.action.ReadValue<Vector2>() * mouseSensitivity;
        Vector2 rot;

        //rotate mouse input in a weird but necessary way for expected behavior
        rot.y = mousePos.x;
        //this clamp prevents the camera from rotating past straight up or straight down, which would disorient the player
        rot.x = ClampCam(mousePos.y);

        //set the new camera rotation
        cam.transform.rotation = Quaternion.Euler(cam.transform.eulerAngles.x - rot.x, cam.transform.eulerAngles.y + rot.y, 0f);
    }

    private float ClampCam(float rotX)
    {
        return Mathf.Clamp(rotX, -89.9f, 89.9f);
    }

    void SetMouseVisibility(bool newVisibility)
    {
        Cursor.visible = newVisibility;
        if (newVisibility)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void SetHandGripping(bool isRight, bool newGripping)
    {
        GetGameHandByBool(isRight).SetInput(newGripping);
    }

    public void UpdateHandBestInteractables()
    {
        lGameHand.UpdateBestInteractable();
        rGameHand.UpdateBestInteractable();
    }

    public GameHand[] GetGameHands()
    {
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

    public void SetIgnoreCollider(Collider collider, bool newIgnore)
    {
        if (collider == null)
        {
            return;
        }

        Physics.IgnoreCollision(coll, collider, newIgnore);
    }
}
