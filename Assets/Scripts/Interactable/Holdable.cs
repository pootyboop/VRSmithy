using System;
using UnityEngine;

[Serializable]
public class Handling {
    public float positionAcceleration = 1f;
    public float rotationAcceleration = 1f;
    public float weight = 9.81f;
}

[RequireComponent(typeof(Rigidbody))]
public class Holdable : Grippable
{
    //refs
    Rigidbody rb;

    //vals
    public Handling handling = new();

    //state
    Vector3 targetPosition;
    Quaternion targetRotation;
    bool isCurrentlyHeld = false;

    //un-gripped state
    private bool isKinematic, useGravity;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        isKinematic = rb.isKinematic;
        useGravity = rb.useGravity;

        interactionPriority = 2;
    }



    void FixedUpdate()
    {
        if (!isCurrentlyHeld) {
            return;
        }

        UpdateTarget();
        MoveToTarget(Time.fixedDeltaTime);
    }



    public override void InteractStartOverrideable(GameHand hand)
    {
        base.InteractStartOverrideable(hand);
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        UpdateIsCurrentlyHeld();
    }



    public override void InteractStopOverrideable(GameHand hand)
    {
        base.InteractStopOverrideable(hand);
        rb.isKinematic = isKinematic;
        rb.useGravity = useGravity;
        UpdateIsCurrentlyHeld();
    }

    void UpdateIsCurrentlyHeld() {
        isCurrentlyHeld = IsGrippedAtAll();
    }

    //figures out where the target position is
    void UpdateTarget()
    {
        if (IsGripGripped(false))
        {

            if (IsGripGripped(true))
            {
                //both gripped
                targetPosition = (primaryGripGameHand.trueHand.transform.position + secondaryGripGameHand.trueHand.transform.position) / 2;
                targetRotation = Quaternion.Slerp(primaryGripGameHand.trueHand.transform.rotation, secondaryGripGameHand.trueHand.transform.rotation, .5f);
            }

            //primary gripped
            targetPosition = primaryGripGameHand.trueHand.transform.position;
            targetRotation = primaryGripGameHand.trueHand.transform.rotation;
            return;
        }

        //none gripped
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

    //moves toward the target position
    void MoveToTarget(float timeMultiplier) {
        Vector3 gravTargetPosition = targetPosition;
        gravTargetPosition.y -= handling.weight * timeMultiplier;

        Vector3 dirPos = gravTargetPosition - transform.position;
        Vector3 velPos = dirPos * handling.positionAcceleration * timeMultiplier;

        Vector3 dirRot = targetRotation.eulerAngles - transform.rotation.eulerAngles;
        Vector3 velRot = dirRot * handling.rotationAcceleration * timeMultiplier;

        rb.AddForce(velPos);
        rb.AddTorque(velRot);

        print(this + " adds: " + velPos + ", " + velRot);
    }
}
