using System.Collections;
using UnityEngine;

public enum EIKFootState
{
    STANDING,
    STEPPING,
    HANGING
}

[System.Serializable]
public class StepInfo
{
    public StepInfo(Vector3 _position, Vector3 _normal)
    {
        position = _position;
        normal = _normal;
    }
    public Vector3 position;
    public Vector3 normal;
}

public class FootIKSolver : MonoBehaviour
{
    [SerializeField] ActiveRagdoll owningCreature;
    MovementHelper mvmt;
    [SerializeField] Transform physicalFoot;
    Vector3 defaultLocalPos;
    StepInfo currStep, nextStep;
    [SerializeField] EIKFootState state = EIKFootState.HANGING;
    [SerializeField] float groundCheckDistance = 1f;
    [SerializeField] float groundCheckStartFromRelativeToBody = .5f;
    [SerializeField] float maxDistanceBeforeStepping = .25f;
    [SerializeField] float stepDistance = .5f;
    [SerializeField] float stepTime = .5f;
    [SerializeField] float stepTimeVelocityScale = .04f;
    [SerializeField] float stepHeight = .5f;
    [SerializeField] float stepHeightVelocityScale = .04f;
    [SerializeField] bool footBonePointsDownward = true;
    [SerializeField] bool forcePhysicalRotation = true;
    float footHeight;
    [SerializeField] LayerMask groundLayer;
    IEnumerator stepCoroutine;

    void Start()
    {
        mvmt = owningCreature.GetComponent<MovementHelper>();
        defaultLocalPos = GetPositionRelativeToBody();
        footHeight = transform.position.y - owningCreature.transform.position.y;
        nextStep = SetStep(new StepInfo(ApplyFootHeight(transform.position), owningCreature.transform.up));
    }

    
    void FixedUpdate()
    {
        if (forcePhysicalRotation && state == EIKFootState.STANDING)
        {
            physicalFoot.LookAt(transform.forward, CorrectFootNormalDirection(currStep.normal));
        }
    }
    

    void Update()
    {
        switch (state)
        {
            case EIKFootState.STANDING:
            case EIKFootState.HANGING:
                UpdateNonSteppingFoot();
                break;
            case EIKFootState.STEPPING:
                break;
        }
        Debug.DrawLine(transform.position, nextStep.position, Color.green);
    }

    void UpdateNonSteppingFoot()
    {

        SetStep(currStep);
        bool isGroundAvailable = FindGround(transform.position, out StepInfo findGround);
        if (isGroundAvailable)
        {
            //should-did we take a step?
            bool startedStep = TryStep();

            //no step?
            if (!startedStep)
            {
                //just standing.
                state = EIKFootState.STANDING;
                SetStep(findGround);
            }
        }

        //no standable ground. just hanging.
        else
        {
            /*
            if (state == EIKFootState.STANDING)
            {
                SetStep(findGround);
            }
            */
            state = EIKFootState.HANGING;
        }
    }

    bool FindGround(Vector3 pos, out StepInfo ground)
    {
        Vector3 creatureUp = owningCreature.transform.up;
        Vector3 rayFromHeight = pos + (creatureUp * groundCheckStartFromRelativeToBody);
        Ray ray = new(rayFromHeight, -creatureUp);

        //is there standable ground?
        if (Physics.Raycast(ray, out RaycastHit hit, groundCheckDistance, groundLayer))
        {
            ground = new StepInfo(ApplyFootHeight(hit.point), hit.normal);
            return true;
        }

        ground = new StepInfo(rayFromHeight + (-creatureUp * groundCheckDistance), creatureUp);
        return false;
    }

    bool TryStep()
    {
        Vector3 defaultWorldPos = CreatureLocalToWorld(defaultLocalPos);
        float distFromDefaultLocalPos = Vector3.Distance(transform.position, defaultWorldPos);

        if (distFromDefaultLocalPos > maxDistanceBeforeStepping && owningCreature.CanStep(this))
        {
            //get direction from foot to default step position
            //Vector3 stepVector = defaultWorldPos - transform.position;
            Vector3 stepVector = owningCreature.transform.forward;

            //remove creature's up vector (move flat along whatever surface it's on, usually flat ground removing the y value)
            stepVector = Vector3.ProjectOnPlane(stepVector, owningCreature.transform.up);

            //overshoot the default step position via stepDistance
            stepVector = stepVector.normalized * stepDistance;

            //get final position to check step position at
            Vector3 nextStepRoughPos = defaultWorldPos + stepVector;
            FindGround(nextStepRoughPos, out StepInfo nextStepInfo);

            TakeStep(nextStepInfo);
            return true;
        }
        return false;
    }

    void TakeStep(StepInfo step)
    {
        nextStep = step;

        if (stepCoroutine != null)
        {
            StopCoroutine(stepCoroutine);
            stepCoroutine = null;
        }

        stepCoroutine = StepProgress();
        StartCoroutine(stepCoroutine);
    }

    Vector3 ApplyFootHeight(Vector3 pos)
    {
        return pos + owningCreature.transform.up * footHeight;
    }

    Vector3 CorrectFootNormalDirection(Vector3 norm)
    {
        if (footBonePointsDownward)
        {
            return norm * -1;
        }

        return norm;
    }

    IEnumerator StepProgress()
    {
        
        state = EIKFootState.STEPPING;
        float time = 0f;
        float maxTime = stepTime - (mvmt.GetVelocity() * stepTimeVelocityScale);

        while (time < maxTime)
        {
            time += Time.deltaTime;
            //print("STEPPING! progress: " + time / maxTime);
            float alpha = Mathf.SmoothStep(0f, 1f, time / maxTime);
            transform.position = Vector3.Lerp(currStep.position, ApplyStepHeight(nextStep.position, alpha), alpha);
            transform.rotation = Quaternion.LookRotation(owningCreature.transform.forward, CorrectFootNormalDirection(owningCreature.transform.up));

            yield return null;
        }
        
        SetStep(nextStep);
        state = EIKFootState.STANDING;
    }

    Vector3 ApplyStepHeight(Vector3 stepPosition, float alpha)
    {
        float height = alpha;
        if (height > 0.5) {
            height = 1f - height;
        }
        return new Vector3(
            stepPosition.x,
            stepPosition.y + height * (stepHeight + stepHeightVelocityScale * mvmt.GetVelocity()),
            stepPosition.z
        );
    }


    StepInfo SetStep(StepInfo step)
    {
        currStep = step;
        //transform.rotation = Quaternion.FromToRotation(transform.up, step.normal) * transform.rotation;
        transform.LookAt(transform.forward, CorrectFootNormalDirection(step.normal));
        transform.position = currStep.position;
        return currStep;
    }


    Vector3 GetPositionRelativeToBody()
    {
        return owningCreature.transform.InverseTransformPoint(transform.position);
    }

    Vector3 CreatureLocalToWorld(Vector3 v)
    {
        return owningCreature.transform.TransformPoint(v);
    }






    public bool IsGrounded()
    {
        return state == EIKFootState.STANDING;
    }

}
