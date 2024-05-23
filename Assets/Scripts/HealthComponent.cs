using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _health;
    //[SerializeField] private GameObject _vfxObject;
    
    public event EventHandler DeathEvent;
    public float Health => _health;

    public void TakeDamage(float damage, HitBoxType type = HitBoxType.DEFAULT)
    {
        float actualDamage = damage;

        if (type == HitBoxType.HEAD)
        {
            actualDamage *= 6;
        }
        
        _health -= actualDamage;

        if (_health <= 0)
        {
            DeathEvent?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnDisable()
    {
        _health = _maxHealth;
    }
}