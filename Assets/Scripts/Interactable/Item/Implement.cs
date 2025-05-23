using System.Collections.Generic;
using UnityEngine;

public class Implement : Item
{
    List<Component> components = new();



    void Reset()
    {
        interactionPriority = 3;
        name = "Implement";
        itemCategory = EItemCategory.Implement;
        stowable = EStowableState.HOLSTER;
    }

    public void Awake()
    {
        base.Awake();
    }

    void OnCollisionEnter(Collision collision)
    {
        foreach (var contact in collision.contacts)
        {
            if (contact.thisCollider.gameObject.TryGetComponent(out Component hitComponent))
            {
                hitComponent.OnCollisionEnter(collision);
            }
        }
    }

    void SetSelfAndDescendantsLayer(GameObject unc, int layer)
    {
        unc.layer = layer;

        foreach (Transform jit in unc.transform)
        {
            SetSelfAndDescendantsLayer(jit.gameObject, layer);
        }
    }

    public void OnComponentAttachedChanged(Component component, bool newAttached) {
        if (newAttached) {
            components.Add(component);
        }

        else {
            components.Remove(component);
        }
    }
}
