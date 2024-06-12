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
    [SerializeField] private float _healingRate;
    
    private bool _canRecharge;
    private float _healingAlpha;

    public event EventHandler DeathEvent;
    public event UnityAction<float, float> HealthChangeEvent;
    
    public float Health => _health;

    private void Start()
    {
        if(_playerEventChannel == null) return;

        _playerEventChannel.PlayerWallRunStartEvent += StartWallRecharge;
        _playerEventChannel.PlayerWallRunStopEvent += StopWallRecharge;
    }

    private void Update()
    {
        if (_canRecharge)
        {
            //Lerp current health to max;
            _healingAlpha = Mathf.MoveTowards(_healingAlpha, 1, _healingRate * Time.deltaTime);
            _health = Mathf.Lerp(0, _maxHealth,_healingAlpha);
            //_playerEventChannel.OnHealthChanged(_health);
        }
    }

    private void StartWallRecharge()
    {
        _healingAlpha = _health / _maxHealth;
        _canRecharge = true;
        _playerEventChannel.OnHealthRechargeStart(_health, _healingRate);
    }
    
    private void StopWallRecharge()
    {
        _canRecharge = false;
        _playerEventChannel.OnHealthRechargeStop();
    }

    public void TakeDamage(float damage, HitBoxType type = HitBoxType.DEFAULT)
    {
        float actualDamage = damage;

        if (type == HitBoxType.HEAD)
        {
            actualDamage *= 6;
        }
        
        _health -= actualDamage;
        

        if (_playerEventChannel != null)
        {
            _playerEventChannel.OnHealthChanged(_health);
        }
        else
        {
            HealthChangeEvent?.Invoke(_health, _maxHealth);
        }
        
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
