using UnityEngine;
using UnityEngine.Serialization;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private ObjectPool _objectPool;
    
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Rigidbody _playerBody;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private Transform _muzzleTransform;
    [SerializeField] private float _projectileSpeed;
    [SerializeField] private float _projectileDamage;
    
    // Start is called before the first frame update
    void Start()
    {
        _inputHandler.ShootEvent += Shoot;
    }

    private void Shoot()
    {
        HandleShoot();
    }
    
    private void HandleShoot()
    {
        //Get direction to shoot in.
        Vector3 direction = _cameraController.CrossHairRay().direction;
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
