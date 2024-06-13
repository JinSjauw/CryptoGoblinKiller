using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event Channels/Weapon Event Channel")]
public class WeaponEventChannel : ScriptableObject
{
    public UnityAction<WeaponType> WeaponChangeEvent;
    public UnityAction<int> FireEvent;
    public UnityAction DryFireEvent;
    public UnityAction<int, float> ReloadStartEvent;
    public UnityAction ReloadEndEvent;

    public void OnWeaponChange(WeaponType type)
    {
        WeaponChangeEvent?.Invoke(type);
    }

    public void OnFire(int ammo)
    {
        FireEvent?.Invoke(ammo);
    }

    public void OnDryFire()
    {
        DryFireEvent?.Invoke();
    }

    public void OnReloadStart(int ammo, float reloadTime)
    {
        ReloadStartEvent?.Invoke(ammo, reloadTime);
    }

    public void OnReloadEnd()
    {
        ReloadEndEvent?.Invoke();
    }

    public void Unsubscribe()
    {
        WeaponChangeEvent = null;
        FireEvent = null;
        DryFireEvent = null;
        ReloadStartEvent = null;
        ReloadEndEvent = null;
    }
}
