using System;
using Ami.BroAudio;
using UnityEngine;

public class WeaponSoundController : MonoBehaviour
{
    [Header("Object Refs")] 
    [SerializeField] private WeaponEventChannel _weaponEventChannel;
    [SerializeField] private Transform _muzzle;
    
    [Header("Shotgun")] 
    [SerializeField] private SoundID _shotgunFire;
    [SerializeField] private SoundID _shotgunDryFire;
    [SerializeField] private SoundID _shotgunReload;
    
    [Header("Revolver")] 
    [SerializeField] private SoundID _revolverFire;
    [SerializeField] private SoundID _revolverDryFire;
    [SerializeField] private SoundID _revolverReload;

    [Header("Default")] 
    [SerializeField] private WeaponType _defaultType;
    
    private WeaponType _currentWeapon;
    private SoundID _currentFireAudio = default;
    private SoundID _currentDryFireAudio = default;
    private SoundID _currentReloadAudio = default;

    private void Start()
    {
        _weaponEventChannel.WeaponChangeEvent += WeaponChange;
        _weaponEventChannel.FireEvent += Fire;
        _weaponEventChannel.DryFireEvent += DryFire;
        _weaponEventChannel.ReloadStartEvent += Reload;
        
        WeaponChange(_defaultType);
    }

    private void Reload(int ammo)
    {
        BroAudio.Play(_currentReloadAudio, _muzzle);
    }

    private void DryFire()
    {
        BroAudio.Play(_currentDryFireAudio, _muzzle);
    }

    private void Fire(int ammo)
    {
        BroAudio.Play(_currentFireAudio, _muzzle);
    }

    private void WeaponChange(WeaponType type)
    {
        _currentWeapon = type;

        switch (_currentWeapon)
        {
            case WeaponType.SHOTGUN:
                _currentFireAudio = _shotgunFire;
                _currentDryFireAudio = _shotgunDryFire;
                _currentReloadAudio = _shotgunReload;
                break;
            case WeaponType.REVOLVER:
                _currentFireAudio = _revolverFire;
                _currentDryFireAudio = _revolverDryFire;
                _currentReloadAudio = _revolverReload;
                break;
        }
    }
}
