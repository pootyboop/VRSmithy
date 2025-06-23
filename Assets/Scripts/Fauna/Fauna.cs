using UnityEngine;
using UnityEngine.AI;

public class Fauna : MonoBehaviour
{
    DamageTakeable damageTakeable;
    DamageDealable damageDealable;
    Rigidbody rb;
    ActiveRagdoll activeRagdoll;
    NavMeshAgent navMeshAgent;
    [SerializeField] Transform target;
    [SerializeField] float movementSpeed = 5f;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] bool onlyRotateOnY = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        activeRagdoll = GetComponent<ActiveRagdoll>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        damageTakeable = GetComponent<DamageTakeable>();
        damageDealable = GetComponent<DamageDealable>();
        InitChildren(transform);
    }


    void FixedUpdate()
    {
        UpdateTarget();
    }


    void UpdateTarget()
    {
        //navMeshAgent.SetDestination(target.position);
        rb.AddForce(transform.forward * movementSpeed);
        TurnTowardFaceTarget(Time.fixedDeltaTime);
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
