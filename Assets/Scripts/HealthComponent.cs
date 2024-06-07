using System;
using UnityEngine;
using UnityEngine.Events;

public class HealthComponent : MonoBehaviour
{
    [Header("Event Channel")]
    [SerializeField] private PlayerEventChannel _playerEventChannel;
    
    [Header("Health")]
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _health;
    
    public event EventHandler DeathEvent;
    public event UnityAction<float, float> HealthChangeEvent;
    
    public float Health => _health;
    
    public void TakeDamage(float damage, HitBoxType type = HitBoxType.DEFAULT)
    {
        float actualDamage = damage;

        if (type == HitBoxType.HEAD)
        {
            actualDamage *= 6;
        }
        
        _health -= actualDamage;
        
        HealthChangeEvent?.Invoke(_health, _maxHealth);

        if(_playerEventChannel != null) _playerEventChannel.OnHealthChanged(_health);
        
        if (_health <= 0)
        {
            DeathEvent?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Awake()
    {
        _health = _maxHealth;
    }

    private void OnDisable()
    {
        _health = _maxHealth;
    }
}
