using Unity.Mathematics;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private Transform _debugImpactSphere;
    [SerializeField] private LayerMask _bulletMask;
    
    private Rigidbody _rgBody;
    private ObjectPool _objectPool;
    private ReturnToPool _returnToPool;
    private TrailRenderer _trailRenderer;
    
    private Vector3 _currentPosition;
    private Vector3 _lastPosition;
    
    private void Awake()
    {
        _rgBody = GetComponent<Rigidbody>();
        _returnToPool = GetComponent<ReturnToPool>();
        _trailRenderer = GetComponent<TrailRenderer>();
    }

    private void FixedUpdate()
    {
        DetectCollision();

        _lastPosition = _currentPosition;
        _currentPosition = transform.position;
    }

    private void DetectCollision()
    {
        if (Physics.Linecast(_lastPosition, _currentPosition, out RaycastHit hit, _bulletMask))
        {
            //Debug.Log("Hit " + hit.collider.name);
            Instantiate(_debugImpactSphere, hit.point, quaternion.identity);
            _objectPool.ReturnGameObject(gameObject);
        }
    }

    public void SetPool(ObjectPool objectPool)
    {
        _objectPool = objectPool;
        _returnToPool.SetPool(objectPool);
    }
    
    public void Shoot(Vector3 direction, float speed, float damage)
    {
        _trailRenderer.Clear();
        _currentPosition = transform.position;
        _lastPosition = _currentPosition;
        transform.forward = direction;
        //_rgBody.AddForce(direction * speed * _rgBody.mass);
        _rgBody.velocity = direction * speed * _rgBody.mass;
    }
}
