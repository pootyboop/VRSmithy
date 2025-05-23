using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Spinner : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 1f;
    Rigidbody rb;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezePosition;
    }
    void FixedUpdate()
    {
        rb.MoveRotation(Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + rotationSpeed, transform.eulerAngles.z));
        //rb.angularVelocity = new Vector3(0f, rotationSpeed, 0f);
        //transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + rotationSpeed, transform.eulerAngles.z);

    }
}
