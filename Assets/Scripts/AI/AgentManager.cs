using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class AgentManager : MonoBehaviour
{
    [SerializeField] private WaveEventChannel _waveEventChannel;
    [SerializeField] private GameObject _agentPrefab;

    [SerializeField] private Transform _playerTransform;
    [SerializeField] private List<Transform> _defendPoints;
    [SerializeField] private List<Transform> _spawnPoints;
    
    [SerializeField] private List<Enemy> _activeList;
    
    //Wave Settings
    [SerializeField] private WaveSpawnType _waveSpawnType;
    [SerializeField] private int _waveAmount;
    [SerializeField] private int _waveDelay;
    [SerializeField] private int _enemyAmount;

    [SerializeField] private float _timeBetweenWaves;
    
    private ObjectPool _objectPool;

    private int _waveNumber;
    [SerializeField] private float _waveTimer;
    
    #region Unity Functions

    private void Awake()
    {
        _objectPool = FindObjectOfType<ObjectPool>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _waveEventChannel.OnNewWave(_waveNumber + 1, _waveAmount, _waveDelay);
        StartCoroutine(SpawnWave(_waveDelay, () =>
        {
            _waveEventChannel.OnNewWave(_waveNumber + 1, _waveAmount, _timeBetweenWaves);
        }));
    }

    private void Update()
    {
        if(_waveSpawnType != WaveSpawnType.ONTIMER) return;
        
        _waveTimer += Time.deltaTime;
        if (_waveTimer > _timeBetweenWaves)
        {
            _waveEventChannel.OnNewWave(_waveNumber + 1, _waveAmount, _timeBetweenWaves);
            _waveTimer = 0;
            StartCoroutine(SpawnWave());
        }
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
            if (_activeList.Count == 0 && _waveNumber < _waveAmount && _waveSpawnType == WaveSpawnType.ONCLEAR)
            {
                StartCoroutine(SpawnWave(_waveDelay));
            }

            if (_activeList.Count == 0 && _waveNumber >= _waveAmount)
            {
                _waveEventChannel.OnWavesCleared();
            }

            if (_activeList.Count == 0 && _waveNumber <= _waveAmount && _waveTimer < _timeBetweenWaves && _waveSpawnType == WaveSpawnType.ONTIMER)
            {
                _waveEventChannel.OnNewWave(_waveNumber + 1, _waveAmount, _waveDelay);
                _waveTimer = 0;
                StartCoroutine(SpawnWave(_waveDelay, () =>
                {
                    _waveEventChannel.OnNewWave(_waveNumber + 1, _waveAmount, _timeBetweenWaves);
                }));
            }
        }
    }

    private IEnumerator SpawnWave(float delay = 0, Action onWaveSpawn = null)
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

        onWaveSpawn?.Invoke();
        
        yield return null;
    }

    #endregion
}

public enum WaveSpawnType
{
    ONCLEAR = 0,
    ONTIMER = 1,
}
