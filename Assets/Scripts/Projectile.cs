using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

public class Projectile : MonoBehaviour
{
    [SerializeField] private GameObject _impactVFX;
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
            Vector3 direction = _currentPosition - _lastPosition;
            float angle = Vector3.Dot(hit.normal, Vector3.up);
            
            GameObject impact = _objectPool.GetObject(_impactVFX);
            impact.transform.position = hit.point;
            
            //Debug.Log(hit.collider.name);
            
            if (Mathf.Abs(angle) > .7f)
            {
                //Debug.Log(angle);
                impact.transform.localRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(direction.normalized, hit.normal));
            }
            else
            {
                impact.transform.localRotation = Quaternion.LookRotation(Vector3.up, hit.normal);
            }
            
            impact.GetComponent<ReturnToPool>().SetPool(_objectPool);
            
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
