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
    [SerializeField] float maxMovementSpeed = 3f;
    [SerializeField] float jumpStrength = 200f;
    [SerializeField] bool canJumpMidair = false;

    [Header("Grounded")]
    private bool isGrounded = false;
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
            rb = gameObject.GetComponent<Rigidbody>();
        }

        SetGrounded(isGrounded);
    }

    void FixedUpdate()
    {
        UpdateIsGrounded();
    }

    void UpdateIsGrounded()
    {
        bool rawGrounded = Physics.Raycast(coll.transform.position, -Vector3.up, 0.1f);

        if (rawGrounded != isGrounded)
        {
            SetGrounded(rawGrounded);
        }
    }

    void SetGrounded(bool newGrounded)
    {
        isGrounded = newGrounded;

        if (isGrounded)
        {
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
        if (!isGrounded && !canJumpMidair)
        {
            return;
        }

        rb.AddForce(new Vector3(0f, jumpStrength, 0f));
    }

    public void Move(Vector3 movement, float time)
    {
        if (movement == Vector3.zero || rb.linearVelocity.magnitude > maxMovementSpeed)
        {
            return;
        }
        
        Vector3 movementVector = movement.normalized * movementAcceleration;

        float plannedSpeed = (movementVector + rb.linearVelocity).magnitude / rb.mass;
        if (plannedSpeed > maxMovementSpeed)
        {
            movementVector = movementVector.normalized * (maxMovementSpeed - plannedSpeed);
        }

        print(movementVector * time);
        rb.AddForce(movementVector * time);
    }
}