using System;
using UnityEngine;

[Serializable]
public class PhysicalDamageData {
    public EPhysicalDamageType type;
    public Sprite icon;
}

[Serializable]
public class DamageEffectData {
    public EDamageEffectType type;
    public Sprite icon;
    public Color color = new Color(1f, 0f, 0f, 1f);
}

public class DamageManager : MonoBehaviour
{
    public static DamageManager instance;
    public PhysicalDamageData[] physicalDamageData;
    public DamageEffectData[] damageEffectData;
    
    [SerializeField] DamageNumber damageNumberPrefab;
    void Start()
    {
        instance = this;
    }

    public void SpawnDamageNumber(Damage damage, float efficacy) {
        DamageNumber damageNumber = Instantiate(damageNumberPrefab);
        damageNumber.transform.position = damage.position;
        damageNumber.SetData(damage.damageAmount, efficacy);
    }
}
