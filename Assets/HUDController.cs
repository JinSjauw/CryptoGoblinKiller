using TMPro;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] private PlayerEventChannel _playerEventChannel;
    [SerializeField] private WeaponEventChannel _weaponEventChannel;

    [Header("Field Refs")]
    [SerializeField] private Image _healthBar;
    [SerializeField] private Image _shotgunIcon;
    [SerializeField] private Image _revolverIcon;
    [SerializeField] private TextMeshProUGUI ammoCounter;
    
    private int _maxHealth = 250;
    private int _maxAmmo = 6;
    
    private void Start()
    {
        //Subscribe all events here
        _playerEventChannel.PlayerSpawnEvent += PlayerSpawned;
        _playerEventChannel.ChangedHealthEvent += HealthChanged;

        _weaponEventChannel.WeaponChangeEvent += WeaponChanged;
        _weaponEventChannel.FireEvent += WeaponFire;
        _weaponEventChannel.ReloadStartEvent += WeaponReload;
    }

    private void WeaponReload(int maxAmmo)
    {
        ammoCounter.text = maxAmmo + "/" + maxAmmo;
        _maxAmmo = maxAmmo;
    }

    private void WeaponFire(int ammo)
    {
        ammoCounter.text = ammo + "/" + _maxAmmo;
    }

    private void WeaponChanged(WeaponType type)
    {
        if (type == WeaponType.SHOTGUN)
        {
            _shotgunIcon.gameObject.SetActive(true);
            _revolverIcon.gameObject.SetActive(false);
        }
        else
        {
            _shotgunIcon.gameObject.SetActive(false);
            _revolverIcon.gameObject.SetActive(true);
        }
        
        
    }

    private void PlayerSpawned(int maxHealth)
    {
        _maxHealth = maxHealth;
    }

    private void HealthChanged(float health)
    {
        _healthBar.fillAmount = health / _maxHealth;
    }
}
