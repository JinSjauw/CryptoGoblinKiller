using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ObjectivePoint : MonoBehaviour
{
    [SerializeField] private Transform _debugPrefab;
    [SerializeField] private bool _spawnDebug;
    
    [SerializeField] private int _maxSieging;
    [SerializeField] private float _siegeRadius;
    
    [SerializeField] private int _maxGuarding;
    [SerializeField] private float _guardRadius;

    private HealthComponent _healthComponent;
    
    private List<Vector3> _surroundPositions;
    private List<Vector3> _openSurroundPositions;
    
    private List<Vector3> _guardPositions;
    private List<Vector3> _openGuardPositions;
    
    private Dictionary<EnemyAgent, Vector3> _siegerList;
    private Dictionary<EnemyAgent, Vector3> _guardList;
    
    #region Unity Functions

    private void Awake()
    {
        _healthComponent = GetComponent<HealthComponent>();
        
        _surroundPositions = CalculateCircledPositions(_siegeRadius, _maxSieging, transform.position);
        _openSurroundPositions = new List<Vector3>(_surroundPositions);
        
        _guardPositions = CalculateCircledPositions(_guardRadius, _maxGuarding, transform.position);
        _openGuardPositions = new List<Vector3>(_guardPositions);
        
        _siegerList = new Dictionary<EnemyAgent, Vector3>();
        _guardList = new Dictionary<EnemyAgent, Vector3>();
    }

    private void Start()
    {
        _healthComponent.DeathEvent += OnDestruction;
    }

    private void OnDestruction(object sender, EventArgs e)
    {
        foreach (EnemyAgent agent in _guardList.Keys)
        {
            agent.GoNextPoint();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyAgent agent = other.GetComponentInParent<EnemyAgent>();
        
        //Debug.Log(other.name);
        
        if(_healthComponent.Health <= 0) agent.GoNextPoint();
        
        if (agent != null)
        {
            Debug.Log(agent.name);

            if (agent.State == NPCStates.MOVING && _openSurroundPositions.Count > 0 && !_siegerList.ContainsKey(agent))
            {
                Vector3 surroundPosition = _openSurroundPositions[Random.Range(0, _openSurroundPositions.Count)];
                
                agent.SetObjectivePosition(surroundPosition, NPCStates.SIEGING);
                _siegerList.Add(agent, surroundPosition);
                _openSurroundPositions.Remove(surroundPosition);
            }
            else if (agent.State == NPCStates.MOVING && _openGuardPositions.Count > 0 && !_guardList.ContainsKey(agent))
            {
                Vector3 guardPosition = _openGuardPositions[Random.Range(0, _openGuardPositions.Count)];
                
                agent.SetObjectivePosition(guardPosition, NPCStates.GUARDING);
                _guardList.Add(agent, guardPosition);
                _openGuardPositions.Remove(guardPosition); 
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        EnemyAgent agent = other.GetComponentInParent<EnemyAgent>();
        
        //Debug.Log(other.name);
        
        if (agent != null)
        {
            Debug.Log(agent.name);

            if (_siegerList.ContainsKey(agent))
            {
                _openSurroundPositions.Add(_siegerList[agent]);
                _siegerList.Remove(agent);
            }

            if (_guardList.ContainsKey(agent))
            {
                _openGuardPositions.Add(_guardList[agent]);
                _guardList.Remove(agent);
            }
        }
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
