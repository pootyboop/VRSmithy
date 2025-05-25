using System;
using UnityEngine;


/*
public enum EHoldableTargetMovementType
{
    ADD_FORCE,
    SET_VELOCITY,
    SPRING,
    SPRING2
}
*/


[Serializable]
public class Handling
{
    /*
    public EHoldableTargetMovementType mvmtType = EHoldableTargetMovementType.ADD_FORCE;
    public float positionAcceleration = 10f;
    public float rotationAcceleration = 10f;
    public float maxPositionAccel = 50f;
    public float maxRotationAccel = 200f;

    public float positionStiffness = 800f;
    public float positionDamping = 80f;
    public float rotationStiffness = 500f;
    public float rotationDamping = 50f;
    */

    // === Parameters ===
    public float positionResponsiveness = 30f;     // Linear snap speed
    public float rotationResponsiveness = 100f;     // Angular snap speed
    public float positionResponsivenessCurvePower = 1f;
    public float rotationResponsivenessCurvePower = 1f;
    public float maxLinearSpeed = 1000f;             // Clamp linear velocity
    public float maxAngularSpeed = 2000f;            // Clamp angular velocity (radians/sec)
    public float weight = 1f;
}



public class Holdable : Grippable
{
    //refs
    Rigidbody rb;


    [Header("Handling")]
    /*
    //vals
    public float frequency = 10f; // Hz — how fast it tries to reach target (5–15 is good)
    public float maxPosAccel = 150f;
    public float maxRotAccel = 300f;
    */
    public Handling handling = new();
    [SerializeField] Transform targetPreview;

    //state
    Vector3 targetPosition;
    Quaternion targetRotation;
    [SerializeField] bool positionTracking = true;
    [SerializeField] bool rotationTracking = true;

    //un-gripped rb state
    private bool isKinematic, useGravity;
    private RigidbodyInterpolation interpolation;

    void Reset()
    {
        interactionPriority = 2;
    }


    public void Awake()
    {
        base.Awake();
        if (rb == null)
        {
            if (TryGetComponent<Rigidbody>(out var tryRb))
            {
                SetRB(tryRb);
            }
            else
            {
                Debug.LogWarning("No Rigidbody set for Holdable " + this + ". It needs one to move!");
            }
        }
    }

    public void SetRB(Rigidbody newRb)
    {
        rb = newRb;
        UpdateRBDefaults();
    }

    public void UpdateRBDefaults()
    {
        if (rb != null)
        {
            isKinematic = rb.isKinematic;
            useGravity = rb.useGravity;
            interpolation = rb.interpolation;
        }
    }

    public Rigidbody GetRB()
    {
        return rb;
    }


    void FixedUpdate()
    {
        if (grippedState == EGrippedState.UNGRIPPED)
        {
            return;
        }

        UpdateTarget();
        MoveToTargetSetVelocity();
        //MoveToTarget(Time.fixedDeltaTime);
    }



