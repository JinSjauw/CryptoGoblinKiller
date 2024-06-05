using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event Channels/Player Event Channel")]
public class PlayerEventChannel : ScriptableObject
{
    public UnityAction<float> ChangedHealthEvent;
    public UnityAction<int> ChangedAmmoEvent;
    public UnityAction DeathEvent;
    public UnityAction<float> ReloadStartEvent;
    public UnityAction ReloadEndEvent;
    public UnityAction<int> PlayerSpawnEvent;

    public UnityAction PlayerJumpEvent;
    public UnityAction PlayerLandEvent;
    
    public void OnHealthChanged(float health)
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

    public void OnPlayerSpawn(int maxHealth)
    {
        PlayerSpawnEvent?.Invoke(maxHealth);
    }

    public void OnPlayerJump()
    {
        PlayerJumpEvent?.Invoke();
    }

    public void OnPlayerLand()
    {
        PlayerLandEvent?.Invoke();
    }
}
