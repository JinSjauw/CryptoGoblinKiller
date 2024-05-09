using System;
using Unity.Mathematics;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private Transform _debugImpactSphere;

    private Rigidbody _rgBody;
    
    private Vector3 _currentPosition;
    private Vector3 _lastPosition;


    private void Awake()
    {
        _rgBody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        DetectCollision();

        _lastPosition = _currentPosition;
        _currentPosition = transform.position;
    }

    private void DetectCollision()
    {
        if (Physics.Linecast(_lastPosition, _currentPosition, out RaycastHit hit))
        {
            Debug.Log("Hit " + hit.collider.name);
            Instantiate(_debugImpactSphere, hit.point, quaternion.identity);
            //Destroy(gameObject);
        }
    }
    
    public void Shoot(Vector3 direction, float speed, float damage)
    {
        _currentPosition = transform.position;
        transform.forward = direction;
        //_rgBody.AddForce(direction * speed * _rgBody.mass);
        _rgBody.velocity = direction * speed * _rgBody.mass;
    }
}
