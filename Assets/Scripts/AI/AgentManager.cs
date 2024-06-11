using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class AgentManager : MonoBehaviour
{
    [SerializeField] private GameObject _agentPrefab;

    [SerializeField] private Transform _playerTransform;
    [SerializeField] private List<Transform> _defendPoints;
    [SerializeField] private List<Transform> _spawnPoints;
    
    [SerializeField] private List<Enemy> _activeList;
    
    //Wave Settings
    
    [SerializeField] private int _waveAmount;
    [SerializeField] private int _waveDelay;
    
    [SerializeField] private int _enemyAmount;
    
    private ObjectPool _objectPool;

    [SerializeField] private int _waveNumber;
    
    #region Unity Functions

    private void Awake()
    {
        _objectPool = FindObjectOfType<ObjectPool>();
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
        foreach (Enemy agent in _activeList)
        {
            agent.OnEnemyDeath -= OnAgentDeath;
            agent.GetComponent<HealthComponent>().TakeDamage(10000);
        }
        
        _activeList.Clear();

        if (_waveNumber < _waveAmount)
        {
            StartCoroutine(SpawnWave(_waveDelay));
        }
    }

    #endregion
    
    #region Private Regions

    private void SpawnAgent(Vector3 position)
    {
        if (_objectPool.GetObject(_agentPrefab).TryGetComponent(out Enemy enemy))
        {
            _activeList.Add(enemy);
            enemy.OnEnemyDeath += OnAgentDeath;
            Vector2 randomOffset = Random.insideUnitCircle;
            
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
            {
                enemy.transform.position = new Vector3(hit.position.x + randomOffset.x, hit.position.y, hit.position.z + randomOffset.y);
            }
            else
            {
                Debug.Log("Couldnt find navmesh!");
                return;
            }

            Transform targetObjective = _defendPoints[Random.Range(0, _defendPoints.Count)];
            List<Transform> objectivesList = new List<Transform>(_defendPoints);
            objectivesList.Remove(targetObjective);
            
            enemy.Initialize(_playerTransform, targetObjective, _objectPool, objectivesList);
            enemy.Spawn();
        }
    }

    private void OnAgentDeath(Enemy enemy)
    {
        if (_activeList.Contains(enemy))
        {
            _activeList.Remove(enemy);
            
            //Spawn new Wave?
            if (_activeList.Count == 0 && _waveNumber < _waveAmount)
            {
                StartCoroutine(SpawnWave(_waveDelay));
            }
        }
    }

    private IEnumerator SpawnWave(float delay = 0)
    {
        _waveNumber++;
        
        yield return new WaitForSeconds(delay);
        
        int enemyPerPoint = (_enemyAmount * _waveNumber) / _spawnPoints.Count;
        
        for (int i = 0; i < _spawnPoints.Count; i++)
        {
            for (int j = 0; j < enemyPerPoint; j++)
            {
                //Debug.Log(i + " " + j + " Enemy per Point: " + enemyPerPoint);
                SpawnAgent(_spawnPoints[i].position);
            }
        }

        yield return null;
    }

    #endregion
}
