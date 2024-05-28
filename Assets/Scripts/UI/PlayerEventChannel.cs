using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event Channels/Player Event Channel")]
public class PlayerEventChannel : ScriptableObject
{
    public UnityAction<int> ChangedHealthEvent;
    public UnityAction<int> ChangedAmmoEvent;
    public UnityAction DeathEvent;
    public UnityAction<float> ReloadStartEvent;
    public UnityAction ReloadEndEvent;
    public UnityAction PlayerSpawnEvent;

    public void OnHealthChanged(int health)
    {
        ChangedHealthEvent?.Invoke(health);
    }

    public void OnAmmoChanged(int ammo)
    {
        ChangedAmmoEvent?.Invoke(ammo);
    }

    public void OnDeath()
    {
        DeathEvent?.Invoke();
    }

    public void OnReloadStart(float reloadTime)
    {
        ReloadStartEvent?.Invoke(reloadTime);
    }

    public void OnReloadEnd()
    {
        ReloadEndEvent?.Invoke();
    }

    public void OnPlayerSpawn()
    {
        PlayerSpawnEvent?.Invoke();
    }
}
