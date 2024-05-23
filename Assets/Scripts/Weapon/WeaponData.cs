using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [SerializeField] private float _damage;
    [SerializeField] private float _muzzleVelocity;
    [SerializeField] private float _fireRate;
    [SerializeField] private float _reloadTime;
    [SerializeField] private WeaponType _weaponType;
    [SerializeField] private int _magSize;
    [SerializeField] private int _reserveAmmo;

    public float Damage => _damage;
    public float MuzzleVelocity => _muzzleVelocity;
    public float FireRate => _fireRate;
    public float ReloadTime => _reloadTime;
    public WeaponType WeaponType => _weaponType;
    public int MagSize => _magSize;
    public int ReserveAmmo => _reserveAmmo;
}

public enum WeaponType
{
    REVOLVER = 0,
    SHOTGUN = 1,
}
