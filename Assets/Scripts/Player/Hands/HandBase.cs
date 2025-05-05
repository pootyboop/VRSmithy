using UnityEngine;

public class HandBase : MonoBehaviour
{
    private Vector3 originalLocalScale = Vector3.one;
    public bool isRight = false;

    void Awake()
    {
        //originalLocalScale = transform.localScale;
        ParentSafely(transform.parent);
    }

    public void ParentSafely(Transform parent) {
        transform.parent = null;
        transform.localScale = Vector3.one;
        transform.parent = parent.transform;
    }

    public void RestoreOriginalLocalScale() {
        transform.localScale = originalLocalScale;
    }
}
