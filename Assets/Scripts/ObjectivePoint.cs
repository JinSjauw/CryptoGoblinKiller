using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class ObjectivePoint : MonoBehaviour
{
    [SerializeField] private Transform _debugPrefab;
    [SerializeField] private bool _spawnDebug;
    
    [SerializeField] private int _maxSieging;
    [SerializeField] private float _siegeRadius;
    
    [SerializeField] private int _maxGuarding;
    [SerializeField] private float _guardRadius;

    public Guid ID { get; } = Guid.NewGuid();
    
    public UnityAction<float, float, Guid> ObjectiveHealthChangedEvent;
    public UnityAction ObjectiveDestructionEvent;
    
    private HealthComponent _healthComponent;
    private SensorRange _sensorRange;
    
    private List<Vector3> _surroundPositions;
    private List<Vector3> _openSiegePositions;
    
    private List<Vector3> _guardPositions;
    private List<Vector3> _openGuardPositions;
    
    private Dictionary<EnemyAgent, Vector3> _siegerList;
    private Dictionary<EnemyAgent, Vector3> _guardList;
    
    #region Unity Functions

    private void Awake()
    {
        _healthComponent = GetComponent<HealthComponent>();
        _sensorRange = GetComponentInChildren<SensorRange>();
        
        _surroundPositions = CalculateCircledPositions(_siegeRadius, _maxSieging, transform.position);
        _openSiegePositions = new List<Vector3>(_surroundPositions);
        
        _guardPositions = CalculateCircledPositions(_guardRadius, _maxGuarding, transform.position);
        _openGuardPositions = new List<Vector3>(_guardPositions);
        
        _siegerList = new Dictionary<EnemyAgent, Vector3>();
        _guardList = new Dictionary<EnemyAgent, Vector3>();
    }

    private void Start()
    {
        _healthComponent.DeathEvent += OnDestruction;
        _healthComponent.HealthChangeEvent += OnHealthChange;
        _sensorRange.OnEnterRange += OnEnterRange;
        _sensorRange.OnExitRange += OnExitRange;
    }

    private void OnExitRange(Transform other)
    {
        EnemyAgent agent = other.GetComponentInParent<EnemyAgent>();
        
        if(other.GetComponent<SensorRange>()) return;
        //Debug.Log(other.name);
        
        if (agent != null)
        {
            //Debug.Log(agent.name);

            if (_siegerList.ContainsKey(agent))
            {
                _openSiegePositions.Add(_siegerList[agent]);
                _siegerList.Remove(agent);
            }

            if (_guardList.ContainsKey(agent))
            {
                _openGuardPositions.Add(_guardList[agent]);
                _guardList.Remove(agent);
            }
            
            agent.AgentDeathEvent -= OnAgentDeath;

            /*
            if (_openSiegePositions.Count > 0 && _guardList.Count > 0)
            {
                EnemyAgent agentToSwap = _guardList.First().Key;
                _openGuardPositions.Remove(_guardList[agentToSwap]);
                
                Vector3 surroundPosition = _openSiegePositions[Random.Range(0, _openSiegePositions.Count)];
                _openSiegePositions.Remove(surroundPosition);
                agentToSwap.SetObjectivePosition(surroundPosition, NPCStates.SIEGING);
                _siegerList.Add(agentToSwap, surroundPosition);
            }*/
        }
    }
    
    private void OnEnterRange(Transform other)
    {
        EnemyAgent agent = other.GetComponentInParent<EnemyAgent>();
        
        if(other.GetComponent<SensorRange>()) return;
        
        //Debug.Log(other.name);
        
        if (agent != null)
        {
            if(agent.State == NPCStates.CHASING || agent.State == NPCStates.ATTACKING) return;
            
            if(_siegerList.ContainsKey(agent) || _guardList.ContainsKey(agent)) return;
            
            if (_healthComponent.Health <= 0 || (_openGuardPositions.Count <= 0 && _openSiegePositions.Count <= 0))
            {
                agent.GoNextPoint();
                //agent.Stop();
                return;
            }
            
            //Debug.Log(agent.name);

            if (agent.State == NPCStates.CHASING || agent.State == NPCStates.ATTACKING)
            {
                return;
            }
            
            if (_openSiegePositions.Count > 0 && !_siegerList.ContainsKey(agent))
            {
                Vector3 surroundPosition = _openSiegePositions[Random.Range(0, _openSiegePositions.Count)];
                
                _openSiegePositions.Remove(surroundPosition);
                agent.SetObjectivePosition(surroundPosition, NPCStates.SIEGING);
                _siegerList.Add(agent, surroundPosition);
            }
            else if (_openGuardPositions.Count > 0 && !_guardList.ContainsKey(agent))
            {
                Vector3 guardPosition = _openGuardPositions[Random.Range(0, _openGuardPositions.Count)];
                
                _openGuardPositions.Remove(guardPosition); 
                agent.SetObjectivePosition(guardPosition, NPCStates.GUARDING);
                _guardList.Add(agent, guardPosition);
            }
            
            agent.AgentDeathEvent += OnAgentDeath;
        }
    }
    

    private void OnDisable()
    {
        _healthComponent.DeathEvent -= OnDestruction;
    }

    private void OnDestruction(object sender, EventArgs e)
    {
        foreach (EnemyAgent agent in _guardList.Keys)
        {
            agent.GoNextPoint();
            agent.AgentDeathEvent -= OnAgentDeath;
        }

        foreach (EnemyAgent agent in _siegerList.Keys)
        {
            agent.GoNextPoint();
            agent.AgentDeathEvent -= OnAgentDeath;
        }
        
        ObjectiveDestructionEvent?.Invoke();
    }
    
    private void OnHealthChange(float health, float maxHealth)
    {
        ObjectiveHealthChangedEvent?.Invoke(health, maxHealth, ID);
    }

    private void OnAgentDeath(object sender, EnemyAgent deadAgent)
    {
        if (_siegerList.ContainsKey(deadAgent))
        {
            _openSiegePositions.Add(_siegerList[deadAgent]);
            _siegerList.Remove(deadAgent);
        }

        if (_guardList.ContainsKey(deadAgent))
        {
            _openGuardPositions.Add(_guardList[deadAgent]);
            _guardList.Remove(deadAgent);
        }
        
        deadAgent.AgentDeathEvent -= OnAgentDeath;
    }
    #endregion

    #region Private Functions
    
    private List<Vector3> CalculateCircledPositions(float radius, int amount, Vector3 center)
    {
        List<Vector3> positions = new List<Vector3>();
        
        
        for (int i = 0; i < amount; i++)
        {
            Vector3 newPosition = new Vector3(
                center.x + radius * Mathf.Cos(2 * Mathf.PI * i / amount),
                center.y,
                center.z + radius * Mathf.Sin(2 * Mathf.PI * i / amount));
            
            positions.Add(newPosition);

            if (_spawnDebug)
            {
                Instantiate(_debugPrefab, newPosition, Quaternion.identity);
            }
        }
        
        return positions;
    }

    #endregion
}
