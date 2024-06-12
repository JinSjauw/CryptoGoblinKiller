using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class WeaponController : MonoBehaviour
{
    [Header("Object Refs")]
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private WeaponEventChannel _weaponEventChannel;
    [SerializeField] private ObjectPool _objectPool;
    [SerializeField] private Rigidbody _playerBody;
    [SerializeField] private CameraController _cameraController;
    
    [SerializeField] private GameObject _bulletPrefab;

    [Header("Revolver Refs")] 
    [SerializeField] private GameObject _revolverMesh;
    [SerializeField] private WeaponData _revolverData;
    [SerializeField] private VisualEffect _revolverMuzzleFlash;
    [SerializeField] private Transform _revolverMuzzleTransform;

    [SerializeField] private Transform _revolverSpinAxis;

    [Header("Shotgun Refs")] 
    [SerializeField] private GameObject _shotgunMesh;
    [SerializeField] private WeaponData _shotgunData;
    [SerializeField] private VisualEffect _shotGunMuzzleFlash;
    [SerializeField] private Transform _shotgunMuzzleTransform;
    [SerializeField] private ShotgunSpread _shotgunSpread;

    [SerializeField] private Transform _shotgunSpinAxis;
    
    [Header("Obstacle Layer")]
    [SerializeField] private LayerMask _obstacleLayer;
    
    [Header("Weapon Handling")] 
    [SerializeField] private float _switchingTime;
    [SerializeField] private float _inputBufferTime;
    
    private WeaponRecoil _weaponRecoil;
    private WeaponRecoilSpring _weaponRecoilSpring;
    
    private float _maxAimCheckDistance = 100;
    
    private VisualEffect _muzzleFlash;
    private Transform _muzzleTransform;
    
    //Current Weapon Data
    private float _projectileDamage;
    private float _projectileVelocity;
    private float _fireRate;
    private float _reloadTime;
    private WeaponType _weaponType;
    private int _magSize;
    private int _reserveAmmo;

    //Weapon Logic Data
    
    private bool _canShoot;
    private int _currentAmmo;
    
    private int _shotgunAmmo;
    private int _revolverAmmo;
    
    private float _fireTimer;
    private bool _isReloading;
    private float _reloadTimer;

    //Input buffer
    private int _shootInputBuffer = 0;
    private float _bufferTimer;
    
    #region Unity Functions

    private void Awake()
    {
        if (_objectPool == null)
        {
            _objectPool = FindObjectOfType<ObjectPool>();
        }

        _weaponRecoil = GetComponentInChildren<WeaponRecoil>();
        _weaponRecoilSpring = GetComponentInChildren<WeaponRecoilSpring>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _inputHandler.ShootEvent += Shoot;
        _inputHandler.WeaponChangeEvent += ChangeWeapon;
        _inputHandler.WeaponReloadEvent += Reload;

        _shotgunAmmo = _shotgunData.MagSize;
        _revolverAmmo = _revolverData.MagSize;
        
        ChangeWeapon(-1);
    }

    private void Update()
    {
        //_cameraController.CrossHairRay();
        if (_shootInputBuffer > 0)
        {
            _bufferTimer += Time.deltaTime;
            if (_bufferTimer >= _inputBufferTime)
            {
                _shootInputBuffer = 0;
                _bufferTimer = 0;
            }
        }
        
        CanShoot();

        if (_isReloading)
        {
            _reloadTimer += Time.deltaTime;
            if (_reloadTimer > _reloadTime)
            {
                _reloadTimer = 0;
                _isReloading = false;
                _currentAmmo = _magSize;
                _canShoot = true;
            }
        }
    }
    
    #endregion

    #region CallBacks

    private void ChangeWeapon(float state)
    {
        if (state >= 1)
        {
            //_isShotgun = false;
            if (_weaponType == WeaponType.REVOLVER) return;
            
            _shotgunMesh.SetActive(false);
            _revolverMesh.SetActive(true);
            
            ChangeWeaponData(_revolverData);
            _muzzleFlash = _revolverMuzzleFlash;
            _muzzleTransform = _revolverMuzzleTransform;

            _shotgunAmmo = _currentAmmo;
            _currentAmmo = _revolverAmmo;
            
            _weaponRecoil.ChangeWeapon(_revolverSpinAxis, _revolverData.RecoilData);
        }
        else if (state <= -1)
        {
            if(_weaponType == WeaponType.SHOTGUN) return;
            
            _shotgunMesh.SetActive(true);
            _revolverMesh.SetActive(false);
            
            ChangeWeaponData(_shotgunData);
            _muzzleFlash = _shotGunMuzzleFlash;
            _muzzleTransform = _shotgunMuzzleTransform;

            _revolverAmmo = _currentAmmo;
            _currentAmmo = _shotgunAmmo;
            
            _weaponRecoil.ChangeWeapon(_shotgunSpinAxis, _shotgunData.RecoilData);
        }
        
        _weaponEventChannel.OnWeaponChange(_weaponType);
        
        _canShoot = true;
        //_currentAmmo = _magSize;
    }
    
    private void Shoot()
    {
        if (_canShoot)
        {
            _currentAmmo--;
            _canShoot = false;
            
            _weaponEventChannel.OnFire(_currentAmmo);
            HandleShoot();
        }
        else if (!_isReloading && _currentAmmo <= 0)
        {
            _weaponEventChannel.OnDryFire();
        }
        else
        {
            //store and play next possible moment
            _shootInputBuffer++;
        }
    }

    private void Reload()
    {
        if (!_isReloading)
        {
            _weaponEventChannel.OnReloadStart(_magSize, _reloadTime);
            _canShoot = false;
            _isReloading = true;
            //Play Reload Anim;
            Transform target = _weaponType == WeaponType.REVOLVER ? _revolverSpinAxis : _shotgunSpinAxis;
            StartCoroutine(PlayReload(_reloadTime, target));
        }
    }

    #endregion

    #region Private Functions

    private IEnumerator PlayReload(float duration, Transform target)
    {
        Quaternion startRot = target.localRotation;
        float t = 0.0f;
        while ( t  < duration )
        {
            t += Time.deltaTime;
            target.localRotation = startRot * Quaternion.AngleAxis(t / duration * 360f, Vector3.right);
            yield return null;
        }
        target.localRotation = startRot;
    }
    
    private void ChangeWeaponData(WeaponData data)
    {
        _projectileDamage = data.Damage;
        _projectileVelocity = data.MuzzleVelocity;
        _fireRate = data.FireRate;
        _reloadTime = data.ReloadTime;
        _weaponType = data.WeaponType;
        _magSize = data.MagSize;
        _reserveAmmo = data.ReserveAmmo;
        
        
    }

    private void CanShoot()
    {
        if (!_canShoot && _currentAmmo > 0 && !_isReloading)
        {
            _fireTimer += Time.deltaTime;
            if (_fireTimer >= 60 / _fireRate)
            {
                _fireTimer = 0;
                _canShoot = true;

                if (_shootInputBuffer > 0)
                {
                    _shootInputBuffer--;
                    Shoot();
                }
            }
        }
    }
    
    private void HandleShoot()
    {
        //Play Recoil
        
        if(_weaponRecoil != null) _weaponRecoil.PlayRecoil();
        if(_weaponRecoilSpring != null) _weaponRecoilSpring.PlayRecoil();
        
        //Play MuzzleFlash VFX
        _muzzleFlash.Play();
        
        //Get direction to shoot in.
        Ray aimRay = _cameraController.CrossHairRay();
        Vector3 direction;

        if (Physics.Raycast(aimRay, out RaycastHit hit, _maxAimCheckDistance, _obstacleLayer))
        {
            direction = hit.point - _muzzleTransform.position;
            /*Debug.DrawLine(_muzzleTransform.position, direction.normalized * hit.distance, Color.green, 2.5f);
            Debug.Log("Hit: " + hit.collider.name);*/
        }
        else
        {
            direction = aimRay.origin + (aimRay.direction * _maxAimCheckDistance) - _muzzleTransform.position;
        }
        
        //Check if current weapon is shotgun.
        if (_weaponType == WeaponType.SHOTGUN)
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
                    projectile.Shoot(pellet.forward, _projectileVelocity / 2, _projectileDamage, true);
                }
            }
            
            return;
        }
        
        //Spawn bullet
        GameObject bulletObject = _objectPool.GetObject(_bulletPrefab);
        bulletObject.transform.position = _muzzleTransform.position;
        if (bulletObject.TryGetComponent(out Projectile bullet))
        {
            bullet.SetPool(_objectPool);
            bullet.Shoot(direction.normalized, _projectileVelocity, _projectileDamage);
        }
    }

    #endregion
    
    
}