    public override void InteractStartOverrideable(GameHand hand)
    {
        base.InteractStartOverrideable(hand);
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            rb.linearVelocity = Vector3.zero;
        }
    }



    public override void InteractStopOverrideable(GameHand hand)
    {
        base.InteractStopOverrideable(hand);
        if (rb != null)
        {
            rb.isKinematic = isKinematic;
            rb.useGravity = useGravity;
            rb.interpolation = interpolation;
        }
    }

    //figures out where the target position is
    void UpdateTarget()
    {
        switch (grippedState)
        {
            case EGrippedState.TWOHANDED:   //both gripped
                SolveTwoHandedTargets(primaryGripGameHand.GetTrueHandTransform(), secondaryGripGameHand.GetTrueHandTransform());
                break;
            case EGrippedState.ONEHANDED:   //primary gripped
                SetTargetToTransform(primaryGripGameHand.GetTrueHandTransform());
                break;
            case EGrippedState.UNGRIPPED:   //none gripped - should never be called
                SetTargetToTransform(transform);
                break;
        }

        targetRotation *= primaryGripTransform.localRotation;
        targetRotation *= Quaternion.Euler(90, 0, 0);

        if (targetPreview != null) UpdateTargetPosition();
    }

    void SolveTwoHandedTargets(Transform primary, Transform secondary)
    {
        //average position between hands
        targetPosition = Vector3.Lerp(primary.position, secondary.position, .5f);

        //directly use primary position
        //targetPosition = primary.position;



        //======================================================================



        //average rotation between hands. almost never useful
        //targetRotation = Quaternion.Slerp(primary.rotation, secondary.rotation, .5f);

        //directly rotate from primary to secondary. roll is lost.
        //targetRotation = Quaternion.LookRotation(secondary.position - primary.position);

        //directly rotate from primary to secondary. roll is preserved from primary.
        Vector3 directionToB = secondary.position - primary.position;
        Quaternion lookRotation = Quaternion.LookRotation(directionToB, primary.up);
        Vector3 originalEuler = primary.rotation.eulerAngles;
        Vector3 lookEuler = lookRotation.eulerAngles;
        targetRotation = Quaternion.Euler(lookEuler.x, lookEuler.y, originalEuler.z);

    }

    void UpdateTargetPosition()
    {
        targetPreview.SetPositionAndRotation(targetPosition, targetRotation);
    }

    void SetTargetToTransform(Transform t)
    {
        targetPosition = t.position;
        targetRotation = t.rotation;
    }

    public void DetachFromParent()
    {
        transform.SetParent(null, true);
        SetInteractable(true);
        rb.isKinematic = false;
        rb.useGravity = true;
        UpdateRBDefaults();

        if (TryGetComponent(out DamageTakeable damageTakeable))
        {
            Destroy(damageTakeable);
        }
    }




    /*



    //moves toward the target position
    void MoveToTarget(float time)
    {
        switch (handling.mvmtType)
        {
            case EHoldableTargetMovementType.ADD_FORCE:
                MTT_AddForce(time);
                return;
            case EHoldableTargetMovementType.SET_VELOCITY:
                MTT_SetVelocity(time);
                return;
            case EHoldableTargetMovementType.SPRING:
                MTT_Spring(time);
                return;
            case EHoldableTargetMovementType.SPRING2:
                MTT_Spring2(time);
                return;
        }
    }



    //======================================================================================
    //these are all options for target following used to figure out which is the best method
    //======================================================================================

    // 1. Add force/torque toward the target position
    void MTT_AddForce(float time)
    {

        Vector3 weightedTargetPosition = new(
            targetPosition.x,
            targetPosition.y - handling.weight * time,
            targetPosition.z
        );

        Vector3 dirPos = weightedTargetPosition - transform.position;
        Vector3 velPos = handling.positionAcceleration * time * dirPos;

        Vector3 dirRot = targetRotation.eulerAngles - transform.rotation.eulerAngles;
        Vector3 velRot = handling.rotationAcceleration * time * dirRot;

        rb.AddForce(velPos);
        rb.AddTorque(velRot);
    }

    // 2. Set velocity toward the target position
    void MTT_SetVelocity(float time)
    {
        Vector3 weightedTargetPosition = new(
            targetPosition.x,
            targetPosition.y - handling.weight * time,
            targetPosition.z
        );

        Vector3 dirPos = weightedTargetPosition - transform.position;
        Vector3 velPos = handling.positionAcceleration * time * dirPos;

        Vector3 dirRot = (targetRotation * Quaternion.Inverse(transform.rotation)).eulerAngles;
        Vector3 velRot = handling.rotationAcceleration * time * dirRot;

        rb.linearVelocity = velPos;
        rb.angularVelocity = velRot;
    }

    void MTT_Spring(float time)
    {
        // POSITION
        Vector3 rootPosition = rb.worldCenterOfMass;
        Vector3 posError = targetPosition - rootPosition;
        Vector3 desiredVel = posError * handling.positionStiffness;
        Vector3 force = (desiredVel - rb.linearVelocity) * handling.positionDamping;
        force = Vector3.ClampMagnitude(force, handling.maxPositionAccel * rb.mass);
        rb.AddForce(force, ForceMode.Force);

        Debug.DrawLine(rootPosition, targetPosition, Color.red);

        //================================================ THIS WAS COMMENTED OUT ================================================
        // ROTATION
        Quaternion rotError = targetRotation * Quaternion.Inverse(rb.rotation);
        rotError.ToAngleAxis(out float angleInDeg, out Vector3 axis);
        if (angleInDeg > 180f) angleInDeg -= 360f;
        Vector3 angularError = axis * Mathf.Deg2Rad * angleInDeg;

        Vector3 desiredAngularVel = angularError * handling.rotationStiffness;
        Vector3 torque = (desiredAngularVel - rb.angularVelocity) * handling.rotationDamping;
        torque = Vector3.ClampMagnitude(torque, handling.maxRotationAccel * rb.mass);
        rb.AddTorque(torque, ForceMode.Force);
        //================================================ END OF COMMENTED OUT ================================================
    }

    void MTT_Spring2(float time)
    {
        // === STABLE SPRING TO POSITION ===
        Vector3 rootPosition = primaryGripTransform.position;//rb.worldCenterOfMass;
        Vector3 posError = targetPosition - rootPosition;
        float dampingRatio = 1f; // 1 = critically damped, >1 = overdamped

        float kp = (2 * Mathf.PI * frequency) * (2 * Mathf.PI * frequency); // spring strength
        float kd = 2 * dampingRatio * Mathf.Sqrt(kp); // damping coefficient

        Vector3 force = kp * posError - kd * rb.linearVelocity;
        force = Vector3.ClampMagnitude(force, handling.maxPositionAccel * rb.mass);
        rb.AddForce(force, ForceMode.Force);

        // === STABLE SPRING TO ROTATION ===
        Quaternion rootRotation = primaryGripTransform.rotation;//rb.rotation;
        Quaternion delta = targetRotation * Quaternion.Inverse(rootRotation);
        delta.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;
        if (angle != 0f) axis.Normalize();

        Vector3 angularError = axis * angle * Mathf.Deg2Rad;
        Vector3 angularVel = rb.angularVelocity;

        kp = Mathf.Pow(2 * Mathf.PI * frequency, 2);
        kd = 2 * dampingRatio * Mathf.Sqrt(kp);

        Vector3 torque = kp * angularError - kd * angularVel;
        torque = Vector3.ClampMagnitude(torque, handling.maxRotationAccel * rb.mass);
        rb.AddTorque(torque, ForceMode.Force);
    }



    void MTT()
    {
        if (primaryGripGameHand == null)
            return;

        // === Determine target position and rotation ===

        if (secondaryGripGameHand != null)
        {
            targetPosition = (primaryGripGameHand.GetTrueHandTransform().position + secondaryGripGameHand.GetTrueHandTransform().position) * 0.5f;

            Vector3 forward = (secondaryGripGameHand.GetTrueHandTransform().position - primaryGripGameHand.GetTrueHandTransform().position).normalized;
            Vector3 up = (primaryGripGameHand.GetTrueHandTransform().up + secondaryGripGameHand.GetTrueHandTransform().up).normalized;
            if (up == Vector3.zero) up = Vector3.up;
            targetRotation = Quaternion.LookRotation(forward, up);
        }
        else
        {
            targetPosition = primaryGripGameHand.GetTrueHandTransform().position;
            targetRotation = primaryGripGameHand.GetTrueHandTransform().rotation;
        }

        Debug.DrawRay(targetPosition, targetRotation.eulerAngles, Color.green);

        // === Parameters ===
        float positionDeadZone = 0.05f;   // meters
        float rotationDeadZone = 0.1f;    // radians (~0.5 degrees)
        float dampingRatio = 1f;
        float kp = Mathf.Pow(2 * Mathf.PI * frequency, 2);
        float kd = 2 * dampingRatio * Mathf.Sqrt(kp);

        // === Apply Position Force to Match Grip Point ===
        Vector3 localGripOffset = transform.InverseTransformPoint(primaryGripTransform.position);
        Vector3 gripWorld = transform.TransformPoint(localGripOffset);

        Vector3 posError = targetPosition - gripWorld;
        if (posError.sqrMagnitude > positionDeadZone * positionDeadZone)
        {
            Vector3 velAtPoint = rb.GetPointVelocity(gripWorld);
            Vector3 force = kp * posError - kd * velAtPoint;
            force = Vector3.ClampMagnitude(force, maxPosAccel * rb.mass);

            rb.AddForceAtPosition(force, gripWorld, ForceMode.Force);
        }

        // === Apply Rotation Torque to Match Orientation ===
        Quaternion delta = targetRotation * Quaternion.Inverse(rb.rotation);
        if (delta == Quaternion.identity) return; // avoid NaNs at exact match

        delta.ToAngleAxis(out float angleDeg, out Vector3 axis);
        float angleRad = Mathf.Deg2Rad * ((angleDeg > 180f) ? angleDeg - 360f : angleDeg);

        if (Mathf.Abs(angleRad) > rotationDeadZone)
        {
            if (axis == Vector3.zero) axis = Vector3.up; // fallback

            Vector3 angularError = axis.normalized * angleRad;
            Vector3 torque = kp * angularError - kd * rb.angularVelocity;
            torque = Vector3.ClampMagnitude(torque, maxRotAccel * rb.mass);

            rb.AddTorque(torque, ForceMode.Force);
        }
    }

    void MTT_SetVelocity2(float time)
    {

        Vector3 weightedTargetPosition = new(
            targetPosition.x,
            targetPosition.y - handling.weight * time,
            targetPosition.z
        );
        Vector3 dirPos = weightedTargetPosition - transform.position;
        Vector3 dirRot = (
            targetRotation * Quaternion.Inverse(transform.rotation)).eulerAngles;

        Vector3 velPos = handling.positionAcceleration * time * dirPos;
        Quaternion delta = targetRotation * Quaternion.Inverse(rb.rotation);
        delta.ToAngleAxis(out float angleDeg, out Vector3 axis);
        float angleRad = Mathf.Deg2Rad * ((angleDeg > 180f) ? angleDeg - 360f : angleDeg);
        if (axis == Vector3.zero) axis = Vector3.up; // fallback

        Vector3 angularError = axis.normalized * angleRad;
        Vector3 torque = kp * angularError - kd * rb.angularVelocity;

        rb.linearVelocity = velPos;
        rb.angularVelocity = velRot;
    }
    */



    void MoveToTargetSetVelocity()
    {
        if (primaryGripGameHand == null)
            return;

        if (positionTracking)
        {
            // === POSITION MATCHING ===
            Vector3 localGrabOffset = transform.InverseTransformPoint(primaryGripGameHand.transform.position);
            Vector3 currentGrabWorld = transform.TransformPoint(localGrabOffset);

            Vector3 positionDelta = targetPosition - currentGrabWorld;
            float distance = positionDelta.magnitude;
            float scaledResponsiveness = handling.positionResponsiveness * Mathf.Pow(distance, handling.positionResponsivenessCurvePower);
            Vector3 desiredVelocity = positionDelta.normalized * scaledResponsiveness;
            desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, handling.maxLinearSpeed);

            rb.linearVelocity = desiredVelocity;
        }

        if (rotationTracking)
        {
            // === ROTATION MATCHING ===
            Quaternion deltaRotation = targetRotation * Quaternion.Inverse(rb.rotation);
            deltaRotation.ToAngleAxis(out float angleDeg, out Vector3 axis);

            if (angleDeg > 180f) angleDeg -= 360f;
            if (Mathf.Abs(angleDeg) > 0.1f)
            {
                axis.Normalize();
                float angleRad = angleDeg * Mathf.Deg2Rad;
                float angleRadAbs = Mathf.Abs(angleRad);

                //float scaledRotResponsiveness = handling.rotationResponsiveness * Mathf.Pow(angleRadAbs, handling.rotationResponsivenessCurvePower);
                //Vector3 desiredAngularVelocity = axis * scaledRotResponsiveness;
                float scaledRotResponsiveness = handling.rotationResponsiveness * Mathf.Clamp01(Mathf.Abs(angleRad));
                Vector3 desiredAngularVelocity = axis * angleRad * scaledRotResponsiveness;

                desiredAngularVelocity = Vector3.ClampMagnitude(desiredAngularVelocity, handling.maxAngularSpeed);

                rb.angularVelocity = desiredAngularVelocity;
            }
            else
            {
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

}