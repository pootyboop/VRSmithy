using UnityEngine;

public class ActiveRagdoll : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private float positionSpring = 1500f;
    [SerializeField] private float positionDamper = 10f;
    [SerializeField] private float maximumForce = 3.402823e+38f;
    [SerializeField] private bool useAcceleration = false;
    [SerializeField] private float limit = 360f;
    [SerializeField] private float bounciness = 0f;
    [SerializeField] private float contactDistance = 0f;

    [Header("References")]
    [SerializeField] private Transform physicalRoot, animatedRoot;
    [SerializeField] private Transform[] physicalBones, animatedBones;
    private ConfigurableJoint[] joints;
    private Quaternion[] initialJointRotations;



    void Awake()
    {
        InitializeBody();
    }



    void FixedUpdate()
    {
        UpdatePhysicalAnim();
    }



    void InitializeBody()
    {
        joints = new ConfigurableJoint[physicalBones.Length];
        initialJointRotations = new Quaternion[physicalBones.Length];
        for (int i = 0; i < physicalBones.Length; i++)
        {
            if (physicalBones[i].TryGetComponent<ConfigurableJoint>(out var joint))
            {
                joints[i] = joint;
                initialJointRotations[i] = physicalBones[i].localRotation;

                if (TryGetClosestHierarchicalRB(physicalBones[i], out var boneRB))
                {
                    joint.connectedBody = boneRB;
                    boneRB.interpolation = RigidbodyInterpolation.Interpolate;
                }

                JointDrive jointDrive = new()
                {
                    positionSpring = positionSpring,
                    positionDamper = positionDamper,
                    maximumForce = maximumForce,
                    useAcceleration = useAcceleration
                };

                SoftJointLimit softJointLimit = new()
                {
                    limit = limit,
                    bounciness = bounciness,
                    contactDistance = contactDistance
                };

                joint.angularXDrive = jointDrive;
                joint.angularYZDrive = jointDrive;

                joint.lowAngularXLimit = softJointLimit;
                joint.highAngularXLimit = softJointLimit;
                joint.angularYLimit = softJointLimit;
                joint.angularZLimit = softJointLimit;

                joint.enableCollision = false;



                if (physicalBones[i].TryGetComponent<Rigidbody>(out var jointRB))
                {
                    jointRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    //jointRB.useGravity = false;
                }
            }
        }
    }



    bool TryGetClosestHierarchicalRB(Transform bone, out Rigidbody closestRB)
    {
        int maxChecks = 2;
        Transform currT = bone;
        for (int i = 0; i < maxChecks; i++)
        {
            currT = currT.parent;
            if (currT.TryGetComponent<Rigidbody>(out var newRB))
            {
                closestRB = newRB;
                return true;
            }
        }

        closestRB = null;
        return false;
    }





    void UpdatePhysicalAnim()
    {
        //forces physical root to animated root position. looks glitchy
        //physicalRoot.SetPositionAndRotation(animatedRoot.position, animatedRoot.rotation);

        for (int i = 0; i < physicalBones.Length; i++)
        {
            ConfigurableJointExtensions.SetTargetRotationLocal(joints[i], animatedBones[i].localRotation, initialJointRotations[i]);
        }
    }
}