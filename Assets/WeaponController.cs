using Unity.Mathematics;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private Transform _bulletPrefab;
    [SerializeField] private Rigidbody _playerBody;
    [SerializeField] private CameraController _cameraController;
    
    
    [SerializeField] private Transform _muzzlePosition;
    [SerializeField] private float _projectileSpeed;
    [SerializeField] private float _projectileDamage;
    
    // Start is called before the first frame update
    void Start()
    {
        _inputHandler.ShootEvent += Shoot;
    }

    // Update is called once per frame
    void Update()
    {
       
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
        Transform bulletObject = Instantiate(_bulletPrefab, _muzzlePosition.position, Quaternion.identity);
        if (bulletObject.TryGetComponent(out Projectile bullet))
        {
            bullet.Shoot(direction.normalized, _projectileSpeed, _projectileDamage);
        }
    }
}
