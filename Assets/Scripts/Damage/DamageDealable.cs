using UnityEngine;



[RequireComponent(typeof(Collider))]
public class DamageDealable : MonoBehaviour
{
    Collider coll;
    public DamageDealable owner;
    public DamageStats damageStats = new();

    void Awake()
    {
        coll = GetComponent<Collider>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out IDamageable hitDamageable))
        {
            DealDamage(collision, hitDamageable);
        }
    }

    void DealDamage(Collision collision, IDamageable hitDamageable)
    {
        Damage damage = SolveDealtDamage(collision, hitDamageable);
        if (damage != null)
        {
            hitDamageable.Damage(damage);
        }
    }

    Damage SolveDealtDamage(Collision collision, IDamageable hitDamageable)
    {
        Damage intendedDamage = new();

        intendedDamage.stats = damageStats;
        if (owner == null)
        {
            owner = this;
        }

        intendedDamage.dealer = owner;
        intendedDamage.literalDealer = gameObject;

        intendedDamage.damageAmount = SolveDealtDamageAmount(collision.impulse.magnitude);

        intendedDamage.direction = collision.GetContact(0).normal;
        intendedDamage.position = collision.GetContact(0).point;



        Damage finalDamage = hitDamageable.GetDamageResistance().ApplyResistanceToDamage(intendedDamage);

        if (finalDamage.damageAmount <= 0f)
        {
            finalDamage.damageAmount = 0f;
        }

        return finalDamage;
    }

    float SolveDealtDamageAmount(float force)
    {
        float raw = damageStats.physical.rawDamage;
        float sharpness = Mathf.Abs(damageStats.physical.sharpness) * 0.2f;

        return (raw + sharpness) * force;

    }
}