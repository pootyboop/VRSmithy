using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager instance;
    [SerializeField] GameObject selectionPreviewPrefab;

    void Start()
    {
        instance = this;
    }

    public GameObject CreateSelectionPreview(Transform attachPoint) {
        return Instantiate(selectionPreviewPrefab, attachPoint);
    }
}
