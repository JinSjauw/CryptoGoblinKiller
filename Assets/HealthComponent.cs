using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] private float _health;

    public event EventHandler DeathEvent;
    public float Health => _health;

    public void TakeDamage(float damage, HitBoxType type = HitBoxType.DEFAULT)
    {
        float actualDamage = damage;

        if (type == HitBoxType.HEAD)
        {
            actualDamage = damage * 15;
        }
        
        _health -= actualDamage;

        if (_health <= 0)
        {
            DeathEvent?.Invoke(this, EventArgs.Empty);
            //Debug.Log("Death for: " + transform.name);
        }
    }
    
}
