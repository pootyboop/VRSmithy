using UnityEngine;

public enum ESnapFrequency
{
    NONE,
    UPDATE,
    FIXEDUPDATE,
    EVERYTHING
}

public class StickToTransform : MonoBehaviour
{
    [SerializeField] Transform stickTo;
    [SerializeField] ESnapFrequency snapFrequency = ESnapFrequency.FIXEDUPDATE;

    void Update()
    {
        if (snapFrequency == ESnapFrequency.UPDATE || snapFrequency == ESnapFrequency.EVERYTHING) Snap();
    }

    void FixedUpdate()
    {
        if (snapFrequency == ESnapFrequency.FIXEDUPDATE || snapFrequency == ESnapFrequency.EVERYTHING) Snap();
    }

    public void Snap()
    {
        transform.SetPositionAndRotation(stickTo.position, stickTo.rotation);
    }
}