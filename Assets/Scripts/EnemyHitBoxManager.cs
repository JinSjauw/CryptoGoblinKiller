using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyHitBoxManager : MonoBehaviour
{
    [SerializeField] private List<EnemyHitBox> _hitBoxexList;

    private HealthComponent _healthComponent;
    
    private void Awake()
    {
        _hitBoxexList = GetComponentsInChildren<EnemyHitBox>().ToList();
        _healthComponent = GetComponent<HealthComponent>();
    }

    private void Start()
    {
        foreach (EnemyHitBox hitBox in _hitBoxexList)
        {
            hitBox.onHitEvent += OnHit;
        }
    }

    private void OnHit(object sender, EnemyHit e)
    {
        float actualDamage = e.Damage;

        if (e.HitBoxType == HitBoxType.HEAD)
        {
            actualDamage *= 6;

            if (_healthComponent.Health - actualDamage <= 0)
            {
                e.HitBox.AnimateHead();
            }
        }
        
        _healthComponent.TakeDamage(actualDamage);
    }

    private void OnDestroy()
    {
        foreach (EnemyHitBox hitBox in _hitBoxexList)
        {
            hitBox.onHitEvent -= OnHit;
        }
    }
}
