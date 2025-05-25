using UnityEngine;

public class Fauna : MonoBehaviour
{
    DamageTakeable damageTakeable;
    DamageDealable damageDealable;

    void Awake()
    {
        damageTakeable = GetComponent<DamageTakeable>();
        damageDealable = GetComponent<DamageDealable>();
        SetAllChildDamageRelateds(transform);
    }


    void SetAllChildDamageRelateds(Transform t)
    {
        foreach (Transform child in transform)
        {
            SetAllChildDamageRelateds(child);
        }

        if (TryGetComponent(out DamageTakeable dmged))
        {
            if (dmged.owner == null)
            {
                dmged.owner = damageTakeable;
            }
        }

        if (TryGetComponent(out DamageDealable dmger))
        {
            if (dmger.owner == null)
            {
                dmger.owner = damageDealable;
            }
        }
    }
}
