using System;
using System.Collections.Generic;
using Ami.BroAudio;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class ObjectivePoint : MonoBehaviour
{
    [SerializeField] private Transform _debugPrefab;
    [SerializeField] private bool _spawnDebug;

    [SerializeField] private Shape _shapeID;
    [SerializeField] private int _maxSieging;
    [SerializeField] private float _siegeRadius;

    [Header("Mesh Components")] 
    [SerializeField] private List<Rigidbody> _meshComponents;

    [Header("Explosion")] 
    [SerializeField] private ParticleSystem _explosion;
    [SerializeField] private Transform _explosionPoint;
    
    public Guid ID { get; } = Guid.NewGuid();
    public Shape ShapeID => _shapeID;
    
    public UnityAction<float, float, Shape> ObjectiveHealthChangedEvent;
    public UnityAction ObjectiveDestructionEvent;
    
    private HealthComponent _healthComponent;
    private SoundSource _soundSource;
    
    private List<Vector3> _surroundPositions;
    private List<Vector3> _openSiegePositions;
    
    private Dictionary<Enemy, Vector3> _siegerList;

    private bool _isDestroyed;
    
    #region Unity Functions

    private void Awake()
    {
        _healthComponent = GetComponent<HealthComponent>();
        _soundSource = GetComponent<SoundSource>();
        
        _surroundPositions = CalculateCircledPositions(_siegeRadius, _maxSieging, transform.position);
        _openSiegePositions = new List<Vector3>(_surroundPositions);
        
        _siegerList = new Dictionary<Enemy, Vector3>();
    }

    private void Start()
    {
        _healthComponent.DeathEvent += OnDestruction;
        _healthComponent.HealthChangeEvent += OnHealthChange;
        //Explode();
    }
    
    private void OnDisable()
    {
        _healthComponent.DeathEvent -= OnDestruction;
    }

    private void OnDestruction(object sender, EventArgs e)
    {
        if (_isDestroyed) return;

        foreach (Enemy enemy in _siegerList.Keys)
        {
            enemy.GoToNextObjective();
        }
        
        Explode();
        ObjectiveDestructionEvent?.Invoke();
    }
    
    private void OnHealthChange(float health, float maxHealth)
    {
        ObjectiveHealthChangedEvent?.Invoke(health, maxHealth, ShapeID);
    }

    private void OnSiegerDeath(Enemy deadAgent)
    {
        if (_siegerList.ContainsKey(deadAgent))
        {
            _openSiegePositions.Add(_siegerList[deadAgent]);
            _siegerList.Remove(deadAgent);
        }
    }
    #endregion

    #region Private Functions

    private void Explode()
    {
        _soundSource.Play();
        //Enable RigidBodies on mesh and explode with impulse.
        _explosion.Play();
        foreach (Rigidbody meshComponent in _meshComponents)
        {
            meshComponent.isKinematic = false;
            meshComponent.useGravity = true;

            Vector2 randomOffset = Random.insideUnitCircle * 1.2f;
            Vector3 explosionOrigin = _explosionPoint.position + new Vector3(randomOffset.x, 0, randomOffset.y);
            
            meshComponent.AddExplosionForce(1000, explosionOrigin, 10);
        }
    }
    
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

    #region Public Functions

    public bool IsFull()
    {
        return _openSiegePositions.Count <= 0;
    }

    public bool IsDestroyed()
    {
        return _healthComponent.Health <= 0;
    }

    public bool IsSieger(Enemy enemy)
    {
        return _siegerList.ContainsKey(enemy);
    }
    
    public Vector3 GetSiegePosition(Enemy enemy)
    {
        Vector3 surroundPosition;
        
        if (!_siegerList.ContainsKey(enemy))
        {
            surroundPosition = _openSiegePositions[Random.Range(0, _openSiegePositions.Count)];
            _openSiegePositions.Remove(surroundPosition);
            _siegerList.Add(enemy, surroundPosition);

            enemy.OnEnemyDeath += OnSiegerDeath;
        }
        else
        {
            surroundPosition = _siegerList[enemy];
        }

        return surroundPosition;
    }

    public void RemoveSieger(Enemy enemy)
    {
        if (_siegerList.ContainsKey(enemy))
        {
            _openSiegePositions.Add(_siegerList[enemy]);
            _siegerList.Remove(enemy);
            enemy.OnEnemyDeath -= OnSiegerDeath;
        }
    }

    #endregion
}

public enum Shape
{
    TORUS = 0,
    CUBE = 1,
    PYRAMID = 2,
    CROSS = 3,
}