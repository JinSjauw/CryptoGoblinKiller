using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private ObjectPool _objectPool;

    [SerializeField] private Rigidbody _playerBody;
    [SerializeField] private CameraController _cameraController;

    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private VisualEffect _muzzleFlash;
    [SerializeField] private Transform _muzzleTransform;

    [SerializeField] private bool _isShotgun;
    [SerializeField] private ShotgunSpread _shotgunSpread;
    
    [SerializeField] private float _projectileSpeed;
    [SerializeField] private float _projectileDamage;

    [SerializeField] private LayerMask _obstacleLayer;
    
    private float _maxAimCheckDistance = 100;
    
    // Start is called before the first frame update
    void Start()
    {
        _inputHandler.ShootEvent += Shoot;
    }

    private void Update()
    {
        //Vector3 direction = _cameraController.CrossHairRay().direction;
    }

    private void Shoot()
    {
        HandleShoot();
    }
    
    private void HandleShoot()
    {
        _muzzleFlash.Play();
        //Get direction to shoot in.
        Ray aimRay = _cameraController.CrossHairRay();
        Vector3 direction;

        if (Physics.Raycast(aimRay, out RaycastHit hit, _maxAimCheckDistance, _obstacleLayer))
        {
            direction = hit.point - _muzzleTransform.position;
        }
        else
        {
            direction = (aimRay.origin + aimRay.direction * _maxAimCheckDistance) - _muzzleTransform.position;
        }
        
        if (_isShotgun)
        {
            _shotgunSpread.transform.forward = direction.normalized;
            
            List<Transform> pelletPositions = _shotgunSpread.GetShotgunSpread();
            foreach (Transform pellet in pelletPositions)
            {
                //spawn projectile
                GameObject pelletObject = _objectPool.GetObject(_bulletPrefab);
                pelletObject.transform.position = pellet.position;
                if (pelletObject.TryGetComponent(out Projectile projectile))
                {
                    projectile.SetPool(_objectPool);
                    projectile.Shoot(pellet.forward, _projectileSpeed / 2, _projectileDamage, true);
                }
                //launch projectile in its own forward;
            }
            
            return;
        }
        //Spawn bullet
        GameObject bulletObject = _objectPool.GetObject(_bulletPrefab);
        bulletObject.transform.position = _muzzleTransform.position;
        if (bulletObject.TryGetComponent(out Projectile bullet))
        {
            bullet.SetPool(_objectPool);
            bullet.Shoot(direction.normalized, _projectileSpeed, _projectileDamage);
        }
    }
}
