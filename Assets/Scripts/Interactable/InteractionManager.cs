using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager instance;
    [SerializeField] GameObject selectionPreviewPrefab;
    public int grippedLayer = 8;

    void Start()
    {
        instance = this;
    }

    public GameObject CreateSelectionPreview(Transform attachPoint) {
        return Instantiate(selectionPreviewPrefab, attachPoint);
    }
}
