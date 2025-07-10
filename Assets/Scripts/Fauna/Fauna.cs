using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

public class Fauna : MonoBehaviour
{
    [Header("References")]
    DamageTakeable damageTakeable;
    DamageDealable damageDealable;
    Rigidbody rb;
    ActiveRagdoll activeRagdoll;
    NavMeshAgent navMeshAgent;
    MovementHelper mvmt;
    IEnumerator headIKWeightCoroutine;

    [Header("Movement")]
    [SerializeField] bool usePresetTarget = true;
    [SerializeField] bool trackTarget = true;
    [SerializeField] Transform target;
    [SerializeField] bool testAlwaysFollowTarget = false;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] bool onlyRotateOnY = true;

    [Header("IK")]
    [SerializeField] float headTargetingTime = 1f;
    [SerializeField] MultiAimConstraint headIK;
    Transform headIKTarget;
    float headIKTrackingSpeed = .2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        activeRagdoll = GetComponent<ActiveRagdoll>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        mvmt = GetComponent<MovementHelper>();
        damageTakeable = GetComponent<DamageTakeable>();
        damageDealable = GetComponent<DamageDealable>();

        headIKTarget = headIK.data.sourceObjects[0].transform;

        InitChildren(transform);
    }

    void Start()
    {
        if (usePresetTarget)
        {
            SetTarget(target);
        }
        else
        {
            SetTarget(null);
        }
    }


    void FixedUpdate()
    {
        if (trackTarget)
        {
            UpdateTargeting();
        }
    }


    void SetTarget(Transform newTarget)
    {
        target = newTarget;

        if (target != null)
        {
            SetHeadIKWeight(1f);
        }

        else
        {
            headIKTarget.SetParent(headIK.transform, false);
            headIKTarget.localPosition = Vector3.zero;
            SetHeadIKWeight(0f);
        }
    }



    void SetHeadIKWeight(float newWeight)
    {
        if (headIKWeightCoroutine != null)
        {
            StopCoroutine(headIKWeightCoroutine);
            headIKWeightCoroutine = null;
        }

        StartCoroutine(SetHeadIKWeightCoroutine(newWeight));
    }



    IEnumerator SetHeadIKWeightCoroutine(float newWeight)
    {
        float time = 0;
        float start = headIK.weight;

        while (time < headTargetingTime)
        {
            headIK.weight = Mathf.Lerp(start, newWeight, time / headTargetingTime);

            time += Time.deltaTime;
            yield return null;
        }

        headIK.weight = newWeight;
    }


    void UpdateTargeting()
    {
        if (target == null)
        {
            return;
        }
        
        headIKTarget.position = Vector3.Lerp(headIKTarget.position, target.position, headIKTrackingSpeed);
        
        //navMeshAgent.SetDestination(target.position);
        TurnTowardFaceTarget(Time.fixedDeltaTime);
        if (testAlwaysFollowTarget)
        {
            mvmt.Move(transform.forward, Time.fixedDeltaTime);
        }
    }


    void TurnTowardFaceTarget(float time)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        if (onlyRotateOnY)
        {
            lookRotation = Quaternion.Euler(transform.eulerAngles.x, lookRotation.eulerAngles.y, transform.eulerAngles.z);
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, time * rotationSpeed);

        /*
        Vector3 groundDirection = Vector3.down;
        Vector3 towardsOther = (target.position - transform.position).normalized;
        Vector3 rotateAxis = Vector3.Cross(towardsOther, groundDirection);
        Vector3 torque = turnSpeed * time * rotateAxis;
        //rb.AddTorque(torque);
        //activeRagdoll.AddTorque(torque);
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + torque.y, transform.eulerAngles.z);
        */
    }


    void InitChildren(Transform t)
    {
        foreach (Transform child in t)
        {
            InitChildren(child);
        }

        if (t.TryGetComponent(out DamageTakeable dmged))
        {
            if (dmged.owner == null)
            {
                dmged.owner = damageTakeable;
            }
        }

        if (t.TryGetComponent(out DamageDealable dmger))
        {
            if (dmger.owner == null)
            {
                dmger.owner = damageDealable;
            }
        }

        if (t.TryGetComponent(out Collider coll))
        {
            if (coll.material == null && t.gameObject.layer == FaunaManager.instance.faunaLayer)
            {
                coll.material = FaunaManager.instance.defaultFaunaMaterial;
            }
        }
    }
}
