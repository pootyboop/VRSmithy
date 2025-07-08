using UnityEngine;
using UnityEngine.Events;

public class MovementHelper : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Leave blank to just track hits on the first collider on this GameObject.")]
    [SerializeField] private Collider coll;
    [Tooltip("Leave blank to just move this GameObject.")]
    [SerializeField] private Rigidbody rb;

    [Header("Movement")]
    [SerializeField] float movementAcceleration = 10f;
    [Tooltip("Set to 0 to disable max speed.")]
    [SerializeField] float maxMovementSpeed = 3f;
    [SerializeField] float jumpStrength = 200f;
    [SerializeField] bool canJumpMidair = false;
    private bool isGrounded = false;
    private bool groundedJumped = false;
    private float groundedCheckDistance = 0.1f;

    [Header("Events")]
    [SerializeField] UnityEvent onGroundedEnter;
    [SerializeField] UnityEvent onGroundedExit;

    void Awake()
    {
        if (coll == null)
        {
            coll = GetComponent<Collider>();
        }
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        UpdateIsGrounded(true);
    }

    void FixedUpdate()
    {
        UpdateIsGrounded();
        //print(isGrounded + ", " + groundedJumped);
    }


    void UpdateIsGrounded()
    {
        UpdateIsGrounded(false);
    }
    void UpdateIsGrounded(bool forceUpdate)
    {
        float floorMargin = 0.01f;

        Vector3 raycastFrom = new Vector3(
            coll.transform.position.x,
            coll.transform.position.y + groundedCheckDistance + floorMargin,
            coll.transform.position.z
        );

        bool rawGrounded = Physics.Raycast(raycastFrom, Vector3.down, groundedCheckDistance + floorMargin);

        if (rawGrounded != isGrounded || forceUpdate)
        {
            SetGrounded(rawGrounded);
        }
    }

    void SetGrounded(bool newGrounded)
    {
        isGrounded = newGrounded;

        if (isGrounded)
        {
            groundedJumped = false;
            onGroundedEnter.Invoke();
        }

        else
        {
            onGroundedExit.Invoke();
        }
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public void Jump()
    {
        if ((!isGrounded || groundedJumped) && !canJumpMidair)
        {
            return;
        }

        rb.AddForce(new Vector3(0f, jumpStrength, 0f));

        if (isGrounded)
        {
            groundedJumped = true;
        }
    }

    public void Move(Vector3 movement, float time)
    {
        if (movement == Vector3.zero || rb.linearVelocity.magnitude > maxMovementSpeed)
        {
            return;
        }
        
        Vector3 movementVector = movement.normalized * movementAcceleration;

        float plannedSpeed = ((movementVector * time) + rb.linearVelocity).magnitude / rb.mass;
        if (plannedSpeed > maxMovementSpeed && maxMovementSpeed > 0)
        {
            return;
            /*
            float currSpeed = rb.linearVelocity.magnitude / rb.mass;
            if (currSpeed > maxMovementSpeed)
            {
                return;
            }
            */
            //movementVector = movementVector.normalized * (Mathf.Clamp(plannedSpeed, -maxMovementSpeed, maxMovementSpeed) * rb.mass - rb.linearVelocity) / time;
        }

        rb.AddForce(movementVector * time);
    }
}