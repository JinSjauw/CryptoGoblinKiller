using UnityEngine;
using UnityEngine.AI;


public class EnemyAgent : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    
    [SerializeField] private Transform _defendPointB;
    [SerializeField] private Transform _defendPoint;
    [SerializeField] private Transform _playerTransform;

    [SerializeField] private LayerMask _obstacleMask;
    
    [SerializeField] private float _detectionRadius;
    [SerializeField] private float _moveSpeed;
    
    //Sensor Colliders
    [Header("Sensors")]
    [SerializeField] private SensorRange _chaseSensor;
    [SerializeField] private SensorRange _attackSensor;
    
    [Header("Chase")]
    [SerializeField] private float _chaseSpeed;
    [SerializeField] private float _chaseRadius;
    
    [Header("Attack")]
    [SerializeField] private float _attackRadius;
    [SerializeField] private float _attackDelay;
    [SerializeField] private float _attackDamage;
    
    
    //NavMesh
    private NavMeshAgent _agent;
    
    //Chase
    private bool _chase;
    private float _chaseIntervalTimer;
    
    //Attack
    private bool _attack;
    private float _attackIntervalTimer;
    private HealthComponent _targetHealthComponent;

    #region Unity Functions

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        //_collider = GetComponent<SphereCollider>();

        //_collider.radius = _detectionRadius;
    }

    private void Start()
    {
        Move(_defendPoint, _moveSpeed);
        _animator.Play("walk");
        
        _chaseSensor.SetRadius(_detectionRadius);
        _chaseSensor.OnEnterRange += EnterChase;
        _chaseSensor.OnExitRange += ExitChase;
        
        _attackSensor.SetRadius(_attackRadius);
        _attackSensor.OnEnterRange += EnterAttack;
        _attackSensor.OnExitRange += ExitAttack;
    }
    
    private void Update()
    {
        if (_attack)
        {
            Attack(_targetHealthComponent, _attackDamage,_attackDelay);
            return;
        }
        
        if(!_chase) return;
        Chase(_playerTransform, .2f);
    }
    
    #endregion

    #region Sensor Callbacks

    private void EnterChase(Transform target)
    {
        if (CheckLineOfSight(target))
        {
            _attack = false;
            ChangeTarget(target);
        }
    }
    
    private void ExitChase()
    {
        if (!CheckLineOfSight(_playerTransform))
        {
            ChangeTarget(_defendPoint);
        }
    }
    
    private void EnterAttack(Transform target)
    {
        Debug.Log("In Attack Range");
        
        if (target.TryGetComponent(out HealthComponent healthComponent))
        {
            Debug.Log("Attacking: " + healthComponent.name);
            _targetHealthComponent = healthComponent;
            _attack = true;
            _attackIntervalTimer = 0;
        }
    }
    
    private void ExitAttack()
    {
        _targetHealthComponent = null;
        _attack = false;

        if (_chase)
        {
            _animator.Play("run");
        }
    }
    

    #endregion
    
    #region Private Functions

    private bool CheckLineOfSight(Transform target)
    {
        return !Physics.Linecast(target.position, transform.position, _obstacleMask);
    }

    private float CheckDistance(Transform target)
    {
        return Vector3.Distance(target.position, transform.position);
    }

    private void ChangeTarget(Transform target)
    {
        if (target == _playerTransform)
        {
            _animator.Play("run");
            _chase = true;
        }
        else
        {
            _chase = false;
            //_agent.SetDestination(target.position);
            _animator.Play("walk");
            Move(target, _moveSpeed);
        }
        
        Debug.Log("Changing Target: " + target.name);
    }
    
    private void Move(Transform target, float speed)
    {
        _agent.speed = speed;
        _agent.SetDestination(target.position);
    }
    
    private void Chase(Transform target, float interval)
    {
        Debug.Log("Chasing! " + target.name);
        
        _chaseIntervalTimer += Time.deltaTime;
        if (_chaseIntervalTimer > interval)
        {
            _chaseIntervalTimer = 0;
            //_agent.SetDestination(target.position);
            Move(target, _chaseSpeed);
        }
        
        //Chase Stop Condition
        if (!CheckLineOfSight(_playerTransform) &&  CheckDistance(_playerTransform) > _chaseRadius)
        {
            _chase = false;
            ChangeTarget(_defendPoint);
        }
    }
    
    private void Attack(HealthComponent target, float damage, float interval)
    {
        if (target.Health <= 0)
        {
            _attack = false;
            _targetHealthComponent = null;
            _defendPoint = _defendPointB;
            ChangeTarget(_defendPoint);
            return;
        }
        
        _attackIntervalTimer += Time.deltaTime;
        if (_attackIntervalTimer > interval)
        {
            _animator.Play("power_attack");
            _attackIntervalTimer = 0;
            target.TakeDamage(damage);
        }
    }

    #endregion
}
