using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class EnemyAgent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _stateDebugText;
    [SerializeField] private Animator _animator;
    
    //[SerializeField] private Transform _defendPointB;
    
    [SerializeField] private LayerMask _obstacleMask;
    
    [SerializeField] private float _detectionRadius;
    [SerializeField] private float _moveSpeed;
    
    //Sensor Colliders
    [FormerlySerializedAs("_eyePosition")]
    [Header("Sensors")] 
    [SerializeField] private Transform _eyesTransform;
    [SerializeField] private SensorRange _chaseSensor;
    [SerializeField] private SensorRange _attackSensor;

    [Header("Actions")] 
    
    [Header("Siege")] 
    [SerializeField] private float _siegePriorityRadius;
    
    [Header("Chase")]
    [SerializeField] private float _chaseSpeed;
    [SerializeField] private float _chaseRadius;
    
    [Header("Attack")]
    [SerializeField] private float _attackRadius;
    [SerializeField] private float _attackDelay;
    [SerializeField] private float _attackDamage;

    [Header("Die")] 
    [SerializeField] private float _corpseTimer;

    public event EventHandler<EnemyAgent> AgentDeathEvent;

    public NPCStates State => _state;
    
    
    //Object Pool
    private ObjectPool _objectPool;
    
    //NavMesh
    private NavMeshAgent _agent;
    
    //Targets
    private Queue<Transform> _targetsQueue;
    private Transform _targetObjective;
    private int _targetIndex;
    
    private Transform _playerTransform;
    private Vector3 _targetDestination;
    private Vector3 _objectiveDestination;
    
    //#TODO Temp vars, need to be split up into different classes later 
    
    //Chase
    private float _chaseIntervalTimer;
    
    //Attack
    private float _attackIntervalTimer;
    private HealthComponent _targetHealthComponent;

    //State
    [SerializeField] private NPCStates _state;
    [SerializeField] private NPCStates _lastState;
    
    //Animation
    private string _animationName;
    private bool _finishedAttackAnim = true;
    
    //Health
    private HealthComponent _healthComponent;

    private bool _isInitialized;
    
    #region Unity Functions

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _healthComponent = GetComponent<HealthComponent>();
    }

    private void Start()
    {
        /*Move(_defendPoint, _moveSpeed);
        _animator.Play("walk");*/
        
        _chaseSensor.SetRadius(_detectionRadius);
        _chaseSensor.OnEnterRange += EnterChase;
        _chaseSensor.OnExitRange += ExitChase;
        
        _attackSensor.SetRadius(_attackRadius);
        _attackSensor.OnEnterRange += EnterAttack;
        _attackSensor.OnExitRange += ExitAttack;

        _healthComponent.DeathEvent += OnDeath;
    }

    //Part of state machine 
    private void Update()
    {
        if(_state == NPCStates.DEAD) return;
        
        if (_state == NPCStates.ATTACKING)
        {
            Attack(_targetHealthComponent, _attackDamage,_attackDelay);
            //return;
        }
        
        if (_state == NPCStates.CHASING)
        {
            Chase(_playerTransform, .2f);
        }

        if (_state == NPCStates.MOVING)
        {
            PlayAnimation(NPCStates.MOVING);
        }

        if (_state == NPCStates.GUARDING)
        {
            Guarding(_objectiveDestination);
        }

        if (_state == NPCStates.IDLE)
        {
            _agent.isStopped = true;
            PlayAnimation(NPCStates.IDLE);
        }
    }

    private void OnEnable()
    {
        if(!_isInitialized) return;
        
        _agent.isStopped = false;
        _state = NPCStates.MOVING;
        Move(_targetObjective.position, _moveSpeed);
    }

    #endregion

    #region Public Functions

    public void Initialize(Transform playerTransform, Transform target, List<Transform> targetsList, ObjectPool objectPool)
    {
        _isInitialized = true;
        
        _playerTransform = playerTransform;
        _targetObjective = target;
        _targetsQueue = new Queue<Transform>(targetsList);
        _objectPool = objectPool;
        
        ChangeState(NPCStates.MOVING);
        Move(_targetObjective.position, _moveSpeed);
        _animator.Play("walk");

        _agent.avoidancePriority = 50 + Random.Range(0, 4);
    }
    
    public bool IsInitialized()
    {
        return _isInitialized;
    }
    
    public void SetObjectivePosition(Vector3 position, NPCStates state)
    {
        _objectiveDestination = position;
        _agent.destination = position;
        _state = state;
    }

    public void Stop()
    {
        _agent.isStopped = true;
        ChangeState(NPCStates.IDLE);
    }
    
    public void GoNextPoint()
    {
        ChangeState(NPCStates.MOVING);
        
        _targetsQueue.Enqueue(_targetObjective);
        _targetObjective = _targetsQueue.Dequeue();
        _objectiveDestination = _targetObjective.position;
        Move(_objectiveDestination, _moveSpeed);
    }
    
    #endregion
    
    #region Animation Handling
    
    private void PlayAnimation(NPCStates state, string attackAnim = "attack1")
    {
        //Return if attack animation is not done
        if (!_finishedAttackAnim && _animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack") &&
            _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            _finishedAttackAnim = true;
        }

        if (!_finishedAttackAnim) return;
        
        switch (state)
        {
            case NPCStates.ATTACKING:
                _animator.Play(attackAnim, 0, 0);
                _finishedAttackAnim = false;
                break;
            case NPCStates.CHASING:
                _animator.Play("run");
                break;
            case NPCStates.MOVING:
                _animator.Play("walk");
                break;
            case NPCStates.GUARDING:
                _animator.Play("idle_battle");
                break;
            case NPCStates.IDLE:
                _animator.Play("idle");
                break;
        }
    }

    #endregion
    
    #region Callbacks

    //State CallBacks
    private void OnDeath(object sender, EventArgs e)
    {
        Die();
    }

    //Sensor Callbacks
    private void EnterChase(Transform target)
    {
        if (CheckLineOfSight(target))
        {
            if(_state == NPCStates.SIEGING && Vector3.Distance(_eyesTransform.position, target.position) > _siegePriorityRadius) return;
            
            //ChangeState(NPCStates.CHASING);
            ChangeTarget(target);
        }
    }
    
    private void ExitChase(Transform target)
    {
        if (!CheckLineOfSight(_playerTransform))
        {
            ChangeTarget(_targetObjective);
        }
    }
    
    private void EnterAttack(Transform target)
    {
        //Debug.Log("In Attack Range");
        
        if(_state == NPCStates.MOVING || _state == NPCStates.GUARDING) return;

        if (target.GetComponent<ObjectivePoint>() && _state != NPCStates.SIEGING)
        {
            if (_state != NPCStates.GUARDING)
            {
                GoNextPoint();
                return;
            }
        }
        
        if (target.TryGetComponent(out HealthComponent healthComponent))
        {
            Debug.Log("Attacking: " + healthComponent.name);
            _targetHealthComponent = healthComponent;
            _attackIntervalTimer = 0;
            ChangeState(NPCStates.ATTACKING);
        }
    }
    
    private void ExitAttack(Transform target)
    {
        _targetHealthComponent = null;
        //ChangeState(_lastState);

        if (_lastState == NPCStates.SIEGING || _lastState == NPCStates.GUARDING)
        {
            ChangeState(_lastState);
            Move(_objectiveDestination, _moveSpeed);
        }
        else
        {
            ChangeState(NPCStates.CHASING);
        }
    }

    #endregion
    
    #region StateMachine

    private void ChangeState(NPCStates state)
    {
        if(_state == NPCStates.DEAD) return;
        
        _lastState = _state;
        _state = state;
        //_stateDebugText.text = "STATE: " + state;
        
        //PlayAnimation(state);
    }

    private void ChangeTarget(Transform target)
    {
        if (target == _playerTransform)
        {
            _targetDestination = target.position;
            ChangeState(NPCStates.CHASING);
        }
        else
        {
            Move(target.position, _moveSpeed);
            ChangeState(NPCStates.MOVING);
        }
        //Debug.Log("Changing Target: " + target.name);
    }
    
    #endregion

    #region Utility

    private bool CheckLineOfSight(Transform target)
    {
        return !Physics.Linecast(target.position, _eyesTransform.position, _obstacleMask);
    }

    private float CheckDistance(Transform target)
    {
        return Vector3.Distance(target.position, _eyesTransform.position);
    }

    private IEnumerator ReturnToPool(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        _objectPool.ReturnGameObject(gameObject);
    }
    
    #endregion
    
    #region Action Execution
    
    //Change Agent Destination
    private void Move(Vector3 targetPosition, float speed)
    {
        _agent.speed = speed;
        _agent.SetDestination(targetPosition);
    }
    
    
    private void Chase(Transform target, float interval)
    {
        //Debug.Log("Chasing! " + target.name);
        
        _chaseIntervalTimer += Time.deltaTime;
        if (_chaseIntervalTimer > interval)
        {
            _chaseIntervalTimer = 0;
            Move(target.position, _chaseSpeed);
        }
        
        PlayAnimation(NPCStates.CHASING);
        
        //Chase Stop Condition
        if (!CheckLineOfSight(_playerTransform) && CheckDistance(_playerTransform) > _chaseRadius || CheckDistance(_playerTransform) > _chaseRadius)
        {
            if (_lastState == NPCStates.GUARDING)
            {
                Move(_objectiveDestination, _moveSpeed);
                ChangeState(NPCStates.GUARDING);
            }
            else
            {
                ChangeTarget(_targetObjective);
            }
        }
    }
    
    private void Attack(HealthComponent target, float damage, float interval)
    {
        if(target == null) return;
        
        Vector3 targetPosition = target.transform.position;
        
        transform.LookAt(new Vector3(targetPosition.x, transform.position.y, targetPosition.z));
        
        if (target.Health <= 0)
        {
            if (_targetsQueue.Contains(_targetObjective))
            {
                //_targetList.Remove(_targetObjective);
            }

            /*if (_targetList.Count > 0)
            {
                //_targetObjective = _targetList.First();
                //ChangeTarget(_targetObjective);
            }
            else
            {
                ChangeState(NPCStates.IDLE);
            }*/
            
            _targetHealthComponent = null;
            
            return;
        }
        
        _attackIntervalTimer += Time.deltaTime;
        if (_attackIntervalTimer > interval)
        {
            PlayAnimation(NPCStates.ATTACKING);
            _attackIntervalTimer = 0;
            target.TakeDamage(damage);
        }
    }

    private void Guarding(Vector3 objectiveDestination)
    {
        if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(objectiveDestination.x, objectiveDestination.z)) > _agent.stoppingDistance)
        {
            PlayAnimation(NPCStates.MOVING);
        }
        else
        {
            //Debug.Log(gameObject.name + " Is Guarding!");
            PlayAnimation(NPCStates.GUARDING);
        }
    }
    
    private void Die()
    {
        if (_state == NPCStates.DEAD) return;
        
        //Play Death Animation
        ChangeState(NPCStates.DEAD);
        _animator.Play("death1");
        _agent.isStopped = true;
        AgentDeathEvent?.Invoke(this, this);
        StartCoroutine(ReturnToPool(_corpseTimer));
    }

    #endregion
}
