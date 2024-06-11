using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityHFSM;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    [Header("Object Refs")]
    [SerializeField] private Transform _playerTarget;
    [SerializeField] private List<Transform> _objectiveList;
    
    [Header("Sensors")]
    [SerializeField] private PlayerSensor _playerDetectSensor;
    [SerializeField] private PlayerSensor _playerMeleeSensor;
    [SerializeField] private PlayerSensor _playerShootSensor;
    [SerializeField] private ObjectiveSensor _objectiveSiegeSensor;
    
    [Header("NPC Variables")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _chaseSpeed;
    [SerializeField] private float _attackSpeed;
    [SerializeField] private float _attackRadius;
    [SerializeField] private float _meleeDamage;

    public float MoveSpeed => _moveSpeed;
    public float ChaseSpeed => _chaseSpeed;
    public float AttackSpeed => _attackSpeed;
    public float MeleeDamage => _meleeDamage;
    public Transform ObjectiveTarget => _objectiveTarget;

    public UnityAction<Enemy> OnEnemyDeath;
    
    private StateMachine<EnemyState, StateEvent> _enemyFSM;
    
    private NavMeshAgent _agent;
    private Animator _animator;
    private HealthComponent _healthComponent;
    private ObjectPool _objectPool;
    
    private Queue<Transform> _objectiveQueue;
    private Transform _objectiveTarget;
    private ObjectivePoint _currentTargetPoint;
    
    private float _lastAttackTime;

    private bool _isInitialized;
    
    public void Initialize(Transform playerTarget, Transform targetObjective, ObjectPool objectPool, List<Transform> objectiveList)
    {
        if (_isInitialized) return;

        _isInitialized = true;

        _objectPool = objectPool;
        _objectiveList = objectiveList;
        
        _objectiveQueue = new Queue<Transform>(_objectiveList);
        //_objectiveTarget = _objectiveQueue.Dequeue();
        
        _objectiveTarget = targetObjective;
        _playerTarget = playerTarget;
        
        _animator = GetComponentInChildren<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _healthComponent = GetComponent<HealthComponent>();
        _enemyFSM = new();
        
        _enemyFSM.AddState(EnemyState.IDLE, new IdleState(false, this));
        _enemyFSM.AddState(EnemyState.CHASE, new ChaseState(false, this, _playerTarget));
        _enemyFSM.AddState(EnemyState.MELEE, new MeleeState(false, this, OnAttack));
        _enemyFSM.AddState(EnemyState.MOVING, new GoToObjectiveState(false, this, SelectObjectivePoint));
        _enemyFSM.AddState(EnemyState.SIEGING, new SiegingState(false, this));
        _enemyFSM.AddState(EnemyState.DEAD, new DeadState(false, this));
        
        //Move Transitions
        _enemyFSM.AddTriggerTransition(StateEvent.GOTOOBJECTIVE, new Transition<EnemyState>(EnemyState.MOVING, EnemyState.MOVING));
        _enemyFSM.AddTriggerTransition(StateEvent.GOTOOBJECTIVE, new Transition<EnemyState>(EnemyState.SIEGING, EnemyState.MOVING));
        
        //Chase Transitions
        _enemyFSM.AddTriggerTransition(StateEvent.CHASEPLAYER, new Transition<EnemyState>(EnemyState.MOVING, EnemyState.CHASE));
        _enemyFSM.AddTriggerTransition(StateEvent.LOSTPLAYER, new Transition<EnemyState>(EnemyState.CHASE, EnemyState.MOVING));
        
        //Melee Transitions
        _enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.MELEE, EnemyState.CHASE, ShouldNotMelee));
        _enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.CHASE, EnemyState.MELEE, ShouldMelee, null, null, true));
        _enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.MOVING, EnemyState.MELEE, ShouldMelee, null, null, true));
        _enemyFSM.AddTriggerTransition(StateEvent.CHASEPLAYER, new Transition<EnemyState>(EnemyState.MELEE, EnemyState.CHASE));
        _enemyFSM.AddTriggerTransition(StateEvent.STOPMELEEPLAYER, new Transition<EnemyState>(EnemyState.MELEE, EnemyState.MOVING));
        
        //Siege Transitions
        _enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.MOVING, EnemyState.SIEGING, ShouldSiege, null, null, true));
        _enemyFSM.AddTriggerTransition(StateEvent.CHASEPLAYER, new Transition<EnemyState>(EnemyState.SIEGING, EnemyState.CHASE));
        _enemyFSM.AddTriggerTransitionFromAny(StateEvent.DIE, EnemyState.DEAD);
        
        //Spawn Transition
        _enemyFSM.AddTriggerTransitionFromAny(StateEvent.SPAWN, EnemyState.MOVING);
        
        //Set Start State
        _enemyFSM.SetStartState(EnemyState.MOVING);
        
        //Init FSM
        _enemyFSM.Init();
        
        Subscribe();
    }

    private void Subscribe()
    {
        _healthComponent.DeathEvent += Die;
        
        _playerDetectSensor.OnPlayerEnter += ChasePlayer;
        _playerDetectSensor.OnPlayerExit += LostPlayer;

        _playerMeleeSensor.OnPlayerEnter += MeleePlayer;
        _playerMeleeSensor.OnPlayerExit += StopMeleePlayer;

        _objectiveSiegeSensor.OnObjectiveEnter += OnObjectiveEnter;
        _objectiveSiegeSensor.OnObjectiveExit += OnObjectiveExit;
    }

    private void Die(object sender, EventArgs e)
    {
        _enemyFSM.Trigger(StateEvent.DIE);
        OnEnemyDeath?.Invoke(this);
    }

    private void OnObjectiveEnter(Transform point)
    {
        //Get Objective Position.
        if (_currentTargetPoint == null)
        {
            _currentTargetPoint = _objectiveTarget.GetComponent<ObjectivePoint>();
        }

        if (_currentTargetPoint.IsFull() || _currentTargetPoint.IsDestroyed())
        {
            //Select New Objective Point Here.
            GoToNextObjective();
            //Debug.Log("Go Next Point: " + gameObject.name);
            //Destroy(gameObject);
        }
        else
        {
            _enemyFSM.Trigger(StateEvent.SIEGEOBJECTIVE);
        }
    }

    private void OnObjectiveExit(Transform point)
    {
        point.GetComponent<ObjectivePoint>().RemoveSieger(this);
    }

    private void Update()
    {
       _enemyFSM.OnLogic(); 
    }

    private void StopMeleePlayer(Vector3 lastPosition)
    {
        if (Vector3.Distance(_playerTarget.position, transform.position) < 25)
        {
            _enemyFSM.Trigger(StateEvent.CHASEPLAYER);
        }
        else
        {
            //Debug.Log("Player Is Completely Gone");
            _enemyFSM.Trigger(StateEvent.STOPMELEEPLAYER);
        }
    }
    
    private void MeleePlayer(Transform target)
    {
        _enemyFSM.Trigger(StateEvent.MELEEPLAYER);
    }
    private void ChasePlayer(Transform target)
    {
        _enemyFSM.Trigger(StateEvent.CHASEPLAYER);
    }
    private void LostPlayer(Vector3 lastPosition)
    {
        _enemyFSM.Trigger(StateEvent.LOSTPLAYER);
    }

    private Transform SelectObjectivePoint()
    {
        return _objectiveTarget;
    }
    
    private void OnAttack(State<EnemyState, StateEvent> state)
    {
        Vector3 playerPosition = _playerTarget.position;
        transform.LookAt(new Vector3(playerPosition.x, transform.position.y, playerPosition.z));
        _lastAttackTime = Time.time;
        
        _playerTarget.GetComponent<HealthComponent>().TakeDamage(_meleeDamage);
    }

    private bool ShouldMelee(Transition<EnemyState> state)
    {
        Vector3 playerPosition = _playerTarget.position;
        return _lastAttackTime + _attackSpeed <= Time.time &&
               Vector3.Distance(new Vector3(playerPosition.x, 0, playerPosition.z), transform.position) < _attackRadius;
    }
    
    private bool ShouldNotMelee(Transition<EnemyState> state)
    {
        Vector3 playerPosition = _playerTarget.position;
        return _lastAttackTime + _attackSpeed <= Time.time &&
               Vector3.Distance(new Vector3(playerPosition.x, 0, playerPosition.z), transform.position) > _attackRadius;
    }

    private bool ShouldSiege(Transition<EnemyState> state)
    {
        Vector3 objectivePosition = _objectiveTarget.position;
        return Vector3.Distance(objectivePosition, transform.position) <= 15 &&
               !_currentTargetPoint.IsFull() && 
               !_currentTargetPoint.IsDestroyed();
    }

    private void SelectNextObjective()
    {
        _objectiveQueue.Enqueue(_objectiveTarget);
        _objectiveTarget = _objectiveQueue.Dequeue();
        _currentTargetPoint = _objectiveTarget.GetComponent<ObjectivePoint>();
    }

    private IEnumerator Despawn(float time)
    {
        yield return new WaitForSeconds(time);
        
        //gameObject.SetActive(false);
        //ReturnToPool();
        _agent.enabled = false;
        _objectPool.ReturnGameObject(gameObject);
    }
    
    public void GoToNextObjective()
    {
        SelectNextObjective();
        _enemyFSM.Trigger(StateEvent.GOTOOBJECTIVE);
    }

    public void ReturnToPool()
    {
        StartCoroutine(Despawn(4));
    }

    public void Spawn()
    {
        _agent.enabled = true;
        _enemyFSM.Trigger(StateEvent.SPAWN);
    }
}
