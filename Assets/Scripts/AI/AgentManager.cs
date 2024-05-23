using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AgentManager : MonoBehaviour
{
    [SerializeField] private GameObject _agentPrefab;

    [SerializeField] private Transform _playerTransform;
    [SerializeField] private List<Transform> _defendPoints;
    [SerializeField] private List<Transform> _spawnPoints;
    
    [SerializeField] private List<EnemyAgent> _activeList;
    
    //Wave Settings
    
    [SerializeField] private int _waveAmount;
    [SerializeField] private int _waveDelay;
    
    [SerializeField] private int _enemyAmount;
    
    private ObjectPool _objectPool;

    private int _waveNumber;
    
    #region Unity Functions

    private void Awake()
    {
        _objectPool = FindObjectOfType<ObjectPool>();
        
        //Find Object pool, otherwise create one?
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnWave(_waveDelay));
    }

    #endregion

    #region Public Functions

    public void KillWave()
    {
        foreach (EnemyAgent agent in _activeList)
        {
            agent.GetComponent<HealthComponent>().TakeDamage(10000);
        }
    }

    #endregion
    
    #region Private Regions

    private void SpawnAgent(Vector3 position)
    {
        if (_objectPool.GetObject(_agentPrefab).TryGetComponent(out EnemyAgent agent))
        {
            _activeList.Add(agent);
            agent.AgentDeathEvent += OnAgentDeath;
            agent.transform.position = position + Random.insideUnitSphere;
            
            if(agent.IsInitialized()) return;

            Transform targetObjective = _defendPoints[Random.Range(0, _defendPoints.Count)];
            List<Transform> objectivesList = new List<Transform>(_defendPoints);
            objectivesList.Remove(targetObjective);
            
            agent.Initialize(_playerTransform, targetObjective, objectivesList, _objectPool);
        }
    }

    private void OnAgentDeath(object sender, EnemyAgent agent)
    {
        if (_activeList.Contains(agent))
        {
            _activeList.Remove(agent);
        }

        //Spawn new Wave?
        if (_activeList.Count <= 0 && _waveNumber < _waveAmount)
        {
            StartCoroutine(SpawnWave(_waveDelay));
        }
    }

    private IEnumerator SpawnWave(float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        
        _waveNumber++;
        
        int enemyPerPoint = (_enemyAmount * _waveNumber) / _spawnPoints.Count;
        
        for (int i = 0; i < _spawnPoints.Count; i++)
        {
            for (int j = 0; j < enemyPerPoint; j++)
            {
                SpawnAgent(_spawnPoints[i].position);
            }
        }

        yield return null;
    }

    #endregion
}
